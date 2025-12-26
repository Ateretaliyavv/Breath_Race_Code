using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * This is a static utility class.
 * All scene changes in the project go through this class.
 */
public static class SceneNavigator
{
    // Global flag – true only when moving to the next level
    public static bool IsNextLevel { get; private set; }

    public static void LoadScene(string sceneName, bool markAsNextLevel)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneNavigator: sceneName is empty!");
            return;
        }

        // Mark that the transition was to the next level
        if (markAsNextLevel)
        {
            IsNextLevel = true;
        }

        // When going back to Home (OpenScene) we reset checkpoints
        if (sceneName == "OpenScene")
        {
            // Clear checkpoint of the current open level
            string currentScene = SceneManager.GetActiveScene().name;
            CheckpointManagment.ClearCheckpoint(currentScene);

            // Clear checkpoint of the last played level (in case we came from GameOver)
            if (!string.IsNullOrEmpty(LevelProgressData.LastLevelSceneName))
            {
                CheckpointManagment.ClearCheckpoint(LevelProgressData.LastLevelSceneName);
            }
            // reset stars for a new run
            StarsNumberKeeper.StarsCollected = 0;
            DiamondRunKeeper.ClearAll();   // clear collected diamonds for new run
        }

        // Load requested scene
        SceneManager.LoadScene(sceneName);
    }

    public static void ResetNextLevelFlag()
    {
        IsNextLevel = false;
    }
}
