using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GetSuprise : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private GameObject targetObject;   // Object we monitor moving on X

    [Header("UI References")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button actionButton;

    [Header("Messages")]
    [TextArea]
    [SerializeField] private string[] messages;

    private bool hasTriggered = false;

    private void Start()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }

    private void Update()
    {
        if (hasTriggered)
            return;

        if (targetObject == null)
            return;

        // Check if target passed the X position of this object
        if (targetObject.transform.position.x >= transform.position.x)
        {
            hasTriggered = true;
            ShowRandomMessage();
        }
    }

    private void ShowRandomMessage()
    {
        if (messages == null || messages.Length == 0)
        {
            Debug.LogWarning("GetSuprise: No messages defined.");
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
