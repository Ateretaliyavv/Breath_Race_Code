using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

/*
 * Runs in Win or GameOver scenes.
 * Uses LevelProgressData for last level id and stars.
 * Saves each run and updates best stars in Cloud Save.
 */
public class ResultSceneCloudSaver : MonoBehaviour
{
    [Header("Status UI")]
    [SerializeField] private TextMeshProUGUI statusText;

    private async void Start()
    {
        // Read last level data from global holder
        string levelId = LevelProgressData.LastLevelId;
        int stars = LevelProgressData.LastLevelStars;

        if (string.IsNullOrEmpty(levelId))
        {
            Debug.LogWarning("ResultSceneCloudSaver: LastLevelId is empty. Nothing to save.");
            if (statusText != null)
            {
                statusText.text = "No level data to save.";
            }
            return;
        }

        // Show current run data to the player
        if (statusText != null)
        {
            statusText.text = "Level " + levelId + " this run: " + stars + " stars.";
        }

        // Make sure Unity Services are initialized
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogError("ResultSceneCloudSaver: Services init failed. " + e);
                if (statusText != null)
                {
                    statusText.text += "\nServices init failed.";
                }
                return;
            }
        }

        // Check player sign in state
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("ResultSceneCloudSaver: Player is not signed in.");
            if (statusText != null)
            {
                statusText.text += "\nNot signed in. Data not saved.";
            }
            return;
        }

        await SaveRunAndBestForLevelAsync(levelId, stars);
    }

    /*
     * Saves a new run entry and updates best stars for this level.
     * Keys in Cloud Save:
     *   level_<id>_runCount
     *   level_<id>_run_<n>_stars
     *   level_<id>_bestStars
     */
    private async Task SaveRunAndBestForLevelAsync(string levelId, int starsThisRun)
    {
        string bestKey = "level_" + levelId + "_bestStars";
        string runCountKey = "level_" + levelId + "_runCount";

        try
        {
            // Load existing best and run count
            var keysToLoad = new HashSet<string> { bestKey, runCountKey };
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keysToLoad);

            int previousBest = 0;
            bool hasPreviousBest = false;

            if (result.ContainsKey(bestKey))
            {
                previousBest = result[bestKey].Value.GetAs<int>();
                hasPreviousBest = true;
            }

            int previousRunCount = 0;
            if (result.ContainsKey(runCountKey))
            {
                previousRunCount = result[runCountKey].Value.GetAs<int>();
            }

            // Compute new run index
            int newRunIndex = previousRunCount + 1;

            // Compute new best and if it is a new record
            int newBest;
            bool isNewBest;

            if (hasPreviousBest)
            {
                if (starsThisRun > previousBest)
                {
                    newBest = starsThisRun;
                    isNewBest = true;
                }
                else
                {
                    newBest = previousBest;
                    isNewBest = false;
                }
            }
            else
            {
                newBest = starsThisRun;
                isNewBest = true;
            }

            // Key for this specific run
            string runStarsKey = "level_" + levelId + "_run_" + newRunIndex + "_stars";

            // Prepare data to save
            var dataToSave = new Dictionary<string, object>
            {
                { runStarsKey, starsThisRun },
                { runCountKey, newRunIndex },
                { bestKey, newBest }
            };

            // Save to Cloud Save
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);

            Debug.Log(
                "Cloud Save: level " + levelId +
                " run " + newRunIndex +
                " stars " + starsThisRun +
                " best " + newBest
            );

            // Show best info to the player as in the original script
            if (statusText != null)
            {
                if (isNewBest)
                {
                    statusText.text += "\nNew best: " + newBest + " stars.";
                }
                else
                {
                    statusText.text += "\nBest stays: " + newBest + " stars.";
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("ResultSceneCloudSaver: Error while saving data. " + e.Message);
            if (statusText != null)
            {
                statusText.text += "\nError saving data.";
            }
        }
    }
}
