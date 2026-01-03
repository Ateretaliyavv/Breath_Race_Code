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

    [HideInInspector]
    [Header("Control Mode")]
    [SerializeField] private InflateControlMode controlMode = InflateControlMode.Keyboard;

    [Header("Input")]
    [SerializeField] private InputAction inflatingButton;

    [Header("Inflation Settings")]
    [Tooltip("How much to add to X & Y scale per second while inflating")]
    [SerializeField] private float inflateRatePerSecond = 0.5f;
    [Tooltip("Maximum scale limit (optional)")]
    [SerializeField] private float maxScale = 3.0f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip inflateSound; // Sound for "Puff"

    [Header("Breath Control")]
    [Tooltip("Source of breath pressure values (kPa)")]
    [SerializeField] private PressureWebSocketReceiver pressureSource;
    [Tooltip("Breath threshold in kPa to start continuous inflation")]
    [SerializeField] private float breathThresholdKPa = 1.0f;

    private bool isInflatingHeld = false;

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
            inflatingButton.canceled += OnInflateReleased;
        }
    }

    private void OnDisable()
    {
        if (controlMode == InflateControlMode.Keyboard)
        {
            inflatingButton.performed -= OnInflatePressed;
            inflatingButton.canceled -= OnInflateReleased;
            inflatingButton.Disable();
        }

        isInflatingHeld = false;
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
            inflatingButton.canceled -= OnInflateReleased;
            inflatingButton.Disable();
        }

        controlMode = newMode;

        // Init new mode
        if (isActiveAndEnabled && controlMode == InflateControlMode.Keyboard)
        {
            inflatingButton.Enable();
            inflatingButton.performed += OnInflatePressed;
            inflatingButton.canceled += OnInflateReleased;
        }

        isInflatingHeld = false;

        Debug.Log("InflatingBalloon: Control mode set to " + controlMode);
    }

    private void Update()
    {
        if (controlMode == InflateControlMode.Keyboard)
        {
            UpdateKeyboardHoldInflation();
        }
        else
        {
            UpdateBreathControl();
        }
    }

    private void OnInflatePressed(InputAction.CallbackContext ctx)
    {
        if (controlMode != InflateControlMode.Keyboard)
            return;

        // Start continuous inflation while the button is held
        isInflatingHeld = true;
    }

    private void OnInflateReleased(InputAction.CallbackContext ctx)
    {
        if (controlMode != InflateControlMode.Keyboard)
            return;

        // Stop continuous inflation when the button is released
        isInflatingHeld = false;
    }

    // Continuous inflation while key is held
    private void UpdateKeyboardHoldInflation()
    {
        if (!isInflatingHeld)
            return;

        InflateContinuous(Time.deltaTime);
    }

    // Handle breath-based inflation (continuous above threshold)
    private void UpdateBreathControl()
    {
        if (pressureSource == null)
            return;

        float pressure = pressureSource.lastPressureKPa;

        // Inflate only while breath is strong enough (above threshold)
        if (pressure >= breathThresholdKPa)
        {
            InflateContinuous(Time.deltaTime);
        }
    }

    // Continuous inflation step + sound
    private void InflateContinuous(float dt)
    {
        if (dt <= 0f || inflateRatePerSecond <= 0f)
            return;

        float delta = inflateRatePerSecond * dt;

        // Calculate new size
        Vector3 s = transform.localScale;
        s.x += delta;
        s.y += delta;

        // Limit size
        if (maxScale > 0f)
        {
            s.x = Mathf.Min(s.x, maxScale);
            s.y = Mathf.Min(s.y, maxScale);
        }

        // Apply new scale
        transform.localScale = s;

        //Play Inflate Sound
        if (audioSource != null && inflateSound != null && !audioSource.isPlaying)
        {
            // Randomize pitch slightly for realism
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(inflateSound);
        }
    }
}
