using UnityEngine;
using UnityEngine.SceneManagement;

/*
* Handles end-of-level logic: collects diamonds amount,
* remembers which level was played, and navigates to Win / GameOver scenes.
*/
public class LevelEndManager : MonoBehaviour
{
    public static LevelEndManager Instance { get; private set; }

    [Header("Level identification")]
    [SerializeField] private string levelId = "Level1";   // set per scene

    [Header("Target scenes")]
    [SerializeField] private string winSceneName = "WinLevel1";
    [SerializeField] private string loseSceneName = "GameOver1";

    [Header("Diamonds counter in this level")]
    [SerializeField] private NumberFieldUI starsCounter;  // your diamonds UI

    private void Awake()
    {
        Instance = this;
    }

    public void PlayerWon()
    {
        EndLevel(winSceneName, true);
    }

    public void PlayerLost()
    {
        //Count EXACTLY ONE retry per loss (works with or without checkpoint)
        LevelProgressData.CurrentRunDeaths++;

        EndLevel(loseSceneName, false);
    }

    private void EndLevel(string sceneName, bool markAsNextLevel)
    {
        if (starsCounter == null)
        {
            Debug.LogError("LevelEndManager: starsCounter is not assigned!");
            return;
        }

        int diamondsThisRun = starsCounter.GetNumberUI();

        LevelProgressData.LastLevelId = levelId;
        LevelProgressData.LastLevelSceneName = SceneManager.GetActiveScene().name;
        LevelProgressData.LastLevelStars = diamondsThisRun;

        SceneNavigator.LoadScene(sceneName, markAsNextLevel);
    }
}
