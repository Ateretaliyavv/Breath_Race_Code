using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Controls one breath setting row:
 * slider value, numeric text, save/load, and live apply.
 */
public class BreathSettingSliderRow : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private BreathActionKey actionKey;

    [Header("UI")]
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text valueText;

    [Header("Value Settings")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 7f;
    [SerializeField] private float step = 1f;
    [SerializeField] private bool wholeNumbers = true;

    [Header("Display")]
    [SerializeField] private string prefix = "";
    [SerializeField] private string suffix = "";

    private bool isRefreshing;

    private void Start()
    {
        // Initialize the slider range and listen for user changes.
        if (slider != null)
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.wholeNumbers = wholeNumbers;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        RefreshFromSavedValue();
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    public void RefreshFromSavedValue()
    {
        if (BreathSettingsManager.Instance == null || slider == null)
            return;

        float value = BreathSettingsManager.Instance.GetValue(actionKey);
        value = NormalizeValue(value);

        // Update UI without triggering save/apply again.
        isRefreshing = true;
        slider.value = value;
        UpdateValueText(value);
        isRefreshing = false;
    }

    private void OnSliderValueChanged(float rawValue)
    {
        if (isRefreshing)
            return;

        float finalValue = NormalizeValue(rawValue);

        // Snap the slider to the final valid value.
        isRefreshing = true;
        slider.value = finalValue;
        isRefreshing = false;

        if (BreathSettingsManager.Instance != null)
            BreathSettingsManager.Instance.SetValue(actionKey, finalValue);

        UpdateValueText(finalValue);

        // Apply the updated threshold immediately in the current scene.
        SceneBreathSettingsApplier applier = FindFirstObjectByType<SceneBreathSettingsApplier>();
        if (applier != null)
            applier.ApplySettingsInCurrentScene();
    }

    private float NormalizeValue(float value)
    {
        // Keep the value inside the allowed range.
        value = Mathf.Clamp(value, minValue, maxValue);

        if (wholeNumbers)
            return Mathf.Round(value);

        if (step > 0f)
            value = Mathf.Round(value / step) * step;

        return value;
    }

    private void UpdateValueText(float value)
    {
        if (valueText == null)
            return;

        // Show either integer or decimal value according to the current mode.
        if (wholeNumbers)
            valueText.text = prefix + Mathf.RoundToInt(value).ToString() + suffix;
        else
            valueText.text = prefix + value.ToString("0.0") + suffix;
    }
}
