using System;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/*
 * Unified pressure receiver:
 * - WebGL: reads pressure from WebSerialPressureReceiver.Instance (USB).
 * - Editor/Standalone: reads pressure from ESP32 via ws:// WebSocket.
 *
 * This script is intended to exist in EVERY scene.
 * It does NOT use DontDestroyOnLoad.
 * It prevents duplicates by enforcing a singleton instance.
 */

public class PressureWebSocketReceiver : MonoBehaviour
{
    [Header("UI (optional)")]
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Latest Pressure (kPa)")]
    public float lastPressureKPa = 0f;

    [Header("Editor/Standalone (WiFi WebSocket)")]
    [SerializeField] private string wsUrl = "ws://192.168.43.3:5005";

    [Header("WebGL (USB WebSerial Receiver)")]
    [Tooltip("Optional. If left empty, the script will auto-use WebSerialPressureReceiver.Instance.")]
    [SerializeField] private WebSerialPressureReceiver webSerialReceiver;

    private static PressureWebSocketReceiver instance;

#if !UNITY_WEBGL || UNITY_EDITOR
    private ClientWebSocket client;
    private CancellationTokenSource cts;
#endif

    private void Awake()
    {
        // Enforce exactly one active instance even if the script exists in every scene.
        // The newest scene instance will be destroyed; the first instance remains active.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: values come from WebSerialPressureReceiver (USB).
        if (webSerialReceiver == null)
            webSerialReceiver = WebSerialPressureReceiver.Instance;

        if (webSerialReceiver == null)
            Debug.LogWarning("PressureWebSocketReceiver (WebGL): WebSerialPressureReceiver.Instance not found. Pressure will stay 0 until USB is connected.");
#else
        // Editor/Standalone: connect over ws:// to ESP32.
        StartDotNetWebSocket();
#endif
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // In case the USB object appears later (after scene load), keep trying to find it.
        if (webSerialReceiver == null)
            webSerialReceiver = WebSerialPressureReceiver.Instance;

        if (webSerialReceiver != null)
            lastPressureKPa = webSerialReceiver.lastPressureKPa;
#endif

        if (debugText != null)
            debugText.text = "Pressure: " + lastPressureKPa.ToString("0.000") + " kPa";
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    private async void StartDotNetWebSocket()
    {
        cts = new CancellationTokenSource();
        client = new ClientWebSocket();

        try
        {
            await client.ConnectAsync(new Uri(wsUrl), cts.Token);
            _ = ReceiveLoop(cts.Token);
            Debug.Log("PressureWebSocketReceiver: Connected via .NET WebSocket");
        }
        catch (Exception e)
        {
            Debug.LogError("PressureWebSocketReceiver: Connect error: " + e.Message);
        }
    }

    private async Task ReceiveLoop(CancellationToken token)
    {
        var buffer = new byte[1024];

        try
        {
            while (!token.IsCancellationRequested &&
                   client != null &&
                   client.State == WebSocketState.Open)
            {
                var segment = new ArraySegment<byte>(buffer);
                WebSocketReceiveResult result = await client.ReceiveAsync(segment, token);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ParsePressure(msg);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            Debug.LogError("PressureWebSocketReceiver: Receive error: " + e.Message);
        }
    }

    private void ParsePressure(string msg)
    {
        if (float.TryParse(msg, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            lastPressureKPa = v;
    }

    private void OnDestroy()
    {
        // Only the active singleton instance should clean up.
        if (instance != this) return;

        try
        {
            if (cts != null) { cts.Cancel(); cts.Dispose(); cts = null; }
            if (client != null) { client.Dispose(); client = null; }
        }
        catch { }
    }
#endif
}
