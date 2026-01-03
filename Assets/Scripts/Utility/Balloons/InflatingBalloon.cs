using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private AudioClip inflateSound; // Sound for "Hiss/Air"

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


        if (audioSource != null)
        {
            audioSource.loop = true; // the sound should loop while inflating
            audioSource.clip = inflateSound;
            audioSource.playOnAwake = false;
        }
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
        StopInflationSound(); // make sure sound stops when disabled
    }

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
        StopInflationSound(); // stop sound on mode change

        Debug.Log("InflatingBalloon: Control mode set to " + controlMode);
    }

    private void Update()
    {
        bool isInflatingNow = false;

        // check if we should inflate this frame
        if (controlMode == InflateControlMode.Keyboard)
        {
            if (isInflatingHeld)
            {
                InflateContinuous(Time.deltaTime);
                isInflatingNow = true;
            }
        }
        else // Breath Mode
        {
            if (pressureSource != null && pressureSource.lastPressureKPa >= breathThresholdKPa)
            {
                InflateContinuous(Time.deltaTime);
                isInflatingNow = true;
            }
        }


        HandleAudio(isInflatingNow);
    }

    private void HandleAudio(bool isInflating)
    {
        if (audioSource == null || inflateSound == null) return;

        if (isInflating)
        {
            // if inflating, ensure the sound is playing
            if (!audioSource.isPlaying)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.Play();
            }
        }
        else
        {
            // if not inflating, stop the sound if it's playing
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void OnInflatePressed(InputAction.CallbackContext ctx)
    {
        if (controlMode != InflateControlMode.Keyboard) return;
        isInflatingHeld = true;
    }

    private void OnInflateReleased(InputAction.CallbackContext ctx)
    {
        if (controlMode != InflateControlMode.Keyboard) return;
        isInflatingHeld = false;
    }

    // Continuous inflation step (ללא סאונד בפנים)
    private void InflateContinuous(float dt)
    {
        if (dt <= 0f || inflateRatePerSecond <= 0f)
            return;

        float delta = inflateRatePerSecond * dt;

        Vector3 s = transform.localScale;
        s.x += delta;
        s.y += delta;

        if (maxScale > 0f)
        {
            s.x = Mathf.Min(s.x, maxScale);
            s.y = Mathf.Min(s.y, maxScale);
        }

        transform.localScale = s;
    }

    // fast stop of inflation sound
    private void StopInflationSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
