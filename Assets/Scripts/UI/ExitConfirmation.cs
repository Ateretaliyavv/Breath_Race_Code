using UnityEngine;

public class ExitConfirmation : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign the Panel GameObject that contains the confirmation text and buttons.")]
    [SerializeField] private GameObject confirmationPanel;

    [Header("Settings")]
    [Tooltip("The name of the scene to load when the user confirms exit (e.g., 'OpenScene').")]
    [SerializeField] private string sceneToLoad = "OpenScene";

    [Header("Run Reset Options")]
    [Tooltip("If true, resets run data before leaving (same as GoToSceneByClick).")]
    [SerializeField] private bool resetRunDataForGuest = true;

    private float previousTimeScale = 1f;

    private void Start()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    // Call this from the in-game 'Back' button
    public void ShowConfirmation()
    {
        // Guest: exit immediately + reset run data if enabled.
        if (string.IsNullOrEmpty(LevelProgressData.Username))
        {
            if (resetRunDataForGuest)
                DiamondRunKeeper.ClearAll(); // Same reset as GoToSceneByClick

            ConfirmExit();
            return;
        }

        // Logged-in: show confirmation panel
        if (confirmationPanel != null)
        {
            previousTimeScale = Time.timeScale; // Save current state
            confirmationPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game
        }
        else
        {
            // Fallback if panel is missing
            ConfirmExit();
        }
    }

    // Call this from the 'Yes' button (or automatically for guests)
    public void ConfirmExit()
    {
        Time.timeScale = 1f; // Always unpause before leaving
        SceneNavigator.LoadScene(sceneToLoad, markAsNextLevel: false);
    }

    // Call this from the 'No' button
    public void CancelExit()
    {
        Time.timeScale = previousTimeScale; // Restore previous state
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
}
