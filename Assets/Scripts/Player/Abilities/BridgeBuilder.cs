using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Builds a bridge under the player while an input is held,
 * provided the player has passed a BridgeStart marker but not yet a BridgeEnd marker.
 * Bridge pieces are instantiated at regular intervals to form the bridge.
 *
 * Optional:
 *  - Between DarkBridgeStart and DarkBridgeEnd markers the bridge pieces
 *    can use an alternative "dark" prefab, and outside of these zones
 *    the regular prefab is used.
 */

public class BridgeBuilder : MonoBehaviour
{
    public enum BridgeControlMode
    {
        Keyboard,
        Breath
    }

    [HideInInspector]
    [Header("Control Mode")]
    [SerializeField] private BridgeControlMode controlMode = BridgeControlMode.Keyboard;

    [Header("Input (Keyboard)")]
    [SerializeField]
    private InputAction buildBridgeAction = new InputAction(type: InputActionType.Button);

    [Header("Tags (Build Zones)")]
    [SerializeField] private string bridgeStartTag = "BridgeStart";
    [SerializeField] private string bridgeEndTag = "BridgeEnd";

    [Header("Bridge Piece Prefabs")]
    [SerializeField] private GameObject bridgePiecePrefab;      // default prefab
    [Tooltip("Optional: alternative prefab for dark bridge segments")]
    [SerializeField] private GameObject darkBridgePiecePrefab;  // dark prefab (optional)

    [Header("Tags (Dark Style Zones - Optional)")]
    [Tooltip("Tag for the start of a dark bridge segment (optional)")]
    [SerializeField] private string darkBridgeStartTag = "DarkBridgeStart";
    [Tooltip("Tag for the end of a dark bridge segment (optional)")]
    [SerializeField] private string darkBridgeEndTag = "DarkBridgeEnd";

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Bridge Settings")]
    [SerializeField] private float pieceWidth = 0.5f;
    [SerializeField] private float buildSpeed = 2f;
    [SerializeField] private float yOffsetBelowFeet = 0.25f;

    [Header("Breath Control")]
    [Tooltip("Source of breath pressure values (kPa)")]
    [SerializeField] private PressureWebSocketReceiver pressureSource;
    [Tooltip("Breath threshold in kPa to start building")]
    [SerializeField] private float breathThresholdKPa = 1.0f;

    // Build zone markers
    private Transform[] bridgeStarts;
    private Transform[] bridgeEnds;

    // Dark segments defined by DarkBridgeStart / DarkBridgeEnd pairs
    private readonly List<Vector2> darkSegments = new List<Vector2>();

    // Build state
    private bool isBuilding = false;
    private float currentLength = 0f;
    private float originX;
    private float originY;
    private float maxLength = Mathf.Infinity;

    // Spawned pieces
    private readonly List<GameObject> pieces = new List<GameObject>();

    // Breath edge detection
    private bool wasBreathStrong = false;

    // Find all markers and build dark segments list
    private void Awake()
    {
        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(bridgeStartTag);
        GameObject[] endObjs = GameObject.FindGameObjectsWithTag(bridgeEndTag);

        bridgeStarts = new Transform[startObjs.Length];
        bridgeEnds = new Transform[endObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            bridgeStarts[i] = startObjs[i].transform;

        for (int i = 0; i < endObjs.Length; i++)
            bridgeEnds[i] = endObjs[i].transform;

        if (bridgeStarts.Length == 0)
            Debug.LogWarning("BridgeBuilder: No objects found with tag " + bridgeStartTag);

        if (bridgeEnds.Length == 0)
            Debug.LogWarning("BridgeBuilder: No objects found with tag " + bridgeEndTag);

        // Dark style segments (optional)
        if (darkBridgePiecePrefab != null &&
            !string.IsNullOrEmpty(darkBridgeStartTag) &&
            !string.IsNullOrEmpty(darkBridgeEndTag))
        {
            GameObject[] darkStartObjs = GameObject.FindGameObjectsWithTag(darkBridgeStartTag);
            GameObject[] darkEndObjs = GameObject.FindGameObjectsWithTag(darkBridgeEndTag);

            List<Transform> darkStarts = new List<Transform>();
            List<Transform> darkEnds = new List<Transform>();

            foreach (var go in darkStartObjs)
                if (go != null) darkStarts.Add(go.transform);

            foreach (var go in darkEndObjs)
                if (go != null) darkEnds.Add(go.transform);

            foreach (var s in darkStarts)
            {
                float startX = s.position.x;
                float closestEndX = float.PositiveInfinity;

                foreach (var e in darkEnds)
                {
                    float endX = e.position.x;
                    if (endX > startX && endX < closestEndX)
                        closestEndX = endX;
                }

                if (closestEndX < float.PositiveInfinity)
                {
                    darkSegments.Add(new Vector2(startX, closestEndX));
                }
                else
                {
                    Debug.LogWarning($"BridgeBuilder: DarkBridgeStart at x={startX} has no DarkBridgeEnd ahead of it.");
                }
            }

            darkSegments.Sort((a, b) => a.x.CompareTo(b.x));
        }
    }

    // Enable keyboard input only when using keyboard mode
    private void OnEnable()
    {
        if (controlMode == BridgeControlMode.Keyboard)
        {
            buildBridgeAction.Enable();
            buildBridgeAction.performed += OnBuildPressed;
            buildBridgeAction.canceled += OnBuildReleased;
        }
    }

    // Disable keyboard input when script is disabled
    private void OnDisable()
    {
        if (controlMode == BridgeControlMode.Keyboard)
        {
            buildBridgeAction.performed -= OnBuildPressed;
            buildBridgeAction.canceled -= OnBuildReleased;
            buildBridgeAction.Disable();
        }
    }

    // Called by InputModeManager to switch between Keyboard/Breath
    public void SetControlMode(bool useBreath)
    {
        BridgeControlMode newMode = useBreath ? BridgeControlMode.Breath : BridgeControlMode.Keyboard;

        if (newMode == controlMode)
            return;

        // Clean up old mode
        if (controlMode == BridgeControlMode.Keyboard)
        {
            buildBridgeAction.performed -= OnBuildPressed;
            buildBridgeAction.canceled -= OnBuildReleased;
            buildBridgeAction.Disable();
        }

        controlMode = newMode;

        // Initialize new mode (keyboard actions)
        if (isActiveAndEnabled && controlMode == BridgeControlMode.Keyboard)
        {
            buildBridgeAction.Enable();
            buildBridgeAction.performed += OnBuildPressed;
            buildBridgeAction.canceled += OnBuildReleased;
        }

        // Reset state when switching mode
        isBuilding = false;
        currentLength = 0f;
        maxLength = Mathf.Infinity;
        wasBreathStrong = false;
        ClearBridgePieces();

        Debug.Log("BridgeBuilder: Control mode set to " + controlMode);
    }

    // Try to start building at the player position (used by both control modes)
    private bool TryStartBuilding()
    {
        if (player == null)
        {
            Debug.LogWarning("BridgeBuilder: Player reference is not assigned");
            return false;
        }

        float playerX = player.position.x;

        float lastStartX = float.NegativeInfinity;
        foreach (Transform s in bridgeStarts)
        {
            if (s == null) continue;
            if (s.position.x <= playerX && s.position.x > lastStartX)
                lastStartX = s.position.x;
        }

        float lastEndX = float.NegativeInfinity;
        foreach (Transform e in bridgeEnds)
        {
            if (e == null) continue;
            if (e.position.x <= playerX && e.position.x > lastEndX)
                lastEndX = e.position.x;
        }

        if (lastStartX == float.NegativeInfinity)
        {
            Debug.Log("BridgeBuilder: Player has not passed any BridgeStart yet");
            return false;
        }

        if (lastEndX >= lastStartX)
        {
            Debug.Log("BridgeBuilder: Player already passed the last BridgeEnd — cannot build here");
            return false;
        }

        originX = player.position.x;
        originY = player.position.y - yOffsetBelowFeet;

        currentLength = 0f;
        ClearBridgePieces();

        float closestEndX = float.PositiveInfinity;
        foreach (Transform e in bridgeEnds)
        {
            if (e == null) continue;
            if (e.position.x > originX && e.position.x < closestEndX)
                closestEndX = e.position.x;
        }

        if (closestEndX < float.PositiveInfinity)
            maxLength = Mathf.Abs(closestEndX - originX);
        else
            maxLength = Mathf.Infinity;

        Debug.Log("BridgeBuilder: Started building bridge for this gap");
        return true;
    }

    // Handle keyboard press event
    private void OnBuildPressed(InputAction.CallbackContext ctx)
    {
        if (controlMode != BridgeControlMode.Keyboard)
            return;

        if (!isBuilding)
        {
            if (!TryStartBuilding())
                return;
        }

        isBuilding = true;
    }

    // Handle keyboard release event
    private void OnBuildReleased(InputAction.CallbackContext ctx)
    {
        if (controlMode != BridgeControlMode.Keyboard)
            return;

        isBuilding = false;
        currentLength = 0f;
    }

    // Main update: handles breath input (if selected) and builds pieces over time
    private void Update()
    {
        if (controlMode == BridgeControlMode.Breath)
        {
            UpdateBreathControl();
        }

        if (!isBuilding || bridgePiecePrefab == null || pieceWidth <= 0f)
            return;

        currentLength += buildSpeed * Time.deltaTime;

        if (currentLength >= maxLength)
        {
            currentLength = maxLength;
            isBuilding = false;
        }

        int neededPieces = Mathf.FloorToInt(currentLength / pieceWidth);
        while (pieces.Count < neededPieces)
        {
            float x = originX + pieces.Count * pieceWidth;
            float y = originY;

            Vector3 pos = new Vector3(x, y, 0f);
            InstantiatePiece(pos);
        }
    }

    // Handle breath input when using breath control mode
    private void UpdateBreathControl()
    {
        if (pressureSource == null)
            return;

        float pressure = pressureSource.lastPressureKPa;
        bool breathStrong = pressure >= breathThresholdKPa;

        if (breathStrong && !wasBreathStrong)
        {
            if (!isBuilding)
            {
                if (!TryStartBuilding())
                    breathStrong = false;
                else
                    isBuilding = true;
            }
        }

        if (!breathStrong && wasBreathStrong)
        {
            isBuilding = false;
            currentLength = 0f;
        }

        wasBreathStrong = breathStrong;
    }

    // Check if an x position is inside any dark segment
    private bool IsInDarkSegment(float x)
    {
        if (darkSegments.Count == 0)
            return false;

        foreach (var seg in darkSegments)
        {
            if (x >= seg.x && x < seg.y)
                return true;
        }
        return false;
    }

    // Instantiate one bridge piece at the given position
    private void InstantiatePiece(Vector3 pos)
    {
        GameObject prefabToUse = bridgePiecePrefab;

        if (darkBridgePiecePrefab != null && IsInDarkSegment(pos.x))
            prefabToUse = darkBridgePiecePrefab;

        GameObject piece = Instantiate(prefabToUse, pos, Quaternion.identity);
        pieces.Add(piece);
    }

    // Destroy all spawned bridge pieces
    private void ClearBridgePieces()
    {
        foreach (GameObject g in pieces)
            if (g != null)
                Destroy(g);

        pieces.Clear();
    }
}
