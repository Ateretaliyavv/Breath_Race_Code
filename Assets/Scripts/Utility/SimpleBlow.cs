using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleBlow : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] Vector3 direction;

    [SerializeField] InputAction blowButton = new InputAction(type: InputActionType.Button); // Enter arrow key
    private bool isBlowing = false;

    void OnEnable()
    {
        blowButton.Enable();
        blowButton.performed += OnBlowPressed;
        blowButton.canceled += OnBlowReleased;
    }

    void OnDisable()
    {
        blowButton.performed -= OnBlowPressed;
        blowButton.canceled -= OnBlowReleased;
        blowButton.Disable();
    }

    private void OnBlowPressed(InputAction.CallbackContext ctx)
    {
        isBlowing = true;
    }

    private void OnBlowReleased(InputAction.CallbackContext ctx)
    {
        isBlowing = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isBlowing)
        {
            // Move the object in the specified direction at the specified speed
            transform.position += direction.normalized * speed * Time.deltaTime;
        }
    }
}
