using System.Collections.Generic;

public static class DiamondRunKeeper
{
    // Stores keys of collected diamonds for the current run
    private static HashSet<string> collected = new HashSet<string>();

    public static void MarkCollected(string key)
    {
        collected.Add(key);
    }

    public static bool IsCollected(string key)
    {
        return collected.Contains(key);
    }

    public static void ClearAll()
    {
        collected.Clear();
    }
}
