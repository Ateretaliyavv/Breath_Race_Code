using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * Shows a global popup when the USB breath device disconnects DURING gameplay,
 * but only if the current input mode is Breath.
 * Popup is suppressed in excluded scenes (e.g., intro/connect scenes).
 *
 * NOTE:
 * - Assign the popup prefab in the Inspector (no Resources folder needed).
 * - This manager must exist once (e.g., in your Boot/Entry scene) and uses DontDestroyOnLoad.
 */
public class BreathDisconnectPopupManager : MonoBehaviour
{
    public static BreathDisconnectPopupManager Instance { get; private set; }

    [Header("Popup Prefab (assign in Inspector)")]
    [SerializeField] private GameObject popupPrefab;

    [Header("Excluded Scenes (no popup here)")]
    [SerializeField] private string[] excludedSceneNames = { "Entry", "Connect", "BreathConnect", "OpenScene" };

    [Header("Popup Text")]
    [TextArea]
    [SerializeField]
    private string popupMessage =
        "The breath device was disconnected.\nPlease reconnect the USB device to continue in Breath mode.";

    private GameObject popupInstance;
    private TextMeshProUGUI popupText;
    private Button closeButton;

    private bool lastConnected = true;
    private bool subscribed = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;

        UnsubscribeUsb();
    }

    private void Start()
    {
        EnsurePopupLoaded();
        HidePopup();

        SubscribeUsbIfPossible();

        // Baseline if receiver already exists
        if (WebSerialPressureReceiver.Instance != null)
            lastConnected = WebSerialPressureReceiver.Instance.IsConnected;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsurePopupLoaded();

        // Re-try subscribing after scene switches (receiver may appear later)
        SubscribeUsbIfPossible();

        // Never show popup in excluded scenes
        if (IsExcludedScene(scene.name))
            HidePopup();
    }

    private void SubscribeUsbIfPossible()
    {
        var receiver = WebSerialPressureReceiver.Instance;
        if (receiver == null)
            return;

        if (!subscribed)
        {
            receiver.ConnectionChanged += OnUsbConnectionChanged;
            subscribed = true;
        }

        // Keep baseline updated
        lastConnected = receiver.IsConnected;
    }

    private void UnsubscribeUsb()
    {
        var receiver = WebSerialPressureReceiver.Instance;
        if (receiver == null)
            return;

        if (subscribed)
        {
            receiver.ConnectionChanged -= OnUsbConnectionChanged;
            subscribed = false;
        }
    }

    private void OnUsbConnectionChanged(bool connected)
    {
        bool wasConnected = lastConnected;
        lastConnected = connected;

        // Only react on Connected -> Disconnected transition
        if (wasConnected && !connected)
            TryShowDisconnectPopup();
    }

    private void TryShowDisconnectPopup()
    {
        // 1) Only if we're in Breath mode
        if (GlobalInputModeManager.Instance == null || !GlobalInputModeManager.Instance.UseBreath)
            return;

        // 2) Suppress in excluded scenes (intro/connect scenes)
        string sceneName = SceneManager.GetActiveScene().name;
        if (IsExcludedScene(sceneName))
            return;

        EnsurePopupLoaded();
        ShowPopup(popupMessage);
    }

    private bool IsExcludedScene(string sceneName)
    {
        if (excludedSceneNames == null) return false;

        for (int i = 0; i < excludedSceneNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(excludedSceneNames[i]) && excludedSceneNames[i] == sceneName)
                return true;
        }

        return false;
    }

    private void EnsurePopupLoaded()
    {
        if (popupInstance != null)
            return;

        if (popupPrefab == null)
        {
            Debug.LogError("BreathDisconnectPopupManager: popupPrefab is not assigned in Inspector.");
            return;
        }

        popupInstance = Instantiate(popupPrefab);
        DontDestroyOnLoad(popupInstance);

        // Auto-find components
        popupText = popupInstance.GetComponentInChildren<TextMeshProUGUI>(true);
        closeButton = popupInstance.GetComponentInChildren<Button>(true);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HidePopup);
            closeButton.onClick.AddListener(HidePopup);
        }
        else
        {
            Debug.LogWarning("BreathDisconnectPopupManager: Close Button not found in popup prefab.");
        }
    }

    private void ShowPopup(string msg)
    {
        if (popupInstance == null)
            return;

        popupInstance.SetActive(true);

        if (popupText != null)
            popupText.text = msg;
    }

    private void HidePopup()
    {
        if (popupInstance != null)
            popupInstance.SetActive(false);
    }
}
