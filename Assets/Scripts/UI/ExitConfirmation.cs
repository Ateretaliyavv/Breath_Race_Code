using UnityEngine;

public class ExitConfirmation : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign the Panel GameObject that contains the confirmation text and buttons.")]
    [SerializeField] private GameObject confirmationPanel;

    [Header("Settings")]
    [Tooltip("The name of the scene to load when the user confirms exit (e.g., 'OpenScene').")]
    [SerializeField] private string sceneToLoad = "OpenScene";

    private void Start()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    // Call this from the in-game 'Back' button
    public void ShowConfirmation()
    {
        // if user is a Guest (no username), exit immediately without asking.
        if (string.IsNullOrEmpty(LevelProgressData.Username))
        {
            ConfirmExit();
            return;
        }

        // If Logged In, show the warning panel
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game
        }
    }

    // Call this from the 'Yes' button (or automatically for guests)
    public void ConfirmExit()
    {
        Time.timeScale = 1f; // Always unpause

        // Use SceneNavigator to clean up checkpoints/data and load the menu
        SceneNavigator.LoadScene(sceneToLoad, false);
    }

    // Call this from the 'No' button
    public void CancelExit()
    {
        Time.timeScale = 1f; // Resume game

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }
}
