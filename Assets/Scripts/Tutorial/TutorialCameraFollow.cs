using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * 2D Camera:
 * - Moves forward on X at a constant speed, but only while the player is walking (IsWalking in Animator).
 * - Follows the player's Y smoothly when moving.
 * - If the player stays out of camera view for some time it is Game Over.
 */

[RequireComponent(typeof(Camera))]
public class TutorialCameraFollow : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] Transform target;      // Player to follow
    [SerializeField] Vector3 offset;        // Offset mainly for Y

    [Header("Vertical Follow")]
    [SerializeField] float verticalSmoothTime = 0.2f;

    [Header("Horizontal Scroll")]
    [SerializeField] float cameraSpeedX = 5f;     // Put the same value as Move.speed

    [Header("Game Over")]
    [SerializeField] string gameOverSceneName = "GameOverScene";
    [SerializeField] float outOfViewThreshold = 0.5f;

    [Header("Player Animation")]
    [SerializeField] Animator playerAnimator;          // Animator that has "IsWalking"
    [SerializeField] string isWalkingParam = "isWalking";

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
        // Auto-find Animator if not assigned in Inspector
        if (playerAnimator == null && target != null)
        {
            playerAnimator = target.GetComponent<Animator>();
            if (playerAnimator == null)
            {
                playerAnimator = target.GetComponentInChildren<Animator>();
            }
        }
    }

    private void LateUpdate()
    {
        if (!target || gameOverTriggered)
            return;

        bool isWalking = false;
        if (playerAnimator != null && !string.IsNullOrEmpty(isWalkingParam))
        {
            isWalking = playerAnimator.GetBool(isWalkingParam);
        }

        // Move camera only while player is walking
        if (isWalking)
        {
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
        }

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
