using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class WebSerialPressureReceiver : MonoBehaviour
{
    [Header("Singleton / Persistence")]
    [Tooltip("Keep this object alive across scene loads (required for WebGL + WebSerial).")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Optional UI")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Latest Pressure (kPa)")]
    public float lastPressureKPa = 0f;

    // Expose a global instance so other scripts can read the latest pressure safely.
    public static WebSerialPressureReceiver Instance { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern int WebSerial_IsSupported();
    [DllImport("__Internal")] private static extern void WebSerial_Connect(string goName, string onDataMethod, string onStatusMethod);
    [DllImport("__Internal")] private static extern void WebSerial_Disconnect();
#endif

    private void Awake()
    {
        // Ensure only one receiver exists. This prevents duplicate connections after returning to menu.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Optional: log scene changes while debugging connection persistence.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // If your UI references exist only in the first scene, they will become null after scene load.
        // The receiver keeps working; only UI updates are skipped when references are missing.
        Debug.Log($"WebSerial: Receiver active in scene '{scene.name}' as '{gameObject.name}'.");
    }

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

            // IMPORTANT: JS will SendMessage to the GameObject name you pass here.
            // This GameObject must continue to exist across scene loads.
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

            // UI references may be null in other scenes (normal if UI exists only in the menu scene).
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
