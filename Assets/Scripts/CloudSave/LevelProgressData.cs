/*
 * Global class that count results for cloud saving
 */
public static class LevelProgressData
{
    public static string Username;

    public static string LastLevelId;
    public static string LastLevelSceneName;
    public static int LastLevelStars;
    public static int CurrentRunDeaths = 0; // count deaths in the current run

    // reset data for a new run
    public static void ResetRunData()
    {
        CurrentRunDeaths = 0;
        LastLevelStars = 0;
    }
}
