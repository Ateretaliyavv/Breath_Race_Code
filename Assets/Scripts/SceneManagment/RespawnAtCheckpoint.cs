using UnityEngine;
using UnityEngine.SceneManagement;

// On level start, moves the player to the last checkpoint position
public class RespawnAtCheckpoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CameraFollow2D cameraFollow;  //Main Camera

    [Header("Position offset above checkpoint")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, 0f);

    private void Start()
    {
        string levelName = SceneManager.GetActiveScene().name;

        // Check if there is a saved checkpoint for this level
        if (CheckpointManagment.TryGetCheckpoint(levelName, out Vector3 checkpointPos))
        {
            Vector3 newPlayerPos = checkpointPos + offset;
            player.position = newPlayerPos;

            // Optional: reset velocity if the player has Rigidbody2D
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            // Now snap the camera to the player
            if (cameraFollow != null)
            {
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
