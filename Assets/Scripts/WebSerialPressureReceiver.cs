using System.Globalization;
using TMPro;
using UnityEngine;
using System;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class WebSerialPressureReceiver : MonoBehaviour
{
    [Header("Optional UI")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Latest Pressure (kPa)")]
    public float lastPressureKPa = 0f;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern int WebSerial_IsSupported();
    [DllImport("__Internal")] private static extern void WebSerial_Connect(string goName, string onDataMethod, string onStatusMethod);
    [DllImport("__Internal")] private static extern void WebSerial_Disconnect();
#endif

    // Call this from a UI Button (must be user gesture)
    public void ConnectUSB()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    try
    {
        int supported = WebSerial_IsSupported();
        SetStatus("WebSerial supported: " + supported);

        if (supported == 0)
        {
            SetStatus("WebSerial not supported. Use Chrome/Edge on desktop.");
            return;
        }

        SetStatus("Requesting USB device...");
        WebSerial_Connect(gameObject.name, nameof(OnSerialLine), nameof(OnSerialStatus));
    }
    catch (Exception e)
    {
        SetStatus("WebSerial exception: " + e.Message);
    }
#else
        SetStatus("USB WebSerial works only in WebGL build (not in Unity Editor).");
#endif
    }


    public void DisconnectUSB()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebSerial_Disconnect();
        SetStatus("Disconnected.");
#else
        SetStatus("Disconnect is WebGL-only.");
#endif
    }

    // Called from JS: receives a single line (should be a number)
    public void OnSerialLine(string line)
    {
        // Ignore debug lines if you ever send them with '#'
        if (!string.IsNullOrEmpty(line) && line.StartsWith("#"))
            return;

        if (float.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
        {
            lastPressureKPa = v;
            if (debugText != null)
                debugText.text = "Pressure: " + lastPressureKPa.ToString("0.000") + " kPa";
        }
    }

    // Called from JS: status messages
    public void OnSerialStatus(string msg)
    {
        SetStatus(msg);
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log("WebSerial: " + msg);
    }
}
