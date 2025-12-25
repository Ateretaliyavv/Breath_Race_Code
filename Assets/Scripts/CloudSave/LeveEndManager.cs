using UnityEngine;

/*
* Handles end-of-level logic: collects stars amount,
* remembers which level was played, and uses SceneNavigator
* to go to Win / GameOver scenes.
*/

public class LevelEndManager : MonoBehaviour
{
    // Global access from other scripts
    public static LevelEndManager Instance { get; private set; }

    [Header("Level identification")]
    [SerializeField] private string levelId = "Level1";   // set per scene

    [Header("Target scenes")]
    [SerializeField] private string winSceneName = "WinLevel1";
    [SerializeField] private string loseSceneName = "GameOver1";

    [Header("Stars counter in this level")]
    [SerializeField] private NumberFieldUI starsCounter;  // your diamonds/stars UI

    private void Awake()
    {
        Instance = this;
    }

    // Called when the player reaches the end-of-level object
    public void PlayerWon()
    {
        EndLevel(winSceneName, true);
    }

    // Called when the camera decides the player is out of view (Game Over)
    public void PlayerLost()
    {
        EndLevel(loseSceneName, false);
    }

    private void EndLevel(string sceneName, bool markAsNextLevel)
    {
        if (starsCounter == null)
        {
            Debug.LogError("LevelEndManager: starsCounter is not assigned!");
            return;
        }

        // TODO: replace CurrentValue with your real property if it's called differently
        int starsThisRun = starsCounter.GetNumberUI();

        // Remember what happened in this level
        LevelProgressData.LastLevelId = levelId;
        LevelProgressData.LastLevelStars = starsThisRun;

        // Use YOUR navigation system
        SceneNavigator.LoadScene(sceneName, markAsNextLevel);
    }
}
