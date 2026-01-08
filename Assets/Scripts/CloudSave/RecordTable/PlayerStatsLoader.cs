using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.CloudSave;
using UnityEngine;

public class PlayerStatsLoader : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentContainer;   // The parent object for the list items
    [SerializeField] private GameObject statRowPrefab;     // The prefab for a single row in the table
    [SerializeField] private TextMeshProUGUI statusText;   // Text for "Loading..." or error messages
    [SerializeField] private GameObject statsWindow;       // The main panel to show/hide

    // These keys must match the ones used in 'ResultSceneCloudSaver'
    private const string BestDiamondsPrefix = "bestDiamonds_";
    private const string BestDeathsPrefix = "bestDeaths_";

    // Internal class to hold data before creating the UI
    private class LevelStat
    {
        public int BestDiamonds = 0;
        public int BestDeaths = -1; // -1 indicates the level was never completed
    }

    public async void ShowStats()
    {
        // 1. Check if the user is logged in
        if (string.IsNullOrEmpty(LevelProgressData.Username))
        {
            if (statusText) statusText.text = "Please Log In";
            return;
        }

        // 2. Prepare the UI
        if (statsWindow != null) statsWindow.SetActive(true);
        ClearTable();
        if (statusText) statusText.text = "Loading...";

        try
        {
            // 3. Load ALL data for the player from Cloud Save
            var data = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
            Dictionary<string, LevelStat> parsedStats = new Dictionary<string, LevelStat>();

            // 4. Process each saved item
            foreach (var item in data)
            {
                string key = item.Key;
                int value = 0;

                // --- ROBUST DATA CONVERSION ---
                // Cloud Save data can arrive as different types (long, int, or JSON wrapper).
                // Using 'GetAs<object>()' and then 'Convert.ToInt32' is the safest way to handle this
                // without getting InvalidCastException.
                try
                {
                    value = Convert.ToInt32(item.Value.Value.GetAs<object>());
                }
                catch
                {
                    value = 0; // Fallback if data is corrupted
                }
                // ------------------------------

                // Check if this key is a "Diamonds" record
                if (key.StartsWith(BestDiamondsPrefix))
                {
                    string levelId = key.Replace(BestDiamondsPrefix, "");

                    if (!parsedStats.ContainsKey(levelId))
                        parsedStats[levelId] = new LevelStat();

                    parsedStats[levelId].BestDiamonds = value;
                }
                // Check if this key is a "Deaths/Retries" record
                else if (key.StartsWith(BestDeathsPrefix))
                {
                    string levelId = key.Replace(BestDeathsPrefix, "");

                    if (!parsedStats.ContainsKey(levelId))
                        parsedStats[levelId] = new LevelStat();

                    parsedStats[levelId].BestDeaths = value;
                }
            }

            // 5. Generate the UI Table
            if (parsedStats.Count == 0)
            {
                if (statusText) statusText.text = "No records yet.";
            }
            else
            {
                if (statusText) statusText.text = ""; // Clear the loading text

                // Sort levels alphabetically (e.g., Level 1, Level 2...)
                List<string> sortedKeys = new List<string>(parsedStats.Keys);
                sortedKeys.Sort();

                foreach (string levelId in sortedKeys)
                {
                    var stat = parsedStats[levelId];
                    CreateStatRow(levelId, stat.BestDiamonds, stat.BestDeaths);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading stats: " + e.Message);
            if (statusText) statusText.text = "Error loading data.";
        }
    }

    private void CreateStatRow(string levelName, int diamonds, int deaths)
    {
        if (statRowPrefab == null) return;

        // Instantiate the row and ensure its scale is correct
        GameObject row = Instantiate(statRowPrefab, contentContainer);
        row.transform.localScale = Vector3.one;

        TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();

        // Assumes the Prefab has 3 Text components in this order:
        // Index 0: Level Name
        // Index 1: Diamond Count
        // Index 2: Death Count
        if (texts.Length >= 3)
        {
            texts[0].text = levelName;
            texts[1].text = diamonds.ToString();

            // Logic: If deaths is -1, it means the level was never completed.
            // We show an empty string instead of "-1".
            if (deaths == -1)
                texts[2].text = "";
            else
                texts[2].text = deaths.ToString();
        }
    }

    private void ClearTable()
    {
        // Destroy all existing rows to avoid duplicates
        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);
    }

    public void CloseWindow()
    {
        if (statsWindow != null) statsWindow.SetActive(false);
    }
}
