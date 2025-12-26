using System.Collections.Generic;
using UnityEngine;

// Stores the last checkpoint position per level.
// Static so it survives scene changes.
public static class CheckpointManagment
{
    // Key = level name (scene name), Value = position of last checkpoint
    private static Dictionary<string, Vector3> checkpoints = new Dictionary<string, Vector3>();

    // Save / update the checkpoint position for a given level
    public static void SetCheckpoint(string levelName, Vector3 position)
    {
        checkpoints[levelName] = position;
        Debug.Log($"[CheckpointSystem] Checkpoint set for {levelName} at {position}");
    }

    // Try to get the last checkpoint position for a given level
    public static bool TryGetCheckpoint(string levelName, out Vector3 position)
    {
        return checkpoints.TryGetValue(levelName, out position);
    }

    // Clear checkpoint for a level (optional)
    public static void ClearCheckpoint(string levelName)
    {
        if (checkpoints.ContainsKey(levelName))
            checkpoints.Remove(levelName);
    }
}
