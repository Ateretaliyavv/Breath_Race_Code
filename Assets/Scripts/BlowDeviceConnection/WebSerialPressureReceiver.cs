using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

/*
 * Receives breath pressure data over USB using WebSerial in WebGL builds.
 * Manages connection status messages and exposes the latest pressure value to the game.
 */
public class WebSerialPressureReceiver : MonoBehaviour
{
    // Single instance shared across scenes
    public static WebSerialPressureReceiver Instance { get; private set; }

    [Header("Optional UI")]
    [SerializeField] private TextMeshProUGUI statusText; // Can be disabled in Inspector; auto-shows on new status

    [Header("Latest Pressure (kPa)")]
    public float lastPressureKPa = 0f;

    // True when device is considered connected (status says connected OR first pressure arrives)
    public bool IsConnected { get; private set; } = false;

    // UI panels can listen and refresh immediately
    public event Action<string> StatusChanged;
    public event Action<bool> ConnectionChanged; // Fires only when IsConnected changes

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

        // Must be a root GameObject for DontDestroyOnLoad to work
        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    // Must be called by a user gesture (button click) in WebGL
    public void ConnectUSB()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // Reset connection state for a new attempt
            SetConnected(false);

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
        SetConnected(false);
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
                Debug.LogWarning("SERIAL: No number in line: " + line);
                continue;
            }

            if (float.TryParse(m.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            {
                lastPressureKPa = v;

                // First valid pressure implies the device is connected/streaming
                if (!IsConnected)
                {
                    SetConnected(true);
                    SetStatus("Device connected. Receiving pressure...");
                }

                Debug.Log(
                    "SERIAL PARSED: " +
                    lastPressureKPa.ToString("0.000", CultureInfo.InvariantCulture) +
                    " kPa"
                );
            }
            else
            {
                Debug.LogWarning("SERIAL: Parse failed for line: " + line);
            }
        }
    }

    // Called from JS: status messages
    public void OnSerialStatus(string msg)
    {
        // Update connection state BEFORE notifying listeners
        string m = (msg ?? "").ToLowerInvariant();

        // Your actual success message: "Serial connected (115200)."
        if (m.Contains("serial connected") || m.Contains("connected"))
            SetConnected(true);

        if (m.Contains("disconnected") || m.Contains("disconnect") || m.Contains("closed") || m.Contains("failed") || m.Contains("error"))
            SetConnected(false);

        SetStatus(msg);
    }

    private void SetConnected(bool connected)
    {
        if (IsConnected == connected)
            return;

        IsConnected = connected;
        ConnectionChanged?.Invoke(IsConnected);
    }

    // Updates status text; if the status UI object is disabled in Inspector, it will be enabled on demand
    private void SetStatus(string msg)
    {
        if (statusText != null)
        {
            if (!statusText.gameObject.activeSelf)
                statusText.gameObject.SetActive(true);

            statusText.text = msg;
        }

        Debug.Log("WebSerial: " + msg);

        // Notify listeners (e.g., your connect popup) about status changes
        StatusChanged?.Invoke(msg);
    }
}
