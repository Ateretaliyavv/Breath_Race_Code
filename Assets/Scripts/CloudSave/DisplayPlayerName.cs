using TMPro;
using UnityEngine;

public class DisplayPlayerName : MonoBehaviour
{
    // Reference to the TextMeshPro component where the message will be displayed
    [SerializeField] private TextMeshProUGUI welcomeText;

    void Start()
    {
        // Retrieve the username saved during the login process
        // (The LoginManager script stored this in the static LevelProgressData class)
        string name = LevelProgressData.Username;

        // Check if the name is empty or null (e.g., if you started this scene directly without logging in)
        // If so, default to "Player"
        if (string.IsNullOrEmpty(name))
        {
            name = "";
        }

        // Update the text on the screen
        // "\n" creates a new line to match your design layout
        if (welcomeText != null)
        {
            welcomeText.text = $"WELCOME {name}\nCHOOSE HOW TO PLAY:";
        }
    }
}
