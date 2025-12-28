using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Builds a bridge under the player while a specified input action is held,
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
    [Header("Input")]
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
    // how far below the player feet the bridge should be built
    [SerializeField] private float yOffsetBelowFeet = 0.25f;

    // Arrays to hold references to BridgeStart and BridgeEnd objects
    private Transform[] bridgeStarts;
    private Transform[] bridgeEnds;

    // Dark segments defined by DarkBridgeStart / DarkBridgeEnd pairs
    private readonly List<Vector2> darkSegments = new List<Vector2>(); // (startX, endX)

    private bool isBuilding = false;
    private float currentLength = 0f;
    // Origin point of the player for building the bridge
    private float originX;
    private float originY;
    private float maxLength = Mathf.Infinity;

    private readonly List<GameObject> pieces = new List<GameObject>();

    // Find BridgeStart, BridgeEnd and DarkBridgeStart, DarkBridgeEnd objects in the scene
    private void Awake()
    {
        // --- Build zone markers ---
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

        // --- Dark style segments (optional) ---
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

            // For each DarkBridgeStart find the nearest DarkBridgeEnd ahead on X
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

            // Optional: sort segments by startX (nice for debugging)
            darkSegments.Sort((a, b) => a.x.CompareTo(b.x));
        }
    }

    // Subscribe and unsubscribe to input action events
    private void OnEnable()
    {
        buildBridgeAction.Enable();
        buildBridgeAction.performed += OnBuildPressed;
        buildBridgeAction.canceled += OnBuildReleased;
    }

    private void OnDisable()
    {
        buildBridgeAction.performed -= OnBuildPressed;
        buildBridgeAction.canceled -= OnBuildReleased;
        buildBridgeAction.Disable();
    }

    // Handle build bridge input action pressed
    private void OnBuildPressed(InputAction.CallbackContext ctx)
    {
        if (player == null)
        {
            Debug.LogWarning("BridgeBuilder: Player reference is not assigned");
            return;
        }

        float playerX = player.position.x;

        // Find the last BridgeStart behind the player
        float lastStartX = float.NegativeInfinity;
        foreach (Transform s in bridgeStarts)
        {
            if (s == null) continue;
            if (s.position.x <= playerX && s.position.x > lastStartX)
                lastStartX = s.position.x;
        }

        // Find the last BridgeEnd behind the player
        float lastEndX = float.NegativeInfinity;
        foreach (Transform e in bridgeEnds)
        {
            if (e == null) continue;
            if (e.position.x <= playerX && e.position.x > lastEndX)
                lastEndX = e.position.x;
        }

        // If no BridgeStart has been passed yet - cannot build
        if (lastStartX == float.NegativeInfinity)
        {
            Debug.Log("BridgeBuilder: Player has not passed any BridgeStart yet");
            return;
        }

        // If the most recent marker behind the player is a BridgeEnd - cannot build
        if (lastEndX >= lastStartX)
        {
            Debug.Log("BridgeBuilder: Player already passed the last BridgeEnd — cannot build here");
            return;
        }

        // At this point the last marker is a BridgeStart - OK to build
        // Set bridge origin from player's current position
        originX = player.position.x;
        // build the bridge slightly below player feet
        originY = player.position.y - yOffsetBelowFeet;

        currentLength = 0f;
        ClearBridgePieces();

        // Find the nearest BridgeEnd ahead of the player
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
            maxLength = Mathf.Infinity; // no BridgeEnd ahead

        isBuilding = true;
        Debug.Log("BridgeBuilder: Started building bridge for this gap");
    }

    // Handle build bridge input action released
    private void OnBuildReleased(InputAction.CallbackContext ctx)
    {
        isBuilding = false;
        Debug.Log("BridgeBuilder: Stopped building bridge");
    }

    // Build bridge pieces while the build action is held
    private void Update()
    {
        if (!isBuilding || bridgePiecePrefab == null || pieceWidth <= 0f)
            return;

        currentLength += buildSpeed * Time.deltaTime;

        // Stop building at the maximum allowed length
        if (currentLength >= maxLength)
        {
            currentLength = maxLength;
            isBuilding = false;
        }

        int neededPieces = Mathf.FloorToInt(currentLength / pieceWidth);
        // Instantiate new pieces as needed
        while (pieces.Count < neededPieces)
        {
            float x = originX + pieces.Count * pieceWidth;
            float y = originY;

            Vector3 pos = new Vector3(x, y, 0f);
            InstantiatePiece(pos);
        }
    }

    // Returns true if the given x is inside any dark segment
    private bool IsInDarkSegment(float x)
    {
        // If no dark segments defined, always false
        if (darkSegments.Count == 0)
            return false;

        foreach (var seg in darkSegments)
        {
            if (x >= seg.x && x < seg.y)
                return true;
        }
        return false;
    }

    // Instantiates a bridge piece at the specified position
    private void InstantiatePiece(Vector3 pos)
    {
        GameObject prefabToUse = bridgePiecePrefab;

        // If we have a dark prefab and this x is within a dark segment - use it
        if (darkBridgePiecePrefab != null && IsInDarkSegment(pos.x))
        {
            prefabToUse = darkBridgePiecePrefab;
        }

        GameObject piece = Instantiate(prefabToUse, pos, Quaternion.identity);
        pieces.Add(piece);
        Debug.Log("BridgeBuilder: Bridge piece placed at " + pos + " (dark=" + (prefabToUse == darkBridgePiecePrefab) + ")");
    }

    // Destroys all instantiated bridge pieces
    private void ClearBridgePieces()
    {
        foreach (GameObject g in pieces)
            if (g != null)
                Destroy(g);

        pieces.Clear();
    }
}
