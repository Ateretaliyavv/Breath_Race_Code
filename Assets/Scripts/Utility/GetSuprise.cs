using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetSuprise : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private GameObject targetObject;   // The other object to detect

    [Header("UI References")]
    [SerializeField] private GameObject messagePanel;   // Panel that holds text + button
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button actionButton;

    [Header("Messages")]
    [TextArea]
    [SerializeField] private string[] messages;         // List of possible messages

    private bool hasTriggered = false;

    private void Start()
    {
        // Hide the panel at the beginning
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check that the trigger is with the specific target object
        if (other.gameObject != targetObject)
            return;

        if (hasTriggered)
            return;

        hasTriggered = true;
        ShowRandomMessage();
    }

    private void ShowRandomMessage()
    {
        if (messages == null || messages.Length == 0)
        {
            Debug.LogWarning("CollisionMessageUI: No messages defined.");
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
