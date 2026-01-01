using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleBlow : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] Vector3 direction;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;

    [SerializeField] InputAction blowButton = new InputAction(type: InputActionType.Button); // Enter arrow key
    private bool isBlowing = false;

    void Awake()
    {
        // Auto-fetch AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

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

        // Stop sound if script is disabled while blowing
        if (audioSource != null) audioSource.Stop();
    }

    private void OnBlowPressed(InputAction.CallbackContext ctx)
    {
        isBlowing = true;

        // --- Start Playing Sound ---
        if (audioSource != null && moveSound != null)
        {
            audioSource.clip = moveSound;
            audioSource.loop = true; // Enable loop so it plays continuously
            audioSource.Play();
        }
    }

    private void OnBlowReleased(InputAction.CallbackContext ctx)
    {
        isBlowing = false;

        // --- Stop Playing Sound ---
        if (audioSource != null)
        {
            audioSource.loop = false; // Optional: reset loop state
            audioSource.Stop();
        }
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
