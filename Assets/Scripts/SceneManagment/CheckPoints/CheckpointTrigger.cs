using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class CheckpointTrigger : MonoBehaviour
{
    private bool _activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_activated) return;
        if (!other.CompareTag("Player")) return;   // make sure your player has tag "Player"

        string levelName = SceneManager.GetActiveScene().name;
        Vector3 checkpointPos = transform.position;

        // Save checkpoint for this level
        CheckpointManagment.SetCheckpoint(levelName, checkpointPos);

        _activated = true;

        Debug.Log($"Checkpoint activated in {levelName} at {checkpointPos}");
    }
}
