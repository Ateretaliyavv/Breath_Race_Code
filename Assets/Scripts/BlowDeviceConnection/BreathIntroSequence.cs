using TMPro;
using UnityEngine;

public class BreathIntroSequence : MonoBehaviour
{
    // Static flag: persists across scene reloads (e.g., when the player retries).
    // This ensures the calibration only happens once per session.
    public static bool HasCompletedBreathCheck = false;

    [Header("UI References")]
    [SerializeField] private GameObject introPanel;       // The background panel for the sequence
    [SerializeField] private TextMeshProUGUI instructionsText; // The changing instruction text

    [Header("Next Step UI")]
    [SerializeField] private GameObject startTextObject;

    [Header("Thresholds")]
    // Target pressure values (kPa) the player needs to reach
    [SerializeField] private int stage1Target = 1;
    [SerializeField] private int stage2Target = 3;
    [SerializeField] private int stage3Target = 5;

    [Header("Player Scripts to Lock")]
    // References to player scripts to disable movement during calibration
    [SerializeField] private Move moveScript;
    [SerializeField] private Jump jumpScript;
    [SerializeField] private BlowUpBalloons blowUpScript;

    private int currentStage = 1;
    private float holdTimer = 0f;

    private void Start()
    {
        // 1. Check if the calibration was already done in this session
        if (HasCompletedBreathCheck)
        {
            EndSequence(); // If yes, close immediately
            return;
        }
        if (startTextObject != null)
            startTextObject.SetActive(false);

        // 2. If not, start the sequence
        StartSequence();
    }

    private void StartSequence()
    {
        // Ensure Input Mode is set to 'Breath' so the pressure bar is visible
        if (GlobalInputModeManager.Instance != null)
            GlobalInputModeManager.Instance.SetBreath();

        introPanel.SetActive(true);
        LockPlayer(true); // Disable player controls
        UpdateText();     // Show the first instruction
    }

    private void Update()
    {
        // If already completed, do nothing
        if (HasCompletedBreathCheck) return;

        // Get current pressure from the WebSocket receiver
        float currentKPa = 0f;
        if (PressureWebSocketReceiver.Instance != null)
            currentKPa = PressureWebSocketReceiver.Instance.lastPressureKPa;

        // Determine the target based on the current stage
        float target = 0;
        switch (currentStage)
        {
            case 1: target = stage1Target; break;
            case 2: target = stage2Target; break;
            case 3: target = stage3Target; break;
        }

        // Check if the player has reached the required pressure
        if (currentKPa >= target)
        {
            // Accumulate time to ensure the breath is steady (not just a spike)
            holdTimer += Time.deltaTime;
            instructionsText.color = Color.green; // Visual feedback

            if (holdTimer > 0.5f) // Required hold duration (0.5 seconds)
            {
                currentStage++;
                holdTimer = 0;
                instructionsText.color = Color.white;

                if (currentStage > 3)
                {
                    // All stages completed successfully
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
            // Reset timer if pressure drops below target
            holdTimer = 0;
            instructionsText.color = Color.white;
        }
    }

    private void UpdateText()
    {
        int target = 0;
        switch (currentStage)
        {
            case 1: target = stage1Target; break;
            case 2: target = stage2Target; break;
            case 3: target = stage3Target; break;
        }

        instructionsText.text = $"Please blow until the number reaches {target}";
    }

    private void EndSequence()
    {
        introPanel.SetActive(false);
        if (startTextObject != null)
            startTextObject.SetActive(true);
        LockPlayer(false); // Re-enable player controls
    }

    private void LockPlayer(bool isLocked)
    {
        if (moveScript) moveScript.enabled = !isLocked;
        if (jumpScript) jumpScript.enabled = !isLocked;
        if (blowUpScript) blowUpScript.enabled = !isLocked;
    }
}
