using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;

/*
 * Saves results of the last played level to Unity Cloud Save.
 *
 * Also persists the username in Cloud Save (key "username" by default), so it is guaranteed to exist even when saving results from this scene.
 */

public class ResultSceneCloudSaver : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Cloud Save Keys")]
    [Tooltip("Key used to store the username in Cloud Save.")]
    [SerializeField] private string cloudUsernameKey = "username";

    [Tooltip("Prefix for per-level best stars in Cloud Save, e.g. bestStars_level1.")]
    [SerializeField] private string bestStarsKeyPrefix = "bestStars_";

    [Tooltip("Optional key for last played level id.")]
    [SerializeField] private string lastLevelIdKey = "lastLevelId";

    [Tooltip("Optional key for last played scene name.")]
    [SerializeField] private string lastLevelSceneKey = "lastLevelScene";

    private async void Start()
    {
        await TrySaveResultAsync();
    }

    public async void SaveNow()
    {
        await TrySaveResultAsync();
    }

    private async Task TrySaveResultAsync()
    {
        if (!IsSignedIn())
        {
            Debug.LogWarning("ResultSceneCloudSaver: Not signed in. Skipping Cloud Save.");
            if (statusText != null) statusText.text = "Not signed in. Cannot save.";
            return;
        }

        try
        {
            if (statusText != null) statusText.text = "Saving to cloud...";

            // Always save username to Cloud Save (safe to overwrite with same value).
            await SaveUsernameIfAvailable();

            // Save last played level metadata (optional).
            await SaveLastLevelMeta();

            // Save best stars per level (only updates if better).
            await SaveBestStarsForLastLevel();

            if (statusText != null) statusText.text = "Saved successfully.";
        }
        catch (Exception e)
        {
            Debug.LogError("ResultSceneCloudSaver: Error while saving data. " + e.Message);
            if (statusText != null)
            {
                statusText.text = "Error saving data: " + e.Message;
            }
        }
    }

    private bool IsSignedIn()
    {
        try
        {
            return AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn;
        }
        catch
        {
            return false;
        }
    }

    private async Task SaveUsernameIfAvailable()
    {
        string username = LevelProgressData.Username;

        if (string.IsNullOrEmpty(username))
        {
            // If username was not set in session, do nothing.
            return;
        }

        var data = new Dictionary<string, object>
        {
            { cloudUsernameKey, username }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        Debug.Log("ResultSceneCloudSaver: Saved username: " + username);
    }

    private async Task SaveLastLevelMeta()
    {
        // If you don't use these fields, you can remove this method.
        // It is kept to align with typical level progress saving patterns.
        if (string.IsNullOrEmpty(LevelProgressData.LastLevelId) && string.IsNullOrEmpty(LevelProgressData.LastLevelSceneName))
            return;

        var data = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(LevelProgressData.LastLevelId))
            data[lastLevelIdKey] = LevelProgressData.LastLevelId;

        if (!string.IsNullOrEmpty(LevelProgressData.LastLevelSceneName))
            data[lastLevelSceneKey] = LevelProgressData.LastLevelSceneName;

        if (data.Count == 0)
            return;

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        Debug.Log("ResultSceneCloudSaver: Saved last level meta.");
    }

    private async Task SaveBestStarsForLastLevel()
    {
        // Requires you to set these fields somewhere when finishing a level.
        if (string.IsNullOrEmpty(LevelProgressData.LastLevelId))
        {
            Debug.Log("ResultSceneCloudSaver: LastLevelId is empty, cannot save best stars.");
            if (statusText != null) statusText.text += "\nNo level id to save.";
            return;
        }

        int newBest = LevelProgressData.LastLevelStars;
        string key = bestStarsKeyPrefix + LevelProgressData.LastLevelId;

        try
        {
            // Load current best.
            var keys = new HashSet<string> { key };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

            int currentBest = -1;

            if (result.TryGetValue(key, out var item))
            {
                // Stored as int or string depending on earlier versions.
                // Try both safely.
                try
                {
                    currentBest = item.Value.GetAs<int>();
                }
                catch
                {
                    int.TryParse(item.Value.GetAs<string>(), out currentBest);
                }
            }

            if (newBest > currentBest)
            {
                var data = new Dictionary<string, object> { { key, newBest } };
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);

                Debug.Log($"ResultSceneCloudSaver: Updated best stars for {LevelProgressData.LastLevelId}: {newBest}");
                if (statusText != null)
                {
                    statusText.text += "\nNew best: " + newBest + " stars.";
                }
            }
            else
            {
                Debug.Log($"ResultSceneCloudSaver: Best stars unchanged for {LevelProgressData.LastLevelId}. Current: {currentBest}, New: {newBest}");
                if (statusText != null)
                {
                    statusText.text += "\nBest stays: " + newBest + " stars.";
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ResultSceneCloudSaver: Error while saving best stars. " + e.Message);
            if (statusText != null)
            {
                statusText.text += "\nError saving best stars.";
            }
        }
    }
}
