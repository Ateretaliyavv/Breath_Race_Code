using UnityEngine;
using UnityEngine.SceneManagement;

// On level start, moves the player to the last checkpoint position
public class RespawnAtCheckpoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CameraFollow2D cameraFollow;  // Main Camera

    [Header("Player Position Settings")]
    [SerializeField] private Vector3 playerSpawnOffset = new Vector3(0f, 1f, 0f);

    [Header("Camera Position Settings")]
    [Tooltip("If checked, use the custom offset below for the camera instead of its default.")]
    [SerializeField] private bool overrideCameraOffset = true;

    [Tooltip("The camera's distance from the player when respawning at a checkpoint (X and Y).")]
    [SerializeField] private Vector3 cameraRespawnOffset = new Vector3(4f, 0.7f, 0f);

    private void Start()
    {
        string levelName = SceneManager.GetActiveScene().name;

        // Check if there is a saved checkpoint for this level
        if (CheckpointManagment.TryGetCheckpoint(levelName, out Vector3 checkpointPos))
        {
            // 1. Move the Player
            Vector3 newPlayerPos = checkpointPos + playerSpawnOffset;
            player.position = newPlayerPos;

            // Reset velocity if the player has Rigidbody2D
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            // 2. Snap the Camera
            if (cameraFollow != null)
            {
                if (overrideCameraOffset)
                {
                    // Use the custom offset defined in this script
                    cameraFollow.SnapToTargetImmediately(cameraRespawnOffset);
                }
                else
                {
                    // Use the default offset defined in CameraFollow2D
                    cameraFollow.SnapToTargetImmediately();
                }
            }

            Debug.Log($"[RespawnAtCheckpoint] Player moved to checkpoint at {newPlayerPos} in {levelName}");
        }
        else
        {
            Debug.Log($"[RespawnAtCheckpoint] No checkpoint for {levelName}, starting at default spawn.");
        }
    }
}
