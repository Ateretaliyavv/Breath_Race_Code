using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    // Drag your UI Image object from the Canvas here
    [SerializeField] private Image tutorialImageDisplay;

    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Input Settings (New System)")]
    [SerializeField] private Key jumpKey = Key.UpArrow;
    [SerializeField] private Key bridgeKey = Key.Space;
    [SerializeField] private Key blowUpKey = Key.Space;
    [SerializeField] private Key pushKey = Key.Space;

    // Internal state variables
    private MonoBehaviour scriptToUnlock;
    private Key keyToWaitFor;
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

    // Updated method signature to accept the Sprite image
    public void TriggerTutorial(string message, TutorialType type, Sprite image)
    {
        // 1. Disable player controls
        // Added null checks to prevent errors if a script is missing
        if (moveScript) moveScript.enabled = false;
        if (jumpScript) jumpScript.enabled = false;
        if (blowUpScript) blowUpScript.enabled = false;
        if (bridgeScript) bridgeScript.enabled = false;
        if (pushBox) pushBox.enabled = false;

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

        // 3. Update the UI Text
        if (tutorialText != null)
            tutorialText.text = message;

        if (promptText != null)
            promptText.text = "Press " + keyToWaitFor.ToString() + " to continue";

        // --- New Logic: Handle the Image ---
        if (tutorialImageDisplay != null)
        {
            if (image != null)
            {
                // If an image was provided, assign it and show the UI element
                tutorialImageDisplay.sprite = image;
                tutorialImageDisplay.gameObject.SetActive(true);

                // Optional: Preserves aspect ratio so the image doesn't look stretched
                tutorialImageDisplay.preserveAspect = true;
            }
            else
            {
                // If no image is provided, hide the Image UI component
                tutorialImageDisplay.gameObject.SetActive(false);
            }
        }

        // 4. Show the panel
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        isTutorialActive = true;
    }

    private void CloseTutorial()
    {
        isTutorialActive = false;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        // Return control to the player
        if (moveScript)
        {
            moveScript.isPressedUI = true;
            moveScript.enabled = true;
        }

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
