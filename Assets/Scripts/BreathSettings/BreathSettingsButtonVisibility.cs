using UnityEngine;

/*
 * Shows this button only in Breath mode.
 * Uses CanvasGroup so the button can hide without disabling this script.
 */
[RequireComponent(typeof(CanvasGroup))]
public class BreathSettingsButtonVisibility : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanelToClose;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (GlobalInputModeManager.Instance != null)
            GlobalInputModeManager.Instance.OnModeChanged += HandleModeChanged;

        RefreshVisibility();
    }

    private void OnDisable()
    {
        if (GlobalInputModeManager.Instance != null)
            GlobalInputModeManager.Instance.OnModeChanged -= HandleModeChanged;
    }

    private void Start()
    {
        RefreshVisibility();
    }

    private void HandleModeChanged(bool useBreath)
    {
        ApplyVisibility(useBreath);
    }

    public void RefreshVisibility()
    {
        bool show = GlobalInputModeManager.Instance != null &&
                    GlobalInputModeManager.Instance.UseBreath;

        ApplyVisibility(show);
    }

    private void ApplyVisibility(bool show)
    {
        if (canvasGroup == null)
            return;

        // Hide visually and disable clicking in Keyboard mode.
        canvasGroup.alpha = show ? 1f : 0f;
        canvasGroup.interactable = show;
        canvasGroup.blocksRaycasts = show;

        // Close the settings panel if Breath mode is turned off.
        if (!show && settingsPanelToClose != null)
            settingsPanelToClose.SetActive(false);
    }
}
