/*
 * Global class that stores results for cloud saving + run counters.
 */
public static class LevelProgressData
{
    public static string Username;

    public static string LastLevelId;
    public static string LastLevelSceneName;

    // Diamonds collected in the last finished run (Win or GameOver)
    public static int LastLevelStars;

    // Retries / fails during the CURRENT run
    public static int CurrentRunDeaths = 0;

    public static void ResetRunData()
    {
        LastLevelStars = 0;
        CurrentRunDeaths = 0;
    }
}
