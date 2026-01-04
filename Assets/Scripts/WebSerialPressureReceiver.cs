using System;
using System.Globalization;
using System.Text.RegularExpressions;
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
        // Ensure exactly one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.name = "BreathUSB"; // Fixed name for JS SendMessage
        DontDestroyOnLoad(gameObject);

        // Hide debug text until a real message is received
        if (debugText != null)
            debugText.gameObject.SetActive(false);
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
            WebSerial_Connect(gameObject.name, nameof(OnSerialData), nameof(OnSerialStatus));
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

    // Called from JS: receives raw serial chunks (may contain multiple lines)
    public void OnSerialData(string chunk)
    {
        if (string.IsNullOrWhiteSpace(chunk))
            return;

        Debug.Log("SERIAL CHUNK: [" + chunk + "]");

        // Normalize line endings and split into lines
        string[] lines = chunk.Replace("\r", "\n").Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            // Ignore comment/debug lines from device
            if (line.StartsWith("#"))
                continue;

            // Extract first floating-point number from the line
            Match m = Regex.Match(line, @"-?\d+(\.\d+)?");
            if (!m.Success)
            {
                ShowDebug("No number: " + line);
                continue;
            }

            if (float.TryParse(m.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            {
                lastPressureKPa = v;

                Debug.Log("SERIAL PARSED: " +
                          lastPressureKPa.ToString("0.000", CultureInfo.InvariantCulture));

                ShowDebug(
                    "Pressure: " +
                    lastPressureKPa.ToString("0.000", CultureInfo.InvariantCulture) +
                    " kPa"
                );
            }
            else
            {
                ShowDebug("Parse failed: " + line);
            }
        }
    }

    // Called from JS: status messages
    public void OnSerialStatus(string msg)
    {
        SetStatus(msg);
    }

    // Enables and updates the debug text only when needed
    private void ShowDebug(string msg)
    {
        if (debugText == null)
            return;

        if (!debugText.gameObject.activeSelf)
            debugText.gameObject.SetActive(true);

        debugText.text = msg;
    }

    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;

        Debug.Log("WebSerial: " + msg);
    }
}
