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
* Handles user registration & login using Unity Authentication and then loads the main game scene.
* The player enters ONLY a username.
* A fixed password is used for ALL users.
* The username is saved to Cloud Save under a dedicated key.
*/

public class LoginManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput; // Optional: can be null / removed from UI
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Scene To Load After Login")]
    [SerializeField] private string sceneToLoad = "Scene1";

    private string fixedPassword = "Password2026!!";

    [Header("Cloud Save Keys")]
    [Tooltip("Key used to store the username in Cloud Save.")]
    [SerializeField] private string cloudUsernameKey = "username";

    [Tooltip("Example key for other cloud data (kept from the original script if you used it).")]
    [SerializeField] private string cloudHighScoreKey = "highScore";

    private async void Start()
    {
        // Disable password UI if exists, since we use a fixed password.
        if (passwordInput != null)
        {
            passwordInput.text = string.Empty;
            passwordInput.interactable = false;
            passwordInput.gameObject.SetActive(false);
        }

        await InitUnityServicesIfNeeded();
    }

    private async Task InitUnityServicesIfNeeded()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                if (statusText != null) statusText.text = "Initializing services...";
                await UnityServices.InitializeAsync();
            }

            if (statusText != null) statusText.text = "Ready. Enter username.";
        }
        catch (Exception e)
        {
            Debug.LogError("Unity Services init failed: " + e.Message);
            if (statusText != null) statusText.text = "Init failed: " + e.Message;
        }
    }

    public async void Register()
    {
        string username = GetTrimmedUsername();
        if (string.IsNullOrEmpty(username))
        {
            SetStatus("Please enter a username.");
            return;
        }

        try
        {
            SetStatus("Registering...");

            // If a user is already signed in, sign out first to avoid conflicts.
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, fixedPassword);

            // Save username for the session + persist to Cloud Save.
            await SaveUsernameToCloud(username);

            SetStatus("Registration successful. Loading...");
            LoadNextScene();
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("Register failed: " + e.Message);
            SetStatus("Register failed: " + e.Message);
        }
        catch (RequestFailedException e)
        {
            Debug.LogError("Register failed: " + e.Message);
            SetStatus("Register failed: " + e.Message);
        }
        catch (Exception e)
        {
            Debug.LogError("Register failed: " + e.Message);
            SetStatus("Register failed: " + e.Message);
        }
    }

    public async void Login()
    {
        string username = GetTrimmedUsername();
        if (string.IsNullOrEmpty(username))
        {
            SetStatus("Please enter a username.");
            return;
        }

        try
        {
            SetStatus("Logging in...");

            // If a user is already signed in, sign out first.
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();

            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, fixedPassword);

            // Save username for the session + persist to Cloud Save.
            await SaveUsernameToCloud(username);

            SetStatus("Login successful. Loading...");
            LoadNextScene();
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("Login failed: " + e.Message);
            SetStatus("Login failed: " + e.Message);
        }
        catch (RequestFailedException e)
        {
            Debug.LogError("Login failed: " + e.Message);
            SetStatus("Login failed: " + e.Message);
        }
        catch (Exception e)
        {
            Debug.LogError("Login failed: " + e.Message);
            SetStatus("Login failed: " + e.Message);
        }
    }

    private string GetTrimmedUsername()
    {
        if (usernameInput == null)
            return string.Empty;

        return usernameInput.text.Trim();
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("LoginManager: sceneToLoad is empty.");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }

    private async Task SaveUsernameToCloud(string username)
    {
        try
        {
            // Keep it in a global static holder for easy access across scenes.
            LevelProgressData.Username = username;

            var data = new Dictionary<string, object>
            {
                { cloudUsernameKey, username }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log("Saved username to Cloud Save: " + username);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save username to Cloud Save: " + e.Message);
            // Not fatal for gameplay; user can continue.
        }
    }

    // Optional helper if you previously loaded some values like high score.
    public async void LoadHighScoreIfExists()
    {
        try
        {
            var keys = new HashSet<string> { cloudHighScoreKey };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            if (result.TryGetValue(cloudHighScoreKey, out var item))
            {
                string highScore = item.Value.GetAs<string>();
                Debug.Log("Loaded high score: " + highScore);

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
