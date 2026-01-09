using UnityEngine;
using UnityEngine.SceneManagement;

// On level start, moves the player to the last checkpoint position (if exists)
// IMPORTANT: does NOT count retries here (retries are counted only in LevelEndManager.PlayerLost)
public class RespawnAtCheckpoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CameraFollow2D cameraFollow;

    [Header("Player Position Settings")]
    [SerializeField] private Vector3 playerSpawnOffset = new Vector3(0f, 1f, 0f);

    [Header("Camera Position Settings")]
    [SerializeField] private bool overrideCameraOffset = true;
    [SerializeField] private Vector3 cameraRespawnOffset = new Vector3(4f, 0.7f, 0f);

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("[RespawnAtCheckpoint] Player is not assigned!");
            return;
        }

        string levelName = SceneManager.GetActiveScene().name;

        // If checkpoint exists -> move player + snap camera
        if (CheckpointManagment.TryGetCheckpoint(levelName, out Vector3 checkpointPos))
        {
            Vector3 newPlayerPos = checkpointPos + playerSpawnOffset;
            player.position = newPlayerPos;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (cameraFollow != null)
            {
                if (overrideCameraOffset)
                    cameraFollow.SnapToTargetImmediately(cameraRespawnOffset);
                else
                    cameraFollow.SnapToTargetImmediately();
            }

            Debug.Log($"[RespawnAtCheckpoint] Player moved to checkpoint at {newPlayerPos} in {levelName}");
        }
        else
        {
            Debug.Log($"[RespawnAtCheckpoint] No checkpoint for {levelName}, starting at default spawn.");
        }
    }
}
