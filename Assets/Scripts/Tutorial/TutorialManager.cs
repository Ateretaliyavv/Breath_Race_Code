using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // חובה להוסיף את השורה הזו בשביל המערכת החדשה

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
    // שים לב: שינינו את זה מ-KeyCode ל-Key
    [SerializeField] private Key jumpKey = Key.UpArrow;
    [SerializeField] private Key bridgeKey = Key.Space;
    [SerializeField] private Key blowUpKey = Key.Space;
    [SerializeField] private Key pushKey = Key.Space;

    // משתנים פנימיים
    private MonoBehaviour scriptToUnlock;
    private Key keyToWaitFor; // שומר איזה מקש מהמערכת החדשה אנחנו מחפשים
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
            // בדיקה האם המקלדת מחוברת (כדי למנוע שגיאות)
            if (Keyboard.current == null) return;

            // הפקודה החדשה: בדיקה האם המקש הספציפי נלחץ בפריים הזה
            if (Keyboard.current[keyToWaitFor].wasPressedThisFrame)
            {
                CloseTutorial();
            }
        }
    }

    public void TriggerTutorial(string message, TutorialType type)
    {
        // 1. עצירת השחקן
        moveScript.enabled = false;
        jumpScript.enabled = false;
        blowUpScript.enabled = false;
        bridgeScript.enabled = false;
        pushBox.enabled = false;

        // 2. הגדרת המקש והסקריפט
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

        // 3. עדכון ה-UI
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
