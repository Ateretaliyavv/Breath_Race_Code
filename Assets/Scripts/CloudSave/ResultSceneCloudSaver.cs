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
        if (statusText) statusText.text = TrFixed("CLOUD_LOADING");
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
            if (statusText) statusText.text = TrFixed("CLOUD_INIT_FAILED");
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

                if (statusText)
                {
                    // Show run result + guest notice via CSV templates.
                    string line1 = TrFormat("RESULT_DIAMONDS_RUN", runDiamonds);
                    string line2 = TrFixed("CLOUD_GUEST_NOT_SAVED");
                    statusText.text = FixIfHebrew($"{line1}\n{line2}");
                }
                return;
            }
        }

        int bestDiamondsToShow = shouldSave ? finalBestDiamonds : savedBestDiamonds;

        if (statusText)
        {
            // Build message via CSV templates.
            string line1 = TrFormat("RESULT_DIAMONDS_RUN", runDiamonds);
            string line2 = TrFormat("RESULT_BEST_DIAMONDS_LEVEL", bestDiamondsToShow);
            statusText.text = FixIfHebrew($"{line1}\n{line2}");
        }
    }

    // Format a localized template with args.
    private string TrFormat(string key, params object[] args)
    {
        string template = (LocalizationManager.I != null) ? LocalizationManager.I.Tr(key) : $"#{key}";
        string raw = string.Format(template, args);
        return FixIfHebrew(raw);
    }

    // Get localized text and apply RTL fix if needed.
    private string TrFixed(string key)
    {
        string raw = (LocalizationManager.I != null) ? LocalizationManager.I.Tr(key) : $"#{key}";
        return FixIfHebrew(raw);
    }

    // Apply RTL fix only when Hebrew is selected.
    private string FixIfHebrew(string raw)
    {
        bool isHebrew = (LocalizationManager.I != null && LocalizationManager.I.CurrentLang == Lang.HE);
        return isHebrew ? RtlTextHelper.FixForceRTL(raw, fixTags: true, preserveNumbers: true) : raw;
    }

    private int GetInt(Dictionary<string, string> loaded, string key, int defaultValue)
    {
        if (loaded != null && loaded.TryGetValue(key, out string s) && int.TryParse(s, out int v))
            return v;
        return defaultValue;
    }
}
