using UnityEngine;

/*
 * Resets all breath settings to defaults,
 * refreshes the sliders, and reapplies them in the scene.
 */
public class BreathSettingsResetButton : MonoBehaviour
{
    public void ResetAllSettings()
    {
        if (BreathSettingsManager.Instance == null)
            return;

        BreathSettingsManager.Instance.ResetToDefaults();

        // Refresh all visible slider rows in the settings panel.
        BreathSettingSliderRow[] rows = FindObjectsByType<BreathSettingSliderRow>(FindObjectsSortMode.None);
        foreach (BreathSettingSliderRow row in rows)
            row.RefreshFromSavedValue();

        // Apply the default thresholds immediately in the current scene.
        SceneBreathSettingsApplier applier = FindFirstObjectByType<SceneBreathSettingsApplier>();
        if (applier != null)
            applier.ApplySettingsInCurrentScene();
    }
}
