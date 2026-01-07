using UnityEngine;

/*
 * Script that manages the camera following a game object
 */
[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;      // Player to follow
    [SerializeField] private Vector3 offset;        // Mainly for Y

    [Header("Vertical Follow")]
    [SerializeField] private float verticalSmoothTime = 0.2f;

    [Header("Horizontal Scroll")]
    [SerializeField] private float cameraSpeedX = 5f;

    [Header("Player Animation")]
    [SerializeField] private Animator playerAnimator;     // Animator with Idle state
    [SerializeField] private string idleStateName = "Idle";

    [Header("Game Over")]
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private float outOfViewThreshold = 0.5f;

    private float verticalVelocity;
    private Camera cam;
    private bool gameOverTriggered = false;
    private float outOfViewTimer = 0f;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        // Try to auto-find the animator on the player
        if (playerAnimator == null && target != null)
        {
            playerAnimator = target.GetComponent<Animator>();
            if (playerAnimator == null)
                playerAnimator = target.GetComponentInChildren<Animator>();
        }
    }

    private void LateUpdate()
    {
        if (!target || gameOverTriggered)
            return;

        bool isIdle = IsPlayerIdle();

        // Move the camera only when the player is NOT idle
        if (!isIdle)
        {
            Vector3 pos = transform.position;

            // Constant forward movement on X
            pos.x += cameraSpeedX * Time.deltaTime;

            // Smooth follow on Y
            float targetY = target.position.y + offset.y;
            pos.y = Mathf.SmoothDamp(
                pos.y,
                targetY,
                ref verticalVelocity,
                verticalSmoothTime
            );

            pos.z = transform.position.z;
            transform.position = pos;
        }

        // Game Over check
        CheckOutOfViewAndHandleGameOver();
    }

    // Returns true if the player is currently in the Idle animation state
    private bool IsPlayerIdle()
    {
        if (playerAnimator == null)
            return false;

        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(idleStateName);
    }

    // Checks if the player left the camera view and triggers Game Over
    private void CheckOutOfViewAndHandleGameOver()
    {
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

                // Centralized scene loading
                if (LevelEndManager.Instance != null)
                {
                    LevelEndManager.Instance.PlayerLost();
                }
                else
                {
                    Debug.LogError("LevelEndManager.Instance is null!");
                }
            }
        }
        else
        {
            outOfViewTimer = 0f;
        }
    }

    // Call this after respawn: move the camera immediately to the target (player).
    // Accepts an optional customOffset. If null, uses the default script offset.
    public void SnapToTargetImmediately(Vector3? customOffset = null)
    {
        if (target == null)
            return;

        // Use customOffset if provided, otherwise use the default 'offset' field
        Vector3 finalOffset = customOffset ?? offset;

        Vector3 pos = transform.position;
        pos.x = target.position.x + finalOffset.x;
        pos.y = target.position.y + finalOffset.y;
        pos.z = transform.position.z;
        transform.position = pos;

        // Reset game-over timers so we won't instantly trigger GameOver again
        outOfViewTimer = 0f;
        gameOverTriggered = false;
    }
}
