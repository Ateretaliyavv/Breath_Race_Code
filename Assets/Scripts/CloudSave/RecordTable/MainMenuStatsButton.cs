using UnityEngine;
using UnityEngine.UI;

public class MainMenuStatsButton : MonoBehaviour
{
    [SerializeField] private Button statsButton;
    [SerializeField] private GameObject statsImage;
    [SerializeField] private PlayerStatsLoader statsLoader;

    private void Start()
    {
        RefreshButtonState();

        if (statsButton != null)
        {
            statsButton.onClick.RemoveAllListeners();
            statsButton.onClick.AddListener(() =>
            {
                if (statsLoader != null)
                    statsLoader.ShowStats();
            });
        }
    }

    public void RefreshButtonState()
    {
        bool isLoggedIn = !string.IsNullOrEmpty(LevelProgressData.Username);

        if (statsButton != null)
            statsButton.gameObject.SetActive(isLoggedIn);
        if (statsImage != null)
            statsImage.SetActive(isLoggedIn);
    }
}
