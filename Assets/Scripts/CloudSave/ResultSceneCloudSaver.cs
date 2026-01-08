using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

/*
 * This script runs in the Win / GameOver scenes.
 * It displays the diamonds collected in the last run and the best (highest) number of diamonds achieved in the same level so far.
 */

public class ResultSceneCloudSaver : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Cloud Save Keys")]
    [SerializeField] private string cloudUsernameKey = "username";
    [SerializeField] private string bestDiamondsKeyPrefix = "bestDiamonds_";
    [SerializeField] private string lastLevelIdKey = "lastLevelId";
    [SerializeField] private string lastLevelSceneKey = "lastLevelScene";

    private async void Start()
    {
        await SaveAndShowAsync();
    }

    private async Task SaveAndShowAsync()
    {
        if (statusText == null)
            return;

        // Diamonds for THIS run come from DiamondRunKeeper
        int diamondsThisRun = Mathf.Max(0, DiamondRunKeeper.DimondsCollected);

        // Level id must be set before loading Win/GameOver
        string levelId = LevelProgressData.LastLevelId;
        if (string.IsNullOrEmpty(levelId))
            levelId = "UnknownLevel";

        bool isGuest = string.IsNullOrEmpty(LevelProgressData.Username);

        if (isGuest)
        {
            statusText.text =
                $"Diamonds this run: {diamondsThisRun}\n" +
                "Nothing was saved to the cloud (Guest).";
            return;
        }

        // Init Unity Services
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("ResultSceneCloudSaver: UnityServices init failed: " + e.Message);
            statusText.text =
                $"Diamonds this run: {diamondsThisRun}\n" +
                "Could not access cloud services.";
            return;
        }

        string bestKey = bestDiamondsKeyPrefix + levelId;
        int currentBest = 0;

        // Load best
        try
        {
            var loaded = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { bestKey });

            if (loaded.TryGetValue(bestKey, out var item))
                currentBest = Convert.ToInt32(item);
        }
        catch (Exception e)
        {
            Debug.LogWarning("ResultSceneCloudSaver: Load best failed: " + e.Message);
            // continue; we'll treat as 0
        }

        int newBest = Mathf.Max(currentBest, diamondsThisRun);

        // Save
        try
        {
            var data = new Dictionary<string, object>
            {
                [cloudUsernameKey] = LevelProgressData.Username,
                [lastLevelIdKey] = levelId,
                [bestKey] = newBest
            };

            if (!string.IsNullOrEmpty(LevelProgressData.LastLevelSceneName))
                data[lastLevelSceneKey] = LevelProgressData.LastLevelSceneName;

            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        }
        catch (Exception e)
        {
            Debug.LogError("ResultSceneCloudSaver: Cloud save failed: " + e.Message);
            statusText.text =
                $"Diamonds this run: {diamondsThisRun}\n" +
                $"Best diamonds: {newBest} (not saved)";
            return;
        }

        // Final UI (2 lines)
        statusText.text =
            $"Diamonds this run: {diamondsThisRun}\n" +
            $"Best diamonds: {newBest}";
    }
}
