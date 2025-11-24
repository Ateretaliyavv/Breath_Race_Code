using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * 2D Camera:
 * - Moves forward on X at a constant speed, but only after the player actually started moving.
 * - Follows the player's Y smoothly.
 * - If the player stays out of camera view for some time it is Game Over.
 */

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] Transform target;      // Player to follow
    [SerializeField] Vector3 offset;        // Offset mainly for Y

    [Header("Vertical Follow")]
    [SerializeField] float verticalSmoothTime = 0.2f;

    [Header("Horizontal Scroll")]
    [SerializeField] float cameraSpeedX = 5f;     // Put the same value as Move.speed

    [Header("Game Over")]
    [SerializeField] string gameOverSceneName = "GameOver";
    [SerializeField] float outOfViewThreshold = 0.5f;

    private float verticalVelocity;
    private Camera cam;
    private bool gameOverTriggered = false;
    private float outOfViewTimer = 0f;

    // Whether the camera has started moving
    private bool started = false;

    // The player's initial X position
    private float initialPlayerX;

    // How much movement counts as "started moving"
    private const float startMoveEpsilon = 0.01f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (target != null)
        {
            initialPlayerX = target.position.x;
        }
    }

    private void LateUpdate()
    {
        if (!target || gameOverTriggered)
            return;

        // The camera does not move until the player actually moves on X
        if (!started)
        {
            float dx = Mathf.Abs(target.position.x - initialPlayerX);
            if (dx < startMoveEpsilon)
            {
                // Player has not moved yet → do not move the camera
                return;
            }
            else
            {
                // Player started moving → camera begins scrolling
                started = true;
            }
        }

        // Move the camera forward on X at a constant speed
        Vector3 pos = transform.position;
        pos.x += cameraSpeedX * Time.deltaTime;

        // Smoothly follow the player's Y
        float targetY = target.position.y + offset.y;
        pos.y = Mathf.SmoothDamp(
            pos.y,
            targetY,
            ref verticalVelocity,
            verticalSmoothTime
        );

        pos.z = transform.position.z;
        transform.position = pos;

        // Check if the player is still inside the camera view
        Vector3 viewPos = cam.WorldToViewportPoint(target.position);

        bool isOutOfView =
            viewPos.z < 0f ||
            viewPos.x < 0f || viewPos.x > 1f ||
            viewPos.y < 0f || viewPos.y > 1f;

        if (isOutOfView)
        {
            outOfViewTimer += Time.deltaTime;

            if (outOfViewTimer >= outOfViewThreshold)
            {
                gameOverTriggered = true;
                SceneManager.LoadScene(gameOverSceneName);
            }
        }
        else
        {
            outOfViewTimer = 0f;
        }
    }
}
