using TMPro;
using UnityEngine;

/*
 * BreathIntroSequence
 * Shows a short breath calibration intro panel with staged targets.
 * While the intro is active, the scene's main gauge is disabled so only the intro gauge reacts.
 */
public class BreathCheckTutorialManager : MonoBehaviour
{
    public static bool HasCompletedBreathCheck = false;

    [Header("UI References")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private TextMeshProUGUI instructionsText;

    [Header("Next Step UI")]
    [SerializeField] private GameObject startTextObject; // "TO START..." text

    [Header("Gauges Control")]
    [Tooltip("Root object of the scene's main gauge (the one that should NOT react during intro).")]
    [SerializeField] private GameObject sceneGaugeRoot;

    [Tooltip("Optional: if you prefer disabling only scripts instead of the whole gauge object.")]
    [SerializeField] private MonoBehaviour[] sceneGaugeScriptsToDisable;

    [Header("Thresholds")]
    [SerializeField] private int stage1Target = 1;
    [SerializeField] private int stage2Target = 3;
    [SerializeField] private int stage3Target = 5;

    [Header("Player Scripts to Lock")]
    [SerializeField] private Move moveScript;
    [SerializeField] private Jump jumpScript;
    [SerializeField] private BlowUpBalloons blowUpScript;

    private int currentStage = 1;
    private float holdTimer = 0f;

    // Initializes the intro flow and ensures only the intro gauge can react.
    private void Start()
    {
        if (HasCompletedBreathCheck)
        {
            EndSequence();
            return;
        }

        if (startTextObject != null)
            startTextObject.SetActive(false);

        StartSequence();
    }

    // Activates intro UI, forces breath mode, locks player, disables the scene gauge.
    private void StartSequence()
    {
        if (GlobalInputModeManager.Instance != null)
            GlobalInputModeManager.Instance.SetBreath();

        SetSceneGaugeEnabled(false);

        introPanel.SetActive(true);
        LockPlayer(true);
        UpdateText();
    }

    // Reads current pressure and advances stages when target is held briefly.
    private void Update()
    {
        if (HasCompletedBreathCheck) return;

        float currentKPa = 0f;
        if (PressureWebSocketReceiver.Instance != null)
            currentKPa = PressureWebSocketReceiver.Instance.lastPressureKPa;

        float target = GetStageTarget(currentStage);

        if (currentKPa >= target)
        {
            holdTimer += Time.deltaTime;
            if (instructionsText) instructionsText.color = Color.green;

            if (holdTimer > 0.5f)
            {
                currentStage++;
                holdTimer = 0f;
                if (instructionsText) instructionsText.color = Color.white;

                if (currentStage > 3)
                {
                    HasCompletedBreathCheck = true;
                    EndSequence();
                }
                else
                {
                    UpdateText();
                }
            }
        }
        else
        {
            holdTimer = 0f;
            if (instructionsText) instructionsText.color = Color.white;
        }
    }

    // Updates the instruction line for the current stage target.
    private void UpdateText()
    {
        int target = (int)GetStageTarget(currentStage);
        if (instructionsText)
            instructionsText.text = $"Please blow until the number reaches {target}";
    }

    // Ends the intro, hides intro panel (and its child gauge), re-enables the scene gauge, unlocks player.
    private void EndSequence()
    {
        introPanel.SetActive(false);

        if (startTextObject != null)
            startTextObject.SetActive(true);

        SetSceneGaugeEnabled(true);
        LockPlayer(false);
    }

    // Enables/disables player control scripts during the intro.
    private void LockPlayer(bool isLocked)
    {
        if (moveScript) moveScript.enabled = !isLocked;
        if (jumpScript) jumpScript.enabled = !isLocked;
        if (blowUpScript) blowUpScript.enabled = !isLocked;
    }

    // Returns the target value for the given stage number.
    private float GetStageTarget(int stage)
    {
        switch (stage)
        {
            case 1: return stage1Target;
            case 2: return stage2Target;
            case 3: return stage3Target;
            default: return stage3Target;
        }
    }

    // Controls whether the scene's main gauge can react during the intro.
    private void SetSceneGaugeEnabled(bool enabled)
    {
        if (sceneGaugeRoot != null)
            sceneGaugeRoot.SetActive(enabled);

        if (sceneGaugeScriptsToDisable != null)
        {
            for (int i = 0; i < sceneGaugeScriptsToDisable.Length; i++)
            {
                if (sceneGaugeScriptsToDisable[i] != null)
                    sceneGaugeScriptsToDisable[i].enabled = enabled;
            }
        }
    }
}

