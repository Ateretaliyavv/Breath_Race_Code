using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using UnityEngine;

public class PlayerStatsLoader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject statRowPrefab;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject statsWindow;

    [Header("Levels (must match LevelEndManager.levelId)")]
    [SerializeField] private string[] levelIds = new string[] { "Level1", "Level2", "Level3" };

    private const string BestDiamondsPrefix = "bestDiamonds_";
    private const string BestRetriesForBestDiamondsPrefix = "bestRetriesForBestDiamonds_";

    public async void ShowStats()
    {
        if (statsWindow != null) statsWindow.SetActive(true);

        await EnsureServicesAsync();
        ClearTable();

        if (statusText) statusText.text = "Loading...";

        // Build keys
        var keys = new HashSet<string>();
        foreach (var levelId in levelIds)
        {
            keys.Add(BestDiamondsPrefix + levelId);
            keys.Add(BestRetriesForBestDiamondsPrefix + levelId);
        }

        Dictionary<string, Item> loaded;

        try
        {
            // New API (not obsolete)
            loaded = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
        }
        catch (Exception e)
        {
            Debug.LogError("Stats load failed: " + e.Message);
            if (statusText) statusText.text = "Failed to load stats.";
            return;
        }

        for (int i = 0; i < levelIds.Length; i++)
        {
            string levelId = levelIds[i];

            int bestDiamonds = GetInt(loaded, BestDiamondsPrefix + levelId, 0);
            int bestRetries = GetInt(loaded, BestRetriesForBestDiamondsPrefix + levelId, 0);

            CreateRow(i + 1, bestDiamonds, bestRetries);
        }

        if (statusText) statusText.text = "";
    }

    private async Task EnsureServicesAsync()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("UnityServices init failed: " + e.Message);
        }
    }

    private int GetInt(Dictionary<string, Item> loaded, string key, int defaultValue)
    {
        if (loaded != null && loaded.TryGetValue(key, out Item item))
        {
            // CloudSave stores values serialized; easiest is to read as string then parse
            string s = item.Value.GetAsString();
            if (int.TryParse(s, out int v))
                return v;
        }
        return defaultValue;
    }

    private void CreateRow(int levelNumber, int bestDiamonds, int bestRetries)
    {
        if (statRowPrefab == null || contentContainer == null) return;

        var row = Instantiate(statRowPrefab, contentContainer);

        // Expect 3 TMP texts: Level | Best | Retries
        var texts = row.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (texts.Length >= 3)
        {
            texts[0].text = levelNumber.ToString();
            texts[1].text = bestDiamonds.ToString();
            texts[2].text = bestRetries.ToString();
        }
    }

    private void ClearTable()
    {
        if (contentContainer == null) return;
        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);
    }

    public void CloseWindow()
    {
        if (statsWindow != null) statsWindow.SetActive(false);
    }
}
