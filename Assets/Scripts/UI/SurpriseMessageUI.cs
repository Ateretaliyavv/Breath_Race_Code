using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Chooses a random localization key and shows the translated text via LocalizedTMP.
 */
public class SurpriseMessageUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button actionButton;

    [Header("Message Keys (CSV)")]
    [SerializeField] private string[] messageKeys;

    private LocalizedTMP localized;

    private void Awake()
    {
        if (messageText != null)
            localized = messageText.GetComponent<LocalizedTMP>();
    }

    private void Start()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);

        ShowRandomMessage();
    }

    // Choose a random key and let LocalizedTMP handle translation + RTL.
    private void ShowRandomMessage()
    {
        if (messageKeys == null || messageKeys.Length == 0)
        {
            Debug.LogWarning("SurpriseMessageUI: No message keys defined.");
            return;
        }

        if (localized == null)
        {
            Debug.LogError("SurpriseMessageUI: MessageText is missing LocalizedTMP component.");
            return;
        }

        int index = Random.Range(0, messageKeys.Length);
        string chosenKey = messageKeys[index];

        localized.SetKey(chosenKey);

        if (messagePanel != null)
            messagePanel.SetActive(true);
    }
}
