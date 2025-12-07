using TMPro;
using UnityEngine;
using UnityEngine.UI; // Use TMPro if you prefer TextMeshPro

public class TutorialManager : MonoBehaviour
{
    [Header("Player Scripts References")]
    [SerializeField] private Move moveScript;
    [SerializeField] private Jump jumpScript;
    [SerializeField] private BridgeBuilder bridgeScript;

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel; // The main UI panel background
    [SerializeField] private TextMeshProUGUI tutorialText;        // The text element for instructions
    [SerializeField] private Button confirmButton;     // The "OK" button

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

    /// <summary>
    /// Called by a TutorialTrigger when the player enters a zone.
    /// Pauses the player and shows the instructions.
    /// </summary>
    public void TriggerTutorial(string message, TutorialType type)
    {
        // 1. Disable all player controls immediately
        // This triggers OnDisable() in the scripts, stopping physics/input
        moveScript.enabled = false;
        jumpScript.enabled = false;
        bridgeScript.enabled = false;

        // 2. Determine which ability to unlock based on the trigger type
        switch (type)
        {
            case TutorialType.Jump:
                scriptToUnlock = jumpScript;
                break;
            case TutorialType.Bridge:
                scriptToUnlock = bridgeScript;
                break;
                // You can add cases here (e.g., Shoot, Dash)
        }

        // 3. Update and show the UI
        if (tutorialText != null)
            tutorialText.text = message;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    /// <summary>
    /// Called when the player clicks the UI button.
    /// Resumes the game and unlocks the specific ability.
    /// </summary>
    private void OnConfirmClicked()
    {
        // 1. Hide the UI
        tutorialPanel.SetActive(false);

        // 2. Always re-enable movement so the player can proceed
        moveScript.isPressedUI = true;
        moveScript.enabled = true;

        // 3. Re-enable the specific learned skill
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
    Bridge
}
