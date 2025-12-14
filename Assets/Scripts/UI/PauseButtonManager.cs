using UnityEngine;
using UnityEngine.UI;

/*
 * SimplePauseManager:
 * - True pause using Time.timeScale
 * - Toggles pause state
 * - Swaps button icon (Pause / Play)
 * - Shows a simple "PAUSE" panel
 */
public class PauseButtonManager : MonoBehaviour
{
    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;   // Panel with "PAUSE" text
    [SerializeField] private Image pauseButtonImage;  // Image component of the pause button
    [SerializeField] private Sprite pauseIcon;        // Icon when game is running
    [SerializeField] private Sprite playIcon;         // Icon when game is paused

    private bool isPaused = false;

    private void Start()
    {
        // Safety: ensure correct initial state
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButtonImage != null && pauseIcon != null)
            pauseButtonImage.sprite = pauseIcon;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
            Pause();
        else
            Resume();
    }

    private void Pause()
    {
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (pauseButtonImage != null && playIcon != null)
            pauseButtonImage.sprite = playIcon;
    }

    private void Resume()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseButtonImage != null && pauseIcon != null)
            pauseButtonImage.sprite = pauseIcon;
    }

    private void OnDestroy()
    {
        // Safety: never leave the game paused
        Time.timeScale = 1f;
    }
}
