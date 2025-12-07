using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Makes the player rise while the jump key is held,
 * BUT ONLY when the player is inside a "jump zone"
 * between objects tagged JumpStart and JumpEnd.
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
    [SerializeField] private string jumpEndTag = "JumpEnd";

    // arrays of all JumpStart / JumpEnd markers in the scene
    private Transform[] jumpStarts;
    private Transform[] jumpEnds;

    // true only while key is held *and* we are in a valid jump zone
    private bool isHeld = false;

    private void Awake()
    {
        // Find all JumpStart and JumpEnd objects by tag
        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(jumpStartTag);
        GameObject[] endObjs = GameObject.FindGameObjectsWithTag(jumpEndTag);

        jumpStarts = new Transform[startObjs.Length];
        jumpEnds = new Transform[endObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            jumpStarts[i] = startObjs[i].transform;

        for (int i = 0; i < endObjs.Length; i++)
            jumpEnds[i] = endObjs[i].transform;

        if (jumpStarts.Length == 0)
            Debug.LogWarning("Jump: No objects found with tag " + jumpStartTag);

        if (jumpEnds.Length == 0)
            Debug.LogWarning("Jump: No objects found with tag " + jumpEndTag);
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
        // Enable jumping only if inside a jump zone
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

        // Be sure to check both conditions
        if (isHeld && IsInsideJumpZone())
        {
            // While the key is held and we're in a jump zone, move the player up
            v.y = riseSpeed;
            animator.SetBool("isJumping", true);
        }
        else
        {
            // When key not held or not in zone – let gravity act
            animator.SetBool("isJumping", false);
        }

        rigidBody.linearVelocity = v;
    }


    // Returns true if the player is between a JumpStart behind them
    // and the closest JumpEnd in front of them (same "segment" logic as the bridge).
    private bool IsInsideJumpZone()
    {
        if (jumpStarts == null || jumpStarts.Length == 0 ||
            jumpEnds == null || jumpEnds.Length == 0)
            return false;

        float playerX = transform.position.x;

        // Find the last JumpStart behind (<= playerX)
        float lastStartX = float.NegativeInfinity;
        foreach (Transform s in jumpStarts)
        {
            if (s == null) continue;
            if (s.position.x <= playerX && s.position.x > lastStartX)
                lastStartX = s.position.x;
        }

        // If we didn't pass any JumpStart yet – not allowed
        if (lastStartX == float.NegativeInfinity)
            return false;

        // Find the closest JumpEnd *ahead or at* playerX
        float nextEndX = float.PositiveInfinity;
        foreach (Transform e in jumpEnds)
        {
            if (e == null) continue;
            if (e.position.x >= playerX && e.position.x < nextEndX)
                nextEndX = e.position.x;
        }

        // If there's no JumpEnd ahead – אין אזור מוגדר
        if (nextEndX == float.PositiveInfinity)
            return false;

        // We are between JumpStart and JumpEnd
        return playerX >= lastStartX && playerX <= nextEndX;
    }
}
