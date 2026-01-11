using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

public class ResultSceneCloudSaver : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    private const string BestDiamondsPrefix = "bestDiamonds_";
    private const string BestRetriesForBestDiamondsPrefix = "bestRetriesForBestDiamonds_";

    private async void Start()
    {
        await SaveAndShowAsync();
    }

    private async Task SaveAndShowAsync()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("UnityServices init failed: " + e.Message);
            if (statusText) statusText.text = "Cloud init failed.";
            return;
        }

        string levelId = LevelProgressData.LastLevelId;
        if (string.IsNullOrEmpty(levelId))
            levelId = "UnknownLevel";

        int runDiamonds = LevelProgressData.LastLevelStars;
        int runRetries = LevelProgressData.CurrentRunDeaths;

        string bestDiamondsKey = BestDiamondsPrefix + levelId;
        string bestRetriesKey = BestRetriesForBestDiamondsPrefix + levelId;

        int savedBestDiamonds = 0;
        int savedBestRetriesForBestDiamonds = int.MaxValue;

        Dictionary<string, string> loaded = null;

        try
        {
            var keys = new HashSet<string> { bestDiamondsKey, bestRetriesKey };
            loaded = await CloudSaveService.Instance.Data.LoadAsync(keys);
        }
        catch
        {
            // No data yet
        }

        savedBestDiamonds = GetInt(loaded, bestDiamondsKey, 0);
        savedBestRetriesForBestDiamonds = GetInt(loaded, bestRetriesKey, int.MaxValue);

        bool shouldSave = false;
        int finalBestDiamonds = savedBestDiamonds;
        int finalBestRetries = savedBestRetriesForBestDiamonds;

        if (runDiamonds > savedBestDiamonds)
        {
            finalBestDiamonds = runDiamonds;
            finalBestRetries = runRetries;
            shouldSave = true;
        }
        else if (runDiamonds == savedBestDiamonds)
        {
            if (savedBestRetriesForBestDiamonds == int.MaxValue || runRetries < savedBestRetriesForBestDiamonds)
            {
                finalBestRetries = runRetries;
                shouldSave = true;
            }
        }

        if (shouldSave)
        {
            // Save as strings to match LoadAsync(Dictionary<string,string>)
            var data = new Dictionary<string, object>
            {
                { bestDiamondsKey, finalBestDiamonds.ToString() },
                { bestRetriesKey,  finalBestRetries.ToString() }
            };

            try
            {
                await CloudSaveService.Instance.Data.ForceSaveAsync(data);
            }
            catch (Exception e)
            {
                Debug.LogError("Cloud save failed: " + e.Message);
                if (statusText) statusText.text = $"Diamonds this run: {runDiamonds}\n" + "Guest - The information is not saved";
                return;
            }
        }

        int bestDiamondsToShow = shouldSave ? finalBestDiamonds : savedBestDiamonds;

        if (statusText)
        {
            statusText.text =
                $"Diamonds this run: {runDiamonds}\n" +
                $"Best diamonds (this level): {bestDiamondsToShow}";
        }
    }

    private int GetInt(Dictionary<string, string> loaded, string key, int defaultValue)
    {
        if (loaded != null && loaded.TryGetValue(key, out string s) && int.TryParse(s, out int v))
            return v;
        return defaultValue;
    }
}
