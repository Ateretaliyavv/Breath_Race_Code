using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using UnityEngine;
using TMPro;

public class PressureWebSocketReceiver : MonoBehaviour
{
    [Header("WebSocket Settings")]
    [Tooltip("IP of ESP32 WebSocket server, e.g. ws://192.168.43.3:5005")]
    [SerializeField] private string wsUrl = "ws://192.168.43.3:5005";

    [Header("Debug UI (optional)")]
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Latest Pressure (kPa)")]
    public float lastPressureKPa = 0f;

    // ====== WebGL (JavaScript plugin) ======
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WS_Connect(string url, string gameObjectName, string methodName);

    [DllImport("__Internal")]
    private static extern void WS_Close();
#endif

    // ====== Editor / Standalone (.NET WebSocket) ======
#if !UNITY_WEBGL || UNITY_EDITOR
    private ClientWebSocket client;
    private CancellationTokenSource cts;
#endif

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: use JS WebSocket
        WS_Connect(wsUrl, gameObject.name, "OnWebSocketMessage");
        Debug.Log("PressureWebSocketReceiver (WebGL): Connecting to " + wsUrl);
#else
        // Editor / PC build: use .NET WebSocket
        Debug.Log("PressureWebSocketReceiver (Editor/Standalone): Connecting to " + wsUrl);
        StartDotNetWebSocket();
#endif
    }

    private void OnDestroy()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WS_Close();
#else
        CloseDotNetWebSocket();
#endif
    }

    // ---------- WEBGL PATH ----------
    // Called from JS when a WebSocket message is received
    public void OnWebSocketMessage(string msg)
    {
        ParsePressure(msg);
    }

    // ---------- EDITOR / PC PATH ----------
#if !UNITY_WEBGL || UNITY_EDITOR
    private async void StartDotNetWebSocket()
    {
        cts = new CancellationTokenSource();
        client = new ClientWebSocket();

        try
        {
            await client.ConnectAsync(new Uri(wsUrl), cts.Token);
            Debug.Log("PressureWebSocketReceiver: Connected via .NET WebSocket");

            _ = ReceiveLoop(cts.Token);
        }
        catch (Exception e)
        {
            Debug.LogError("PressureWebSocketReceiver: ConnectAsync error: " + e.Message);
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
                {
                    Debug.Log("PressureWebSocketReceiver: Server closed connection");
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", token);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ParsePressure(msg);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore – we're shutting down
        }
        catch (Exception e)
        {
            Debug.LogError("PressureWebSocketReceiver: ReceiveLoop error: " + e.Message);
        }
    }

    private void CloseDotNetWebSocket()
    {
        try
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }

            if (client != null)
            {
                if (client.State == WebSocketState.Open)
                    client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).Wait();

                client.Dispose();
                client = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("PressureWebSocketReceiver: CloseDotNetWebSocket error: " + e.Message);
        }
    }
#endif

    // ---------- SHARED PARSING ----------
    private void ParsePressure(string msg)
    {
        if (float.TryParse(msg, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            lastPressureKPa = value;

            if (debugText != null)
                debugText.text = "Pressure: " + lastPressureKPa.ToString("0.000") + " kPa";
        }
        else
        {
            Debug.LogWarning("PressureWebSocketReceiver: Failed to parse message: " + msg);
        }
    }
}
