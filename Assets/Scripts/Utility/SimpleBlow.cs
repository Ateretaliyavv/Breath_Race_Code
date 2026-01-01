using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleBlow : MonoBehaviour
{
    public enum BlowControlMode
    {
        Keyboard,
        Breath
    }

    [Header("Control Mode")]
    [SerializeField] private BlowControlMode controlMode = BlowControlMode.Keyboard;

    [SerializeField] float speed = 5f;
    [SerializeField] Vector3 direction;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;

    [Header("Keyboard Input")]
    [SerializeField] InputAction blowButton = new InputAction(type: InputActionType.Button); // Enter arrow key

    [Header("Breath Control")]
    [Tooltip("Source of breath pressure values (kPa)")]
    [SerializeField] private PressureWebSocketReceiver pressureSource;
    [Tooltip("Breath threshold in kPa to start movement")]
    [SerializeField] private float breathThresholdKPa = 1.0f;

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
        if (controlMode == BlowControlMode.Keyboard)
        {
            blowButton.Enable();
            blowButton.performed += OnBlowPressed;
            blowButton.canceled += OnBlowReleased;
        }
    }

    void OnDisable()
    {
        if (controlMode == BlowControlMode.Keyboard)
        {
            blowButton.performed -= OnBlowPressed;
            blowButton.canceled -= OnBlowReleased;
            blowButton.Disable();
        }

        // Stop sound if script is disabled while blowing
        if (audioSource != null) audioSource.Stop();
        isBlowing = false;
    }

    //To Chage between keybord and breath
    public void SetControlMode(bool useBreath)
    {
        BlowControlMode newMode = useBreath ? BlowControlMode.Breath : BlowControlMode.Keyboard;

        if (newMode == controlMode)
            return;

        // Clean up old mode
        if (controlMode == BlowControlMode.Keyboard)
        {
            blowButton.performed -= OnBlowPressed;
            blowButton.canceled -= OnBlowReleased;
            blowButton.Disable();
        }

        controlMode = newMode;

        // Init new mode (keyboard)
        if (isActiveAndEnabled && controlMode == BlowControlMode.Keyboard)
        {
            blowButton.Enable();
            blowButton.performed += OnBlowPressed;
            blowButton.canceled += OnBlowReleased;
        }

        // Reset state
        StopBlow();

        Debug.Log("SimpleBlow: Control mode set to " + controlMode);
    }

    private void OnBlowPressed(InputAction.CallbackContext ctx)
    {
        if (controlMode != BlowControlMode.Keyboard)
            return;

        StartBlow();
    }

    private void OnBlowReleased(InputAction.CallbackContext ctx)
    {
        if (controlMode != BlowControlMode.Keyboard)
            return;

        StopBlow();
    }

    private void StartBlow()
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

    private void StopBlow()
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
        // במצב נשיפה – בודק לחץ מהחיישן ומחליט אם "לנשוף"
        if (controlMode == BlowControlMode.Breath)
        {
            UpdateBreathControl();
        }

        if (isBlowing)
        {
            // Move the object in the specified direction at the specified speed
            transform.position += direction.normalized * speed * Time.deltaTime;
        }
    }

    private void UpdateBreathControl()
    {
        if (pressureSource == null)
        {
            StopBlow();
            return;
        }

        float pressure = pressureSource.lastPressureKPa;
        bool breathStrong = pressure >= breathThresholdKPa;

        if (breathStrong && !isBlowing)
        {
            StartBlow();
        }
        else if (!breathStrong && isBlowing)
        {
            StopBlow();
        }
    }
}
