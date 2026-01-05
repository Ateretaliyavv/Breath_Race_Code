using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Menu breath-device connect panel:
 * - Connects to WebSerial USB device (WebGL)
 * - Shows Start button only after connection
 * - Starts the game in Breath mode
 */
public class BreathConnectPanel : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Buttons")]
    [SerializeField] private Button connectUsbButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button closeButton;

    [Header("Optional UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Scene To Load")]
    [SerializeField] private string SceneNameToLoad = "OpenScene";

    private WebSerialPressureReceiver usb;

    private void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;

        // Start hidden until connected
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(false);

        if (connectUsbButton != null)
            connectUsbButton.onClick.AddListener(OnConnectClicked);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        usb = WebSerialPressureReceiver.Instance;

        if (usb != null)
            usb.StatusChanged += OnUsbStatus;

        RefreshStartButton();
    }

    private void OnDisable()
    {
        if (usb != null)
            usb.StatusChanged -= OnUsbStatus;
    }

    public void Open()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        SetStatus("Press Connect to pair the USB device.");
        RefreshStartButton();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnConnectClicked()
    {
        usb = WebSerialPressureReceiver.Instance;

        if (usb == null)
        {
            SetStatus("WebSerialPressureReceiver is missing.");
            return;
        }

        SetStatus("Requesting USB device...");
        usb.ConnectUSB();
    }

    private void OnStartClicked()
    {
        // Force global Breath mode before entering game
        if (GlobalInputModeManager.Instance != null)
            GlobalInputModeManager.Instance.SetBreath();

        SceneNavigator.LoadScene(SceneNameToLoad, true);
    }

    private void OnUsbStatus(string msg)
    {
        SetStatus(msg);
        RefreshStartButton();
    }

    private void RefreshStartButton()
    {
        usb = WebSerialPressureReceiver.Instance;

        bool ready = (usb != null && usb.IsConnected);

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(ready);
    }

    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;

        Debug.Log("BreathConnectPanel: " + msg);
    }
}
