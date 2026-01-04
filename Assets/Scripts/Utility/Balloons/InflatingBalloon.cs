using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Inflates a balloon continuously while the input is held (Keyboard)
 * or while breath pressure is above a threshold (Breath).
 *
 * IMPORTANT:
 * - Breath input is read from PressureWebSocketReceiver.Instance (singleton).
 * - Do not assign any pressure source in the Inspector.
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
    [SerializeField] private AudioClip inflateSound; // Sound for "Hiss/Air"

    [Header("Breath Control")]
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
        StopInflationSound();
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
        StopInflationSound();

        Debug.Log("InflatingBalloon: Control mode set to " + controlMode);
    }

    private float GetPressureKPa()
    {
        if (PressureWebSocketReceiver.Instance == null)
            return 0f;

        return PressureWebSocketReceiver.Instance.lastPressureKPa;
    }

    private void Update()
    {
        bool isInflatingNow = false;

        // Decide if we should inflate this frame
        if (controlMode == InflateControlMode.Keyboard)
        {
            if (isInflatingHeld)
            {
                InflateContinuous(Time.deltaTime);
                isInflatingNow = true;
            }
        }
        else
        {
            float pressure = GetPressureKPa();
            if (pressure >= breathThresholdKPa)
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
            if (!audioSource.isPlaying)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
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

    private void StopInflationSound()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
