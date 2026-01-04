using System;
using System.Globalization;
using TMPro;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class WebSerialPressureReceiver : MonoBehaviour
{
    // Single instance shared across scenes
    public static WebSerialPressureReceiver Instance { get; private set; }

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

    private void Awake()
    {
        // Keep exactly one receiver across all scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.name = "BreathUSB"; // Fixed name for JS SendMessage
        DontDestroyOnLoad(gameObject);
    }

    // Must be called by a user gesture (button click) in WebGL
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

    // Called from JS: receives a single line from Serial
    public void OnSerialLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        // Ignore debug lines that start with '#'
        if (line.StartsWith("#"))
            return;

        // Expecting numeric-only lines like: "3.452"
        if (float.TryParse(line.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
        {
            lastPressureKPa = v;
            if (debugText != null)
                debugText.text = "Pressure: " + lastPressureKPa.ToString("0.000") + " kPa";
        }
        else
        {
            // If you ever see this, Arduino is still printing text like "Pressure: ..."
            if (debugText != null)
                debugText.text = "Parse failed: " + line;
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
