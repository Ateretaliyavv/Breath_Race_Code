using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

/// <summary>
/// Runs in Win / GameOver scenes.
/// Reads LevelProgressData (last level + stars),
/// and updates the BEST stars for that level in Cloud Save.
/// </summary>
public class ResultSceneCloudSaver : MonoBehaviour
{
    [Header("Optional UI to show status")]
    [SerializeField] private TextMeshProUGUI statusText;

    private async void Start()
    {
        string levelId = LevelProgressData.LastLevelId;
        int stars = LevelProgressData.LastLevelStars;

        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning("ResultSceneCloudSaver: no LastLevelId, nothing to save.");
            return;
        }

        if (statusText != null)
            statusText.text = $"Level {levelId}: this run {stars} stars.";

        // Make sure services are initialized
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to initialize Unity Services in result scene: " + e);
                return;
            }
        }

        // Player must be signed in (from your login scene)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("ResultSceneCloudSaver: player not signed in, cannot save to cloud.");
            return;
        }

        await SaveBestForLevelAsync(levelId, stars);
    }

    private async Task SaveBestForLevelAsync(string levelId, int starsThisRun)
    {
        string key = $"level_{levelId}_bestStars";

        try
        {
            var keys = new HashSet<string> { key };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            int previousBest = 0;
            bool hasPrevious = false;

            if (result.ContainsKey(key))
            {
                previousBest = result[key].Value.GetAs<int>();
                hasPrevious = true;
            }

            int newBest = hasPrevious ? Mathf.Max(previousBest, starsThisRun) : starsThisRun;

            if (!hasPrevious || newBest != previousBest)
            {
                var data = new Dictionary<string, object>
                {
                    { key, newBest }
                };

                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                Debug.Log($"Cloud Save: best for {levelId} updated to {newBest} stars.");

                if (statusText != null)
                    statusText.text += $"\nBest: {newBest} stars (saved).";
            }
            else
            {
                Debug.Log($"Cloud Save: no update. Existing best for {levelId} = {previousBest}.");
                if (statusText != null)
                    statusText.text += $"\nBest stays: {previousBest} stars.";
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving best stars to Cloud Save: " + e.Message);
            if (statusText != null)
                statusText.text += "\nError saving to cloud.";
        }
    }
}
