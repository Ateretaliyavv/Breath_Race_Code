using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Makes the player rise while the jump key is held,
 * BUT ONLY when inside a jump zone defined between a JumpStart and its child JumpEnd.
 *
 * Now supports:
 * - Keyboard OR Breath control (selectable in Inspector)
 * - 3 breath jump strength levels (low / medium / high), each with configurable
 *   threshold (kPa) and vertical speed.
 */

public class Jump : MonoBehaviour
{
    public enum JumpControlMode
    {
        Keyboard,
        Breath
    }

    [Header("Control Mode")]
    [SerializeField] private JumpControlMode controlMode = JumpControlMode.Keyboard;

    [Header("Components")]
    [SerializeField] Rigidbody2D rigidBody;
    [SerializeField] Animator animator;

    [Header("Keyboard Input")]
    [SerializeField] InputAction jumpButton = new InputAction(type: InputActionType.Button);

    [Header("Breath Input (kPa)")]
    [SerializeField] private PressureReaderFromSerial pressureSource;
    [SerializeField] private float lowThresholdKPa = 1.0f;
    [SerializeField] private float mediumThresholdKPa = 2.0f;
    [SerializeField] private float highThresholdKPa = 3.5f;

    [Header("Breath Speeds")]
    [SerializeField] private float lowJumpSpeed = 2f;
    [SerializeField] private float mediumJumpSpeed = 4f;
    [SerializeField] private float highJumpSpeed = 7f;

    [Header("Jump Zone Tags")]
    [SerializeField] private string jumpStartTag = "JumpStart";
    [SerializeField] private string jumpEndTag = "JumpEnd";

    private Transform[] jumpStarts;
    private bool isHeld = false;

    private void Awake()
    {
        // Find all JumpStart markers
        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(jumpStartTag);
        jumpStarts = new Transform[startObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            jumpStarts[i] = startObjs[i].transform;

        if (jumpStarts.Length == 0)
            Debug.LogWarning("Jump: No objects found with tag " + jumpStartTag);
    }

    private void OnEnable()
    {
        if (controlMode == JumpControlMode.Keyboard)
        {
            jumpButton.Enable();
            jumpButton.performed += OnJumpPressed;
            jumpButton.canceled += OnJumpReleased;
        }
    }

    private void OnDisable()
    {
        if (controlMode == JumpControlMode.Keyboard)
        {
            jumpButton.performed -= OnJumpPressed;
            jumpButton.canceled -= OnJumpReleased;
            jumpButton.Disable();
        }

        isHeld = false;
    }

    // Keyboard press event
    private void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        if (IsInsideJumpZone())
        {
            isHeld = true;
        }
        else
        {
            isHeld = false;
        }
    }

    // Keyboard release event
    private void OnJumpReleased(InputAction.CallbackContext ctx)
    {
        isHeld = false;
    }

    // Physics update for vertical movement
    private void FixedUpdate()
    {
        Vector2 v = rigidBody.linearVelocity;

        if (controlMode == JumpControlMode.Keyboard)
        {
            HandleKeyboardJump(ref v);
        }
        else if (controlMode == JumpControlMode.Breath)
        {
            HandleBreathJump(ref v);
        }

        rigidBody.linearVelocity = v;
    }

    // Keyboard jump logic
    private void HandleKeyboardJump(ref Vector2 v)
    {
        if (isHeld && IsInsideJumpZone())
        {
            v.y = mediumJumpSpeed;  // default keyboard jump uses only medium
            animator.SetBool("isJumping", true);
        }
        else
        {
            animator.SetBool("isJumping", false);
        }
    }

    // Breath jump logic with 3 levels
    private void HandleBreathJump(ref Vector2 v)
    {
        if (pressureSource == null)
        {
            animator.SetBool("isJumping", false);
            return;
        }

        if (!IsInsideJumpZone())
        {
            animator.SetBool("isJumping", false);
            return;
        }

        float pressure = pressureSource.lastPressureKPa;
        float selectedSpeed = 0f;

        // Determine jump strength based on breath level
        if (pressure >= highThresholdKPa)
            selectedSpeed = highJumpSpeed;
        else if (pressure >= mediumThresholdKPa)
            selectedSpeed = mediumJumpSpeed;
        else if (pressure >= lowThresholdKPa)
            selectedSpeed = lowJumpSpeed;
        else
            selectedSpeed = 0f;

        if (selectedSpeed > 0f)
        {
            v.y = selectedSpeed;
            animator.SetBool("isJumping", true);
        }
        else
        {
            animator.SetBool("isJumping", false);
        }
    }

    // Check if the player is inside any jump zone
    private bool IsInsideJumpZone()
    {
        if (jumpStarts == null || jumpStarts.Length == 0)
            return false;

        float playerX = transform.position.x;

        foreach (Transform start in jumpStarts)
        {
            if (start == null)
                continue;

            Transform end = null;

            // Find child with jumpEndTag
            for (int i = 0; i < start.childCount; i++)
            {
                Transform child = start.GetChild(i);
                if (child.CompareTag(jumpEndTag))
                {
                    end = child;
                    break;
                }
            }

            if (end == null)
            {
                // If no tagged child, fallback: use first child
                if (start.childCount > 0)
                    end = start.GetChild(0);
                else
                {
                    Debug.LogWarning("Jump: JumpStart " + start.name + " has no JumpEnd child");
                    continue;
                }
            }

            float x1 = start.position.x;
            float x2 = end.position.x;

            float minX = Mathf.Min(x1, x2);
            float maxX = Mathf.Max(x1, x2);

            if (playerX >= minX && playerX <= maxX)
                return true;
        }

        return false;
    }
}
