using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Moves an object while the input is held (Keyboard)
 * or while breath pressure is above a threshold (Breath).
 *
 * IMPORTANT:
 * - Breath input is read from PressureWebSocketReceiver.Instance (singleton).
 * - Do not assign any pressure source in the Inspector.
 */

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
    [SerializeField] InputAction blowButton = new InputAction(type: InputActionType.Button);

    [Header("Breath Control")]
    [Tooltip("Breath threshold in kPa to start movement")]
    [SerializeField] private float breathThresholdKPa = 1.0f;

    private bool isBlowing = false;

    void Awake()
    {
        // Auto-fetch AudioSource if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
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

        StopBlow();
    }

    //To change between keyboard and breath
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

    private float GetPressureKPa()
    {
        if (PressureWebSocketReceiver.Instance == null)
            return 0f;

        return PressureWebSocketReceiver.Instance.lastPressureKPa;
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
        if (isBlowing) return;
        isBlowing = true;

        // Start sound
        if (audioSource != null && moveSound != null)
        {
            audioSource.clip = moveSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopBlow()
    {
        isBlowing = false;

        // Stop sound
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    void Update()
    {
        if (controlMode == BlowControlMode.Breath)
        {
            UpdateBreathControl();
        }

        if (isBlowing)
        {
            transform.position += direction.normalized * speed * Time.deltaTime;
        }
    }

    private void UpdateBreathControl()
    {
        float pressure = GetPressureKPa();
        bool breathStrong = pressure >= breathThresholdKPa;

        if (breathStrong && !isBlowing)
            StartBlow();
        else if (!breathStrong && isBlowing)
            StopBlow();
    }
}
