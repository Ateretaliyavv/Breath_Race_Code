using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Script that change balloon scales by exhalation
 */
public class InflatingBalloon : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction inflatingButton;

    [Header("Inflation")]
    [Tooltip("How much to add to X & Y scale on each press (e.g. 0.1 = +10%)")]
    [SerializeField] private float scaleStep = 0.1f;

    [Tooltip("Optional: limit the maximum scale (0 = no limit)")]
    [SerializeField] private float maxScale = 0f;

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
        // "performed" for a Button action triggers once per press (good for single click)
        Vector3 s = transform.localScale;

        s.x += scaleStep;
        s.y += scaleStep;

        if (maxScale > 0f)
        {
            s.x = Mathf.Min(s.x, maxScale);
            s.y = Mathf.Min(s.y, maxScale);
        }

        transform.localScale = s;
        // Collider will scale automatically with the transform.
    }
}
