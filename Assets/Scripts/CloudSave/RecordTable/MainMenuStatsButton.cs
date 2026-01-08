using UnityEngine;
using UnityEngine.UI;

public class MainMenuStatsButton : MonoBehaviour
{
    [SerializeField] private Button statsButton;
    [SerializeField] private PlayerStatsLoader statsLoader; // Reference to the PlayerStatsLoader script

    private void Start()
    {
        // Check if there is a valid username saved in LevelProgressData to determine login status
        bool isLoggedIn = !string.IsNullOrEmpty(LevelProgressData.Username);

        if (statsButton != null)
        {
            // Toggle the button's visibility based on whether the user is logged in
            statsButton.gameObject.SetActive(isLoggedIn);

            // Setup the button click event
            statsButton.onClick.RemoveAllListeners();
            statsButton.onClick.AddListener(() =>
            {
                if (statsLoader != null)
                    statsLoader.ShowStats();
            });
        }
    }

    // Public method to refresh the button state
    public void RefreshButtonState()
    {
        Start();
    }
}
