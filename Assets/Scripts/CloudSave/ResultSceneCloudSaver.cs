using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

public class ResultSceneCloudSaver : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Check this ONLY if this script is in the WIN scene. Uncheck for Game Over scene.")]
    [SerializeField] private bool isWinScene = true;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Cloud Save Keys")]
    [SerializeField] private string bestDiamondsKeyPrefix = "bestDiamonds_";
    [SerializeField] private string bestDeathsKeyPrefix = "bestDeaths_";
    [SerializeField] private string lastLevelIdKey = "lastLevelId";

    private async void Start()
    {
        await SaveAndShowAsync();
        LevelProgressData.CurrentRunDeaths = 0;
    }

    private async Task SaveAndShowAsync()
    {
        if (statusText != null) statusText.text = "";

        // 1. Get data from the current run
        int runDiamonds = Mathf.Max(0, DiamondRunKeeper.DimondsCollected);
        int runDeaths = LevelProgressData.CurrentRunDeaths;

        string levelId = LevelProgressData.LastLevelId;
        if (string.IsNullOrEmpty(levelId)) levelId = "UnknownLevel";

        // Check if Guest
        if (string.IsNullOrEmpty(LevelProgressData.Username))
        {
            if (statusText) statusText.text = $"Diamonds: {runDiamonds}\n(Guest Mode)";
            return;
        }

        // Initialize Services
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
        }
        catch { }

        string bestDiamondsKey = bestDiamondsKeyPrefix + levelId;
        string bestDeathsKey = bestDeathsKeyPrefix + levelId;

        int savedBestDiamonds = 0;
        int savedBestDeaths = -1; // -1 indicates the level has never been completed

        // 2. Load previous bests from Cloud
        try
        {
            var loaded = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { bestDiamondsKey, bestDeathsKey });

            if (loaded.TryGetValue(bestDiamondsKey, out var dItem))
                savedBestDiamonds = Convert.ToInt32(dItem.Value.GetAs<object>());

            if (loaded.TryGetValue(bestDeathsKey, out var deathItem))
                savedBestDeaths = Convert.ToInt32(deathItem.Value.GetAs<object>());
        }
        catch
        {
            Debug.Log("No previous record found (or load error).");
        }

        // 3. Calculate new records

        // Diamonds: Always take the maximum found
        int finalBestDiamonds = Mathf.Max(savedBestDiamonds, runDiamonds);

        // Deaths: Calculate only if the level was completed (Win Scene)
        int finalBestDeaths = savedBestDeaths;

        if (isWinScene)
        {
            // Update if it's the first win (-1) OR if the current run has fewer deaths than the saved record
            if (savedBestDeaths == -1 || runDeaths < savedBestDeaths)
            {
                finalBestDeaths = runDeaths;
            }
        }

        // 4. Save to Cloud
        try
        {
            var data = new Dictionary<string, object>
            {
                [lastLevelIdKey] = levelId,
                [bestDiamondsKey] = finalBestDiamonds,
                [bestDeathsKey] = finalBestDeaths
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(data);

            // Only display Diamonds info in the result screen (as requested)
            if (statusText)
            {
                statusText.text = $"Diamonds: {runDiamonds} (Best: {finalBestDiamonds})\nRetries: {runDeaths}";
            }
        }
        catch (Exception e)
        {
            if (statusText) statusText.text = "Save Failed.";
            Debug.LogError(e.Message);
        }
    }
}
