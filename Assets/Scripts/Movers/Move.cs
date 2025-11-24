using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Moves the player forward while the right arrow key is held.
 */

public class Move : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] Vector3 direction;
    [SerializeField] Animator animator;
    [SerializeField] InputAction startMove = new InputAction(type: InputActionType.Button); // Enter arrow key
    private bool isPressed = false;

    // Subscribe and unsubscribe to input action events
    void OnEnable()
    {
        startMove.Enable();
        startMove.performed += OnWalkPressed;
    }

    void OnDisable()
    {
        startMove.performed -= OnWalkPressed;
        startMove.Disable();
    }

    private void OnWalkPressed(InputAction.CallbackContext ctx)
    {
        isPressed = true;
    }

    void Update()
    {
        //Move the player on direction while the key is held
        if (isPressed)
        {
            animator.SetBool("isWalking", true);
            transform.position += direction.normalized * speed * Time.deltaTime;
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}
