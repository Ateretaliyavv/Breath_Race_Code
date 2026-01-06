using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * BlowProgressBar
 * - Reads pressure from PressureWebSocketReceiver singleton (persists across scenes).
 * - Hides itself when the global input mode is Keyboard.
 */
public class BlowProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;               // Image (Type = Filled)
    [SerializeField] private TextMeshProUGUI percentText;   // optional

    [Header("Pressure Range (kPa)")]
    [SerializeField] private float minKPa = 0.3f;
    [SerializeField] private float maxKPa = 8f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 12f;

    [Header("Happy Colors (optional)")]
    [SerializeField] private Gradient happyGradient;

    [Header("Visibility")]
    [SerializeField] private GameObject uiRoot; // If null uses this GameObject

    private float smooth01;
    private bool lastVisibleState = true;

    private void Awake()
    {
        if (uiRoot == null)
            uiRoot = gameObject;

        if (fillImage == null)
            Debug.LogWarning("BlowProgressBar: fillImage is not assigned.");
    }

    private void Update()
    {
        // Show only in Breath mode (if manager missing default to show)
        bool shouldShow = true;

        if (GlobalInputModeManager.Instance != null)
            shouldShow = GlobalInputModeManager.Instance.UseBreath;

        if (shouldShow != lastVisibleState)
        {
            uiRoot.SetActive(shouldShow);
            lastVisibleState = shouldShow;

            // Reset visuals when hiding to avoid showing stale state next time
            if (!shouldShow)
            {
                smooth01 = 0f;
                if (fillImage != null) fillImage.fillAmount = 0f;
                if (percentText != null) percentText.text = "";
                return;
            }
        }

        if (!shouldShow)
            return;

        if (fillImage == null)
            return;

        // Read from singleton (created once in entry scene / auto-created by receiver).
        float kpa = 0f;
        if (PressureWebSocketReceiver.Instance != null)
            kpa = PressureWebSocketReceiver.Instance.lastPressureKPa;

        // Map kPa 0..1
        float target01 = Mathf.Clamp01(Mathf.InverseLerp(minKPa, maxKPa, kpa));

        // Smooth movement
        smooth01 = Mathf.Lerp(smooth01, target01, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        // Apply to UI
        fillImage.fillAmount = smooth01;

        if (percentText != null)
        {
            int kpaInt = Mathf.FloorToInt(kpa);
            percentText.text = kpaInt.ToString();
        }

        if (happyGradient != null)
            fillImage.color = happyGradient.Evaluate(smooth01);
    }
}
