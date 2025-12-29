using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Manages tutorial popups that pause the game and show instructions.
 * Disables player controls during the tutorial.
 * Re-enables specific abilities after the tutorial is confirmed.
 */
public class TutorialManager : MonoBehaviour
{
    [Header("Player Scripts References")]
    [SerializeField] private Move moveScript;
    [SerializeField] private Jump jumpScript;
    [SerializeField] private BlowUpBalloons blowUpScript;
    [SerializeField] private BridgeBuilder bridgeScript;
    [SerializeField] private PushBox pushBox;

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel; // The main UI panel background
    [SerializeField] private TextMeshProUGUI tutorialText;        // The text element for instructions
    [SerializeField] private Button confirmButton;     // The "Play" button

    // Tracks which script should be unlocked after the tutorial popup is closed
    private MonoBehaviour scriptToUnlock;

    private void Start()
    {
        // Ensure the tutorial UI is hidden at game start
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        // Setup the button listener
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);
    }


    public void TriggerTutorial(string message, TutorialType type)
    {
        // Disable all player controls immediately
        moveScript.enabled = false;
        jumpScript.enabled = false;
        blowUpScript.enabled = false;
        bridgeScript.enabled = false;
        pushBox.enabled = false;

        // Determine which ability to unlock based on the trigger type
        switch (type)
        {
            case TutorialType.Jump:
                scriptToUnlock = jumpScript;
                break;
            case TutorialType.Bridge:
                scriptToUnlock = bridgeScript;
                break;
            case TutorialType.BlowUp:
                scriptToUnlock = blowUpScript;
                break;
            case TutorialType.PushBox:
                scriptToUnlock = pushBox;
                break;

        }

        // Update and show the UI
        if (tutorialText != null)
            tutorialText.text = message;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    private void OnConfirmClicked()
    {

        tutorialPanel.SetActive(false);

        moveScript.isPressedUI = true;
        moveScript.enabled = true;

        // Re-enable the specific learned skill
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
