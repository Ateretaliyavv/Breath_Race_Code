using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
* Handles user registration & login using Unity Authentication
* and then loads the main game scene.
*/

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TextMeshProUGUI statusText; // Optional: for displaying messages

    [Header("Scene To Load After Login")]
    [SerializeField] private string sceneToLoad = "OpenScene";

    private bool servicesInitialized = false;

    // Initialization
    private async void Awake()
    {
        // Initialize Unity Services
        try
        {
            await UnityServices.InitializeAsync();
            servicesInitialized = true;
            Debug.Log("Unity Services initialized successfully.");

            if (statusText != null)
                statusText.text = "Please log in or register.";
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to initialize Unity Services: " + e);
            if (statusText != null)
                statusText.text = "Error: Failed to initialize services.";
        }
    }


    // Called by the "Register" button
    public async void OnRegisterClicked()
    {
        if (!servicesInitialized)
        {
            if (statusText != null)
                statusText.text = "Services not initialized yet.";
            return;
        }

        string user = usernameInput.text.Trim();
        string pass = passwordInput.text;

        // Basic validation
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            if (statusText != null)
                statusText.text = "Username and password must not be empty.";
            return;
        }

        try
        {
            // Create a new account with username + password
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(user, pass);
            Debug.Log("SignUp successful. Player ID: " + AuthenticationService.Instance.PlayerId);

            if (statusText != null)
                statusText.text = "Registration successful! Logging in...";

            await AfterLoginAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("SignUp failed: " + e.Message);
            if (statusText != null)
                statusText.text = "Registration failed: " + e.Message;
        }
    }

    // Called by the "Login" button
    public async void OnLoginClicked()
    {
        if (!servicesInitialized)
        {
            if (statusText != null)
                statusText.text = "Services not initialized yet.";
            return;
        }

        string user = usernameInput.text.Trim();
        string pass = passwordInput.text;

        // Basic validation
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            if (statusText != null)
                statusText.text = "Username and password must not be empty.";
            return;
        }

        try
        {
            // Log in with username + password
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(user, pass);
            Debug.Log("SignIn successful. Player ID: " + AuthenticationService.Instance.PlayerId);

            if (statusText != null)
                statusText.text = "Login successful! Loading game...";

            await AfterLoginAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("SignIn failed: " + e.Message);
            if (statusText != null)
                statusText.text = "Login failed: " + e.Message;
        }
    }

    // What happens AFTER successful login / registration
    private async Task AfterLoginAsync()
    {
        // Optional: load player data from Cloud Save before entering the game
        await LoadPlayerDataAsync();

        // Load the main game scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("sceneToLoad is empty. Please set it in the Inspector.");
        }
    }

    // Save a simple value to Cloud Save after login.
    public async Task SavePlayerScoreAsync(int score)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                { "highScore", score }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("Score saved to Cloud Save: " + score);
        }
        catch (Exception e)
        {
            Debug.LogError("Cloud Save failed: " + e.Message);
        }
    }

    // Load data from Cloud Save on login
    private async Task LoadPlayerDataAsync()
    {
        try
        {
            var keys = new HashSet<string> { "highScore" };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (result.ContainsKey("highScore"))
            {
                int highScore = result["highScore"].Value.GetAs<int>();
                Debug.Log("Loaded high score from Cloud Save: " + highScore);

                if (statusText != null)
                    statusText.text = "Loaded high score: " + highScore;
            }
            else
            {
                Debug.Log("No high score found in Cloud Save. New player.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Cloud Load failed: " + e.Message);
        }
    }
}
