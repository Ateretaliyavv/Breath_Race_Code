using System.Collections;
using TMPro;
using UnityEngine;

/*
 * BreathCheckTutorialManager
 * 3-step breath tutorial (1 -> 3 -> 5) that requires "release" between steps.
 * Disables the scene main gauge during the tutorial, then re-enables it and starts the game.
 *
 * Localization:
 * - Uses LocalizedTMP on the instructions text and swaps CSV keys per stage.
 * - LocalizedTMP handles translation + RTL + alignment automatically.
 */
public class BreathCheckTutorialManager : MonoBehaviour
{
    public static bool HasCompletedBreathCheck = false;

    [Header("UI References")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private TextMeshProUGUI instructionsText; // Must have LocalizedTMP component

    [Header("Scene Gauge (Main Gauge)")]
    [Tooltip("The main breath gauge of the scene (disabled during the tutorial).")]
    [SerializeField] private GameObject sceneGaugeRoot;

    [Header("Thresholds")]
    [SerializeField] private int stage1Target = 1;
    [SerializeField] private int stage2Target = 3;
    [SerializeField] private int stage3Target = 5;

    [Header("Release Between Steps")]
    [Tooltip("Player must drop below this value after completing a step, to unlock the next step.")]
    [SerializeField] private float releaseThresholdKPa = 0.5f;

    [Header("Timing")]
    [SerializeField] private float successCloseDelay = 3f;

    [Header("Localization Keys (CSV)")]
    [Tooltip("EN/HE text for stage 1 instruction.")]
    [SerializeField] private string stage1Key = "BLOW_STAGE_1";
    [Tooltip("EN/HE text for stage 2 instruction.")]
    [SerializeField] private string stage2Key = "BLOW_STAGE_3";
    [Tooltip("EN/HE text for stage 3 instruction.")]
    [SerializeField] private string stage3Key = "BLOW_STAGE_5";
    [Tooltip("EN/HE text for success message.")]
    [SerializeField] private string successKey = "GOOD_JOB";

    [Header("Player Scripts to Lock")]
    [SerializeField] private Move moveScript;
    [SerializeField] private Jump jumpScript;
    [SerializeField] private BlowUpBalloons blowUpScript;

    [Header("Start with ENTER text")]
    [SerializeField] private GameObject startTextObject;

    private LocalizedTMP localized; // Cached LocalizedTMP on instructionsText

    private int currentStage = 1;
    private bool closingStarted = false;
    private bool waitingForRelease = false;

    private void Awake()
    {
        // Cache LocalizedTMP once (required for runtime key swaps).
        if (instructionsText != null)
            localized = instructionsText.GetComponent<LocalizedTMP>();
    }

    // Starts the tutorial unless it was already completed.
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

    // Enables tutorial UI, locks player, forces breath mode, and disables the scene gauge.
    private void StartSequence()
    {
        if (GlobalInputModeManager.Instance != null)
            GlobalInputModeManager.Instance.SetBreath();

        if (sceneGaugeRoot != null)
            sceneGaugeRoot.SetActive(false);

        if (introPanel != null)
            introPanel.SetActive(true);

        LockPlayer(true);
        UpdateTextForStage();
    }

    // Advances stages only when target is reached AND a release happened between steps.
    private void Update()
    {
        if (HasCompletedBreathCheck || closingStarted) return;

        float currentKPa = 0f;
        if (PressureWebSocketReceiver.Instance != null)
            currentKPa = PressureWebSocketReceiver.Instance.lastPressureKPa;

        if (waitingForRelease)
        {
            if (currentKPa <= releaseThresholdKPa)
                waitingForRelease = false;

            return;
        }

        int target = GetStageTarget(currentStage);

        if (currentKPa >= target)
        {
            currentStage++;
            waitingForRelease = true;

            if (currentStage > 3)
            {
                HasCompletedBreathCheck = true;
                closingStarted = true;
                ShowSuccessAndClose();
            }
            else
            {
                UpdateTextForStage();
            }
        }
    }

    // Updates the tutorial text based on the current stage.
    // Now uses localization keys via LocalizedTMP instead of hardcoded strings.
    private void UpdateTextForStage()
    {
        if (!instructionsText) return;

        if (localized == null)
        {
            // Fallback: avoid crash; shows key if LocalizedTMP is missing.
            Debug.LogError("BreathCheckTutorialManager: instructionsText is missing LocalizedTMP.");
            return;
        }

        localized.SetKey(GetStageKey(currentStage));
    }

    // Shows success text and closes the panel after a short delay.
    // Now uses successKey via LocalizedTMP instead of hardcoded string.
    private void ShowSuccessAndClose()
    {
        if (localized != null)
            localized.SetKey(successKey);
        else if (instructionsText != null)
            instructionsText.text = $"#{successKey}"; // Minimal fallback

        StartCoroutine(CloseAfterDelay());
    }

    // Waits, then ends the tutorial and starts the game.
    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(successCloseDelay);
        EndSequence();
    }

    // Hides tutorial UI, re-enables the scene gauge, and unlocks player controls.
    private void EndSequence()
    {
        if (introPanel != null)
            introPanel.SetActive(false);

        if (sceneGaugeRoot != null)
            sceneGaugeRoot.SetActive(true);

        if (startTextObject != null)
            startTextObject.SetActive(true);

        LockPlayer(false);
    }

    // Locks/unlocks the player scripts during the tutorial.
    private void LockPlayer(bool isLocked)
    {
        if (moveScript) moveScript.enabled = !isLocked;
        if (jumpScript) jumpScript.enabled = !isLocked;
        if (blowUpScript) blowUpScript.enabled = !isLocked;
    }

    // Returns the numeric threshold for the given stage.
    private int GetStageTarget(int stage)
    {
        switch (stage)
        {
            case 1: return stage1Target;
            case 2: return stage2Target;
            case 3: return stage3Target;
            default: return stage3Target;
        }
    }

    // Returns the localization key for the given stage.
    private string GetStageKey(int stage)
    {
        switch (stage)
        {
            case 1: return stage1Key;
            case 2: return stage2Key;
            case 3: return stage3Key;
            default: return stage3Key;
        }
    }
}
