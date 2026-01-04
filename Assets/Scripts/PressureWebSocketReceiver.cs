using System;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/*
 * PressureWebSocketReceiver
 * - Singleton that persists across scenes (DontDestroyOnLoad)
 * - Auto-creates itself if missing (so it exists even when starting from any scene)
 * - Standalone/Editor: reads pressure over WiFi WebSocket (ws://)
 * - WebGL: reads pressure from WebSerialPressureReceiver.Instance (USB WebSerial)
 *
 * IMPORTANT:
 * - You can place it in the Entry scene, but you don't have to (it will auto-create).
 * - Do NOT add multiple copies in scenes; duplicates will self-destroy.
 */
public class PressureWebSocketReceiver : MonoBehaviour
{
    public static PressureWebSocketReceiver Instance { get; private set; }

    [Header("UI (optional)")]
    [SerializeField] private TextMeshProUGUI debugText;

    [Header("Latest Pressure (kPa)")]
    public float lastPressureKPa = 0f;

    [Header("Editor/Standalone (WiFi WebSocket)")]
    [SerializeField] private string wsUrl = "ws://192.168.43.3:5005";

#if !UNITY_WEBGL || UNITY_EDITOR
    private ClientWebSocket client;
    private CancellationTokenSource cts;
#endif

    // Ensures this singleton exists even if you start Play from a non-entry scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExists()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("PressureWebSocketReceiver");
        go.AddComponent<PressureWebSocketReceiver>();
    }

    private void Awake()
    {
        // Singleton + persist across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: connection is handled by WebSerialPressureReceiver (user gesture required).
#else
        // Editor/Standalone: connect over WiFi WebSocket to ESP32.
        StartDotNetWebSocket();
#endif
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: only update when USB receiver exists; otherwise keep last value (don't reset to 0).
        if (WebSerialPressureReceiver.Instance != null)
            lastPressureKPa = WebSerialPressureReceiver.Instance.lastPressureKPa;
#endif

        if (debugText != null)
            debugText.text = "Pressure: " + lastPressureKPa.ToString("0.000", CultureInfo.InvariantCulture) + " kPa";
    }

#if !UNITY_WEBGL || UNITY_EDITOR
    private async void StartDotNetWebSocket()
    {
        // Avoid duplicate connections if Start is called again somehow.
        if (client != null && client.State == WebSocketState.Open)
            return;

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
        byte[] buffer = new byte[1024];

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

                    // Parse a single float number sent as text (InvariantCulture).
                    if (float.TryParse(msg, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                        lastPressureKPa = v;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal on shutdown
        }
        catch (Exception e)
        {
            Debug.LogError("PressureWebSocketReceiver: Receive error: " + e.Message);
        }
    }

    private void OnDestroy()
    {
        // Only the active singleton should clean up resources.
        if (Instance != this)
            return;

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
                client.Dispose();
                client = null;
            }
        }
        catch
        {
            // Ignore shutdown errors
        }

        // Clear Instance when destroyed (e.g., when quitting play mode)
        if (Instance == this)
            Instance = null;
    }
#endif
}
