using UnityEngine;
using UnityEngine.UI;

/*
 * Help / Info panel:
 * - Opens from an "!" button
 * - Shows scrollable content (text + images) via a ScrollRect
 * - Closes with an X button
 */
public class InfoPanel : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Buttons")]
    [Tooltip("Optional: the '!' button that opens this panel.")]
    [SerializeField] private Button openHelpButton;
    [Tooltip("X close button inside the panel.")]
    [SerializeField] private Button closeButton;

    [Header("Scroll View (optional but recommended)")]
    [Tooltip("ScrollRect of the help panel (for reset-to-top on open).")]
    [SerializeField] private ScrollRect scrollRect;

    [Header("Behavior")]
    [Tooltip("If true, resets the scroll to the top every time you open.")]
    [SerializeField] private bool resetScrollOnOpen = true;

    // Initialize references and wire buttons
    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        // Start closed by default
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (openHelpButton != null)
            openHelpButton.onClick.AddListener(Open);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    // Open panel + optionally reset scroll position
    public void Open()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (resetScrollOnOpen)
            ResetScrollToTop();
    }

    // Close panel
    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    // Keep it simple: set scroll to top on next frame (layout safe)
    private void ResetScrollToTop()
    {
        if (scrollRect == null)
            return;

        // Ensure layout had a chance to rebuild before snapping
        Canvas.ForceUpdateCanvases();

        // Vertical scroll: 1 = top, 0 = bottom
        scrollRect.verticalNormalizedPosition = 1f;
        scrollRect.velocity = Vector2.zero;
    }
}
