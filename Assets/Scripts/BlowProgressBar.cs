using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlowProgressBar : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private PressureWebSocketReceiver receiver;

    [Header("UI References")]
    [SerializeField] private Image fillImage;               // the pink Image (Type=Filled)
    [SerializeField] private TextMeshProUGUI percentText;   // the % text (optional)

    [Header("Pressure Range (kPa)")]
    [SerializeField] private float minKPa = 0.3f;
    [SerializeField] private float maxKPa = 8f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 12f;

    [Header("Happy Colors (optional)")]
    [SerializeField] private Gradient happyGradient;

    private float smooth01;

    private void Update()
    {
        if (receiver == null || fillImage == null) return;

        float kpa = receiver.lastPressureKPa;

        // map kPa -> 0..1
        float target01 = Mathf.Clamp01(Mathf.InverseLerp(minKPa, maxKPa, kpa));

        // smooth movement
        smooth01 = Mathf.Lerp(smooth01, target01, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        // THIS is the correct way for a Filled Image
        fillImage.fillAmount = smooth01;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(smooth01 * 100f) + "%";

        if (happyGradient != null)
            fillImage.color = happyGradient.Evaluate(smooth01);
    }
}
