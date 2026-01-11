using UnityEngine;
using UnityEngine.SceneManagement;

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
        // Ensure the confirmation window is hidden when the level starts
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    // 1. Call this method from your main 'Back' button in the game UI
    public void ShowConfirmation()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true); // Show the window
            Time.timeScale = 0f;               // Pause the game time (stops physics and movement)
        }
    }

    // 2. Call this method from the 'Yes' button inside the confirmation panel
    public void ConfirmExit()
    {
        // IMPORTANT: Always reset time scale to 1 before loading a new scene,
        // otherwise the next scene might start paused.
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToLoad);
    }

    // 3. Call this method from the 'No' button inside the confirmation panel
    public void CancelExit()
    {
        Time.timeScale = 1f; // Resume game time (unpause)

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false); // Hide the window and return to game
        }
    }
}
