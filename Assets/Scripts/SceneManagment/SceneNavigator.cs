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

        if (markAsNextLevel)
        {
            IsNextLevel = true;
        }

        // When going back to Home (OpenScene) we reset checkpoints AND run data
        if (sceneName == "OpenScene")
        {
            string currentScene = SceneManager.GetActiveScene().name;
            CheckpointManagment.ClearCheckpoint(currentScene);

            if (!string.IsNullOrEmpty(LevelProgressData.LastLevelSceneName))
            {
                CheckpointManagment.ClearCheckpoint(LevelProgressData.LastLevelSceneName);
            }

            //THIS is where retries & diamonds are reset for a new run
            LevelProgressData.ResetRunData();
        }

        SceneManager.LoadScene(sceneName);
    }

    public static void ResetNextLevelFlag()
    {
        IsNextLevel = false;
    }
}
