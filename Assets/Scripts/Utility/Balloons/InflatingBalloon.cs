using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Script that handles balloon inflation via button press or breath.
 * - Plays an "Inflate" sound on every step.
 */
public class InflatingBalloon : MonoBehaviour
{
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

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        inflatingButton.Enable();
        inflatingButton.performed += OnInflatePressed;
    }

    private void OnDisable()
    {
        inflatingButton.performed -= OnInflatePressed;
        inflatingButton.Disable();
    }

    private void OnInflatePressed(InputAction.CallbackContext ctx)
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

        // --- Play Inflate Sound ---
        if (audioSource != null && inflateSound != null)
        {
            // Randomize pitch slightly for realism
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(inflateSound);
        }
    }
}
