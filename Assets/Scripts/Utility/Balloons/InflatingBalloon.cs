using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Script that handles balloon inflation via button press or breath.
 * - Plays an "Inflate" sound on every step.
 */
public class InflatingBalloon : MonoBehaviour
{
    public enum InflateControlMode
    {
        Keyboard,
        Breath
    }

    [Header("Control Mode")]
    [SerializeField] private InflateControlMode controlMode = InflateControlMode.Keyboard;

    [Header("Input")]
    [SerializeField] private InputAction inflatingButton;

    [Header("Inflation Settings")]
    [Tooltip("How much to add to X & Y scale on each press")]
    [SerializeField] private float scaleStep = 0.1f;
    [Tooltip("Maximum scale limit (optional)")]
    [SerializeField] private float maxScale = 3.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip inflateSound; // Sound for "Puff"

    [Header("Breath Control")]
    [Tooltip("Source of breath pressure values (kPa)")]
    [SerializeField] private PressureWebSocketReceiver pressureSource;
    [Tooltip("Breath threshold in kPa to trigger one inflation step")]
    [SerializeField] private float breathThresholdKPa = 1.0f;

    private bool wasBreathStrong = false;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (controlMode == InflateControlMode.Keyboard)
        {
            inflatingButton.Enable();
            inflatingButton.performed += OnInflatePressed;
        }
    }

    private void OnDisable()
    {
        if (controlMode == InflateControlMode.Keyboard)
        {
            inflatingButton.performed -= OnInflatePressed;
            inflatingButton.Disable();
        }

        wasBreathStrong = false;
    }

    // Called by InputModeManager to switch between Keyboard/Breath
    public void SetControlMode(bool useBreath)
    {
        InflateControlMode newMode = useBreath ? InflateControlMode.Breath : InflateControlMode.Keyboard;

        if (newMode == controlMode)
            return;

        // Clean up old mode
        if (controlMode == InflateControlMode.Keyboard)
        {
            inflatingButton.performed -= OnInflatePressed;
            inflatingButton.Disable();
        }

        controlMode = newMode;

        // Init new mode
        if (isActiveAndEnabled && controlMode == InflateControlMode.Keyboard)
        {
            inflatingButton.Enable();
            inflatingButton.performed += OnInflatePressed;
        }

        wasBreathStrong = false;

        Debug.Log("InflatingBalloon: Control mode set to " + controlMode);
    }

    private void Update()
    {
        if (controlMode == InflateControlMode.Breath)
        {
            UpdateBreathControl();
        }
    }

    private void OnInflatePressed(InputAction.CallbackContext ctx)
    {
        if (controlMode != InflateControlMode.Keyboard)
            return;

        InflateOnce();
    }

    // Handle breath-based inflation (rising edge)
    private void UpdateBreathControl()
    {
        if (pressureSource == null)
            return;

        float pressure = pressureSource.lastPressureKPa;
        bool breathStrong = pressure >= breathThresholdKPa;

        if (breathStrong && !wasBreathStrong)
        {
            InflateOnce();
        }

        wasBreathStrong = breathStrong;
    }

    // One inflation step + sound
    private void InflateOnce()
    {
        // Calculate new size
        Vector3 s = transform.localScale;
        s.x += scaleStep;
        s.y += scaleStep;

        // Limit size
        if (maxScale > 0f)
        {
            s.x = Mathf.Min(s.x, maxScale);
            s.y = Mathf.Min(s.y, maxScale);
        }

        // Apply new scale
        transform.localScale = s;

        //Play Inflate Sound
        if (audioSource != null && inflateSound != null)
        {
            // Randomize pitch slightly for realism
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(inflateSound);
        }
    }
}
