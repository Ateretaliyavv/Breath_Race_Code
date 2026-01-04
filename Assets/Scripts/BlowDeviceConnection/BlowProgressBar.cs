using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * BlowProgressBar
 * - Reads pressure from PressureWebSocketReceiver singleton (persists across scenes).
 * - No Inspector reference required; it auto-resolves Instance.
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

    private float smooth01;

    private void Awake()
    {
        if (fillImage == null)
            Debug.LogWarning("BlowProgressBar: fillImage is not assigned.");
    }

    private void Update()
    {
        if (fillImage == null) return;

        // Read from singleton (created once in entry scene / auto-created by receiver).
        float kpa = 0f;
        if (PressureWebSocketReceiver.Instance != null)
            kpa = PressureWebSocketReceiver.Instance.lastPressureKPa;

        // Map kPa -> 0..1
        float target01 = Mathf.Clamp01(Mathf.InverseLerp(minKPa, maxKPa, kpa));

        // Smooth movement
        smooth01 = Mathf.Lerp(smooth01, target01, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        // Apply to UI
        fillImage.fillAmount = smooth01;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(smooth01 * 100f) + "%";

        if (happyGradient != null)
            fillImage.color = happyGradient.Evaluate(smooth01);
    }
}
