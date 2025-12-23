using TMPro;
using UnityEngine;
using UnityEngine.UI;
/*
 * Script that chose random massage from list and show it on panel
 */

public class SurpriseMessageUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button actionButton;

    [Header("Messages")]
    [TextArea]
    [SerializeField] private string[] messages;

    private void Start()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);

        ShowRandomMessage();
    }

    // Chose random massage and show it on the panel
    private void ShowRandomMessage()
    {
        if (messages == null || messages.Length == 0)
        {
            Debug.LogWarning("SurpriseMessageUI: No messages defined.");
            return;
        }

        int index = Random.Range(0, messages.Length);
        string chosenMessage = messages[index];

        if (messageText != null)
            messageText.text = chosenMessage;

        if (messagePanel != null)
            messagePanel.SetActive(true);
    }
}
