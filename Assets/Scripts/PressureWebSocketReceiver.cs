using System;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PressureWebSocketReceiver : MonoBehaviour
{
    [Header("UI (optional)")]
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Latest Pressure (kPa)")]
    public float lastPressureKPa = 0f;

    [Header("Editor/Standalone (WiFi WebSocket)")]
    [SerializeField] private string wsUrl = "ws://192.168.43.3:5005";

    [Header("WebGL (USB WebSerial Receiver)")]
    [Tooltip("Assign the WebSerialPressureReceiver in the scene.")]
    [SerializeField] private WebSerialPressureReceiver webSerialReceiver;

#if !UNITY_WEBGL || UNITY_EDITOR
    private ClientWebSocket client;
    private CancellationTokenSource cts;
#endif

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: value comes from WebSerialPressureReceiver (USB).
        if (webSerialReceiver == null)
            Debug.LogWarning("PressureReceiverUnified: webSerialReceiver is not assigned.");
#else
        // Standalone/Editor: connect over ws:// to ESP32.
        StartDotNetWebSocket();
#endif
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
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
            Debug.Log("PressureReceiverUnified: Connected via .NET WebSocket");
        }
        catch (Exception e)
        {
            Debug.LogError("PressureReceiverUnified: Connect error: " + e.Message);
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
            Debug.LogError("PressureReceiverUnified: Receive error: " + e.Message);
        }
    }

    private void ParsePressure(string msg)
    {
        if (float.TryParse(msg, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            lastPressureKPa = v;
    }

    private void OnDestroy()
    {
        try
        {
            if (cts != null) { cts.Cancel(); cts.Dispose(); cts = null; }
            if (client != null) { client.Dispose(); client = null; }
        }
        catch { }
    }
#endif
}
