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
 */

public class PushBox : MonoBehaviour
{
    public enum PushControlMode
    {
        Keyboard,
        Breath
    }

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
    [Tooltip("Source of breath pressure values (kPa)")]
    [SerializeField] private PressureWebSocketReceiver pressureSource;

    [Tooltip("Breath threshold in kPa to allow pushing")]
    [SerializeField] private float breathThresholdKPa = 1.0f;

    // Push zone markers
    private Transform[] pushStarts;
    private Transform[] pushEnds;

    // The crate we are currently touching (if any)
    private Rigidbody2D currentBoxRb;

    // Find all PushStart / PushEnd markers by tag
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

    // Enable keyboard input only when using keyboard mode
    private void OnEnable()
    {
        if (controlMode == PushControlMode.Keyboard)
        {
            pushAction.Enable();
        }
    }

    // Disable keyboard input when script is disabled
    private void OnDisable()
    {
        if (controlMode == PushControlMode.Keyboard)
        {
            pushAction.Disable();
        }
    }

    // Handle pushing logic in physics update
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
            if (pressureSource != null)
            {
                float pressure = pressureSource.lastPressureKPa;
                bool breathStrong = pressure >= breathThresholdKPa;
                canPush = breathStrong && inAllowedZone;
            }
        }

        if (canPush)
        {
            // Allow movement in X, keep rotation frozen
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Clamp horizontal speed for safety
            Vector2 v = currentBoxRb.linearVelocity;
            v.x = Mathf.Clamp(v.x, -maxBoxSpeedX, maxBoxSpeedX);
            currentBoxRb.linearVelocity = v;
        }
        else
        {
            // Lock X movement when pushing is not allowed
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation |
                                       RigidbodyConstraints2D.FreezePositionX;
        }
    }

    // Check if the player is inside a valid push zone between PushStart and PushEnd
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

    // Detect when the player starts touching a pushable crate
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

    // Detect when the player stops touching the crate
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
