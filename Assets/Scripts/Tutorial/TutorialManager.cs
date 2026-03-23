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
    [SerializeField] private Image tutorialImageDisplay;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Input Settings (New System)")]
    [SerializeField] private Key jumpKey = Key.UpArrow;
    [SerializeField] private Key bridgeKey = Key.Space;
    [SerializeField] private Key blowUpKey = Key.Space;
    [SerializeField] private Key pushKey = Key.Space;
    [SerializeField] private Key nextKey = Key.Space;

    private string[] currentMessages;
    private int currentStep = 0;
    private MonoBehaviour scriptToUnlock;
    private bool isTutorialActive = false;
    private Key keyToWaitFor;

    private void Start()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    private void Update()
    {
        if (isTutorialActive)
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current[keyToWaitFor].wasPressedThisFrame)
            {
                ShowNextStep();
            }
        }
    }

    public void TriggerTutorial(string[] messages, TutorialType type, Sprite image)
    {
        currentMessages = messages;
        currentStep = 0;

        TogglePlayerControl(false);
        ConfigureTutorialType(type);
        DisplayCurrentStep(image);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        isTutorialActive = true;
    }

    private void DisplayCurrentStep(Sprite image)
    {
        if (currentMessages == null || currentStep >= currentMessages.Length) return;

        string message = currentMessages[currentStep];
        bool isHebrew = (LocalizationManager.I != null && LocalizationManager.I.CurrentLang == Lang.HE);

        tutorialText.text = isHebrew
            ? RtlTextHelper.FixForceRTL(message, fixTags: true, preserveNumbers: true)
            : message;

        tutorialText.isRightToLeftText = isHebrew;

        if (tutorialImageDisplay != null)
        {
            tutorialImageDisplay.sprite = image;
            tutorialImageDisplay.gameObject.SetActive(image != null);
        }

        if (promptText != null)

        {
            string nextAction = (currentStep < currentMessages.Length - 1) ? "Next" : "Finish";
            promptText.text = $"Press {nextKey} to {nextAction}";
        }
    }

    private void ShowNextStep()
    {
        currentStep++;

        if (currentStep < currentMessages.Length)
        {
            DisplayCurrentStep(tutorialImageDisplay.sprite);
        }
        else
        {
            CloseTutorial();
        }
    }

    private void CloseTutorial()
    {
        isTutorialActive = false;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        TogglePlayerControl(true);

        if (scriptToUnlock != null)
            scriptToUnlock.enabled = true;
    }

    private void TogglePlayerControl(bool state)
    {
        if (moveScript) { moveScript.enabled = state; if (state) moveScript.isPressedUI = true; }
        if (jumpScript) jumpScript.enabled = state;
        if (blowUpScript) blowUpScript.enabled = state;
        if (bridgeScript) bridgeScript.enabled = state;
        if (pushBox) pushBox.enabled = state;
    }

    private void ConfigureTutorialType(TutorialType type)
    {
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
            default:
                scriptToUnlock = null;
                keyToWaitFor = nextKey;
                break;
        }
    }
}

// KEEP THIS HERE, BUT DELETE IT FROM OTHER SCRIPTS
public enum TutorialType
{
    Jump,
    Bridge,
    BlowUp,
    PushBox,
    Flag
}
