using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Builds a bridge under the player while a specified input action is held,
 * provided the player has passed a BridgeStart marker but not yet a BridgeEnd marker.
 * Bridge pieces are instantiated at regular intervals to form the bridge.
 */

public class BridgeBuilder : MonoBehaviour
{
    [Header("Input")]
    [SerializeField]
    private InputAction buildBridgeAction = new InputAction(type: InputActionType.Button);

    [Header("Tags")]
    [SerializeField] private string bridgeStartTag = "BridgeStart";
    [SerializeField] private string bridgeEndTag = "BridgeEnd";

    [Header("References")]
    [SerializeField] private GameObject bridgePiecePrefab;
    [SerializeField] private Transform player;

    [Header("Bridge Settings")]
    [SerializeField] private float pieceWidth = 0.5f;
    [SerializeField] private float buildSpeed = 2f;
    // how far below the player feet the bridge should be built
    [SerializeField] private float yOffsetBelowFeet = 0.25f;

    // Arrays to hold references to BridgeStart and BridgeEnd objects
    private Transform[] bridgeStarts;
    private Transform[] bridgeEnds;

    private bool isBuilding = false;
    private float currentLength = 0f;
    // Origin point of the player for building the bridge
    private float originX;
    private float originY;
    private float maxLength = Mathf.Infinity;

    private readonly List<GameObject> pieces = new List<GameObject>();

    // Find BridgeStart and BridgeEnd objects in the scene
    private void Awake()
    {
        // Find all BridgeStart and BridgeEnd objects by tag
        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(bridgeStartTag);
        GameObject[] endObjs = GameObject.FindGameObjectsWithTag(bridgeEndTag);

        bridgeStarts = new Transform[startObjs.Length];
        bridgeEnds = new Transform[endObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            bridgeStarts[i] = startObjs[i].transform;

        for (int i = 0; i < endObjs.Length; i++)
            bridgeEnds[i] = endObjs[i].transform;

        if (bridgeStarts.Length == 0)
            Debug.LogWarning("BrigeBuilder: No objects found with tag " + bridgeStartTag);

        if (bridgeEnds.Length == 0)
            Debug.LogWarning("BrigeBuilder: No objects found with tag " + bridgeEndTag);
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
            Debug.LogWarning("BrigeBuilder: Player reference is not assigned");
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
            Debug.Log("BrigeBuilder: Player has not passed any BridgeStart yet");
            return;
        }

        // If the most recent marker behind the player is a BridgeEnd - cannot build
        if (lastEndX >= lastStartX)
        {
            Debug.Log("BrigeBuilder: Player already passed the last BridgeEnd — cannot build here");
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
        Debug.Log("BrigeBuilder: Started building bridge for this gap");
    }

    // Handle build bridge input action released1
    private void OnBuildReleased(InputAction.CallbackContext ctx)
    {
        isBuilding = false;
        Debug.Log("BrigeBuilder: Stopped building bridge");
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

    // Instantiates a bridge piece at the specified position
    private void InstantiatePiece(Vector3 pos)
    {
        GameObject piece = Instantiate(bridgePiecePrefab, pos, Quaternion.identity);
        pieces.Add(piece);
        Debug.Log("BrigeBuilder: Bridge piece placed at " + pos);
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
