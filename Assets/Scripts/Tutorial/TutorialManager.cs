using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // Required for the New Input System
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Player Scripts References")]
    [SerializeField] private Move moveScript;
    [SerializeField] private Jump jumpScript;
    [SerializeField] private BlowUpBalloons blowUpScript;
    [SerializeField] private BridgeBuilder bridgeScript;
    [SerializeField] private PushBox pushBox;

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Input Settings (New System)")]
    // Note: Changed from KeyCode to Key for the New Input System
    [SerializeField] private Key jumpKey = Key.UpArrow;
    [SerializeField] private Key bridgeKey = Key.Space;
    [SerializeField] private Key blowUpKey = Key.Space;
    [SerializeField] private Key pushKey = Key.Space;

    // Internal state variables
    private MonoBehaviour scriptToUnlock;
    private Key keyToWaitFor; // Stores which key we are waiting for
    private bool isTutorialActive = false;

    private void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isTutorialActive)
        {
            // Check if Keyboard is connected to prevent errors
            if (Keyboard.current == null) return;

            // Check if the specific key was pressed in this frame
            if (Keyboard.current[keyToWaitFor].wasPressedThisFrame)
            {
                CloseTutorial();
            }
        }
    }

    public void TriggerTutorial(string message, TutorialType type)
    {
        // 1. Disable player controls
        moveScript.enabled = false;
        jumpScript.enabled = false;
        blowUpScript.enabled = false;
        bridgeScript.enabled = false;
        pushBox.enabled = false;

        // 2. Configure the key to wait for and the script to unlock
        switch (type)
        {
            case TutorialType.Jump:
                scriptToUnlock = jumpScript;
                keyToWaitFor = jumpKey;
                break;
            case TutorialType.Bridge:
                scriptToUnlock = bridgeScript;
                keyToWaitFor = bridgeKey;
                break;
            case TutorialType.BlowUp:
                scriptToUnlock = blowUpScript;
                keyToWaitFor = blowUpKey;
                break;
            case TutorialType.PushBox:
                scriptToUnlock = pushBox;
                keyToWaitFor = pushKey;
                break;
        }

        // 3. Update the UI
        if (tutorialText != null)
            tutorialText.text = message;

        if (promptText != null)
            promptText.text = "Press " + keyToWaitFor.ToString() + " to continue";

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        isTutorialActive = true;
    }

    private void CloseTutorial()
    {
        isTutorialActive = false;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        moveScript.isPressedUI = true;
        moveScript.enabled = true;

        if (scriptToUnlock != null)
        {
            scriptToUnlock.enabled = true;
        }
    }
}

// Enum to easily select the tutorial type in the Inspector
public enum TutorialType
{
    Jump,
    Bridge,
    BlowUp,
    PushBox
}
