using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Allows the player to push a crate only between PushStart / PushEnd markers
 * using either a keyboard button or breath input.
 *
 * - Attach this script to the PLAYER.
 * - The crate must have a Rigidbody2D and a tag (e.g. "PushBox").
 * - PushStart / PushEnd are empty GameObjects placed in the scene;
 *   their tags are set in the inspector.
 *
 * IMPORTANT:
 * This script does NOT take a PressureWebSocketReceiver reference from the Inspector.
 * It reads breath from PressureWebSocketReceiver.Instance, so the receiver must exist
 * once in the game (usually created in the Entry scene and marked DontDestroyOnLoad).
 */

public class PushBox : MonoBehaviour
{
    public enum PushControlMode
    {
        Keyboard,
        Breath
    }

    [HideInInspector]
    [Header("Control Mode")]
    [SerializeField] private PushControlMode controlMode = PushControlMode.Keyboard;

    [Header("Input (New Input System)")]
    [Tooltip("Input action used to push the box (Button type).")]
    [SerializeField]
    private InputAction pushAction = new InputAction(type: InputActionType.Button);

    [Header("Tags")]
    [Tooltip("Tag of objects that mark where pushing can start")]
    [SerializeField] private string pushStartTag = "PushStart";

    [Tooltip("Tag of objects that mark where pushing must end")]
    [SerializeField] private string pushEndTag = "PushEnd";

    [Tooltip("Tag of the pushable crate")]
    [SerializeField] private string pushBoxTag = "PushBox";

    [Header("Optional Push Limits")]
    [Tooltip("Maximum horizontal speed for the box when pushing (for safety)")]
    [SerializeField] private float maxBoxSpeedX = 5f;

    [Header("Breath Control")]
    [Tooltip("Breath threshold in kPa to allow pushing")]
    [SerializeField] private float breathThresholdKPa = 1.0f;

    // Push zone markers
    private Transform[] pushStarts;
    private Transform[] pushEnds;

    // The crate we are currently touching (if any)
    private Rigidbody2D currentBoxRb;

    private void Awake()
    {
        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(pushStartTag);
        GameObject[] endObjs = GameObject.FindGameObjectsWithTag(pushEndTag);

        pushStarts = new Transform[startObjs.Length];
        pushEnds = new Transform[endObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            pushStarts[i] = startObjs[i].transform;

        for (int i = 0; i < endObjs.Length; i++)
            pushEnds[i] = endObjs[i].transform;

        if (pushStarts.Length == 0)
            Debug.LogWarning("PushBox: No objects found with tag " + pushStartTag);
        if (pushEnds.Length == 0)
            Debug.LogWarning("PushBox: No objects found with tag " + pushEndTag);
    }

    private void Start()
    {
        if (PressureWebSocketReceiver.Instance == null)
            Debug.LogWarning("PushBox: PressureWebSocketReceiver.Instance is null. Breath mode will not work.");
    }

    private void OnEnable()
    {
        if (controlMode == PushControlMode.Keyboard)
            pushAction.Enable();
    }

    private void OnDisable()
    {
        if (controlMode == PushControlMode.Keyboard)
            pushAction.Disable();
    }

    // Called by InputModeManager to switch between Keyboard/Breath
    public void SetControlMode(bool useBreath)
    {
        PushControlMode newMode = useBreath ? PushControlMode.Breath : PushControlMode.Keyboard;

        if (newMode == controlMode)
            return;

        if (controlMode == PushControlMode.Keyboard)
            pushAction.Disable();

        controlMode = newMode;

        if (isActiveAndEnabled && controlMode == PushControlMode.Keyboard)
            pushAction.Enable();

        Debug.Log("PushBox: Control mode set to " + controlMode);
    }

    private void FixedUpdate()
    {
        if (currentBoxRb == null)
            return;

        bool inAllowedZone = IsInPushZone(transform.position.x);
        bool canPush = false;

        if (controlMode == PushControlMode.Keyboard)
        {
            bool isKeyHeld = pushAction.IsPressed();
            canPush = isKeyHeld && inAllowedZone;
        }
        else if (controlMode == PushControlMode.Breath)
        {
            // Read breath from the global receiver instance (survives scene changes).
            if (PressureWebSocketReceiver.Instance != null)
            {
                float pressure = PressureWebSocketReceiver.Instance.lastPressureKPa;
                bool breathStrong = pressure >= breathThresholdKPa;
                canPush = breathStrong && inAllowedZone;
            }
        }

        if (canPush)
        {
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            Vector2 v = currentBoxRb.linearVelocity;
            v.x = Mathf.Clamp(v.x, -maxBoxSpeedX, maxBoxSpeedX);
            currentBoxRb.linearVelocity = v;
        }
        else
        {
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation |
                                       RigidbodyConstraints2D.FreezePositionX;
        }
    }

    private bool IsInPushZone(float playerX)
    {
        float lastStartX = float.NegativeInfinity;
        float lastEndX = float.NegativeInfinity;

        foreach (Transform s in pushStarts)
        {
            if (s == null) continue;
            if (s.position.x <= playerX && s.position.x > lastStartX)
                lastStartX = s.position.x;
        }

        foreach (Transform e in pushEnds)
        {
            if (e == null) continue;
            if (e.position.x <= playerX && e.position.x > lastEndX)
                lastEndX = e.position.x;
        }

        if (lastStartX == float.NegativeInfinity)
            return false;

        if (lastEndX >= lastStartX)
            return false;

        return true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(pushBoxTag))
            return;

        currentBoxRb = collision.collider.attachedRigidbody;
        if (currentBoxRb != null)
        {
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation |
                                       RigidbodyConstraints2D.FreezePositionX;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (currentBoxRb == null)
            return;

        if (collision.collider.attachedRigidbody == currentBoxRb)
        {
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation |
                                       RigidbodyConstraints2D.FreezePositionX;
            currentBoxRb = null;
        }
    }
}
