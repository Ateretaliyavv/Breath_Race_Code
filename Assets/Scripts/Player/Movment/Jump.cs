using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Makes the player rise while the jump key is held,
 * BUT ONLY when the player is inside a "jump zone"
 * defined between a JumpStart and its child JumpEnd.
 */
public class Jump : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Rigidbody2D rigidBody;
    [SerializeField] Animator animator;

    [Header("Input")]
    [SerializeField] InputAction jumpButton = new InputAction(type: InputActionType.Button);

    [Header("Jump Settings")]
    [SerializeField] float riseSpeed = 4f;

    [Header("Jump Zone Tags")]
    [SerializeField] private string jumpStartTag = "JumpStart";
    [SerializeField] private string jumpEndTag = "JumpEnd"; // optional, used to find the child

    // All JumpStart transforms in the scene
    private Transform[] jumpStarts;

    // true only while key is held and we are in a valid jump zone
    private bool isHeld = false;

    private void Awake()
    {
        // Find all JumpStart objects by tag
        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(jumpStartTag);
        jumpStarts = new Transform[startObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            jumpStarts[i] = startObjs[i].transform;

        if (jumpStarts.Length == 0)
            Debug.LogWarning("Jump: No objects found with tag " + jumpStartTag);
    }

    void OnEnable()
    {
        jumpButton.Enable();
        jumpButton.performed += OnJumpPressed;
        jumpButton.canceled += OnJumpReleased;
    }

    void OnDisable()
    {
        jumpButton.performed -= OnJumpPressed;
        jumpButton.canceled -= OnJumpReleased;
        jumpButton.Disable();
    }

    private void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        if (IsInsideJumpZone())
        {
            isHeld = true;
            Debug.Log("Jump: Jump started inside jump zone");
        }
        else
        {
            isHeld = false;
            Debug.Log("Jump: Player is NOT inside a jump zone - jump ignored");
        }
    }

    private void OnJumpReleased(InputAction.CallbackContext ctx)
    {
        isHeld = false;
    }

    void FixedUpdate()
    {
        Vector2 v = rigidBody.linearVelocity;

        if (isHeld && IsInsideJumpZone())
        {
            v.y = riseSpeed;
            animator.SetBool("isJumping", true);
        }
        else
        {
            animator.SetBool("isJumping", false);
        }

        rigidBody.linearVelocity = v;
    }

    // Returns true if the player is inside any jump zone.
    // Each jump zone is defined by a JumpStart and its child JumpEnd.
    private bool IsInsideJumpZone()
    {
        if (jumpStarts == null || jumpStarts.Length == 0)
            return false;

        float playerX = transform.position.x;

        foreach (Transform start in jumpStarts)
        {
            if (start == null)
                continue;

            // Find the corresponding JumpEnd: either the first child,
            // or specifically the child with the jumpEndTag.
            Transform end = null;

            // Option A: first child is the end point
            if (start.childCount > 0)
                end = start.GetChild(0);

            // Option B (safer): look for a child with tag jumpEndTag
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
                Debug.LogWarning("Jump: JumpStart " + start.name + " has no JumpEnd child");
                continue;
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
