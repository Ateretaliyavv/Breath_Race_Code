using System.Collections.Generic;
using UnityEngine;

// Stores the last checkpoint position per level.
// Static so it survives scene changes.
public static class CheckpointManagment
{
    private static Dictionary<string, Vector3> checkpoints = new Dictionary<string, Vector3>();

    public static void SetCheckpoint(string levelName, Vector3 position)
    {
        checkpoints[levelName] = position;
        Debug.Log($"[CheckpointSystem] Checkpoint set for {levelName} at {position}");
    }

    public static bool TryGetCheckpoint(string levelName, out Vector3 position)
    {
        return checkpoints.TryGetValue(levelName, out position);
    }

    public static void ClearCheckpoint(string levelName)
    {
        if (checkpoints.ContainsKey(levelName))
            checkpoints.Remove(levelName);
    }
}
