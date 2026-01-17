using System.Collections;
using TMPro;
using UnityEngine;

/*
 * BreathCheckTutorialManager
 * 3-step breath tutorial (1 -> 3 -> 5) that requires "release" between steps.
 * Disables the scene main gauge during the tutorial, then re-enables it and starts the game.
 */
public class BreathCheckTutorialManager : MonoBehaviour
{
    public static bool HasCompletedBreathCheck = false;

    [Header("UI References")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private TextMeshProUGUI instructionsText;

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

    [Header("Hebrew UI Text")]
    [SerializeField] private string stage1Text = "Blow with strength\n1";
    [SerializeField] private string stage2Text = "Blow with strength\n3";
    [SerializeField] private string stage3Text = "Blow with strength\n5";
    [SerializeField] private string successText = "Good Job!";

    [Header("Player Scripts to Lock")]
    [SerializeField] private Move moveScript;
    [SerializeField] private Jump jumpScript;
    [SerializeField] private BlowUpBalloons blowUpScript;

    [Header("Start with ENTER text")]
    [SerializeField] private GameObject startTextObject;

    private int currentStage = 1;
    private bool closingStarted = false;
    private bool waitingForRelease = false;

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
    private void UpdateTextForStage()
    {
        if (!instructionsText) return;

        switch (currentStage)
        {
            case 1: instructionsText.text = stage1Text; break;
            case 2: instructionsText.text = stage2Text; break;
            case 3: instructionsText.text = stage3Text; break;
        }
    }

    // Shows success text and closes the panel after a short delay.
    private void ShowSuccessAndClose()
    {
        if (instructionsText)
            instructionsText.text = successText;

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
}
