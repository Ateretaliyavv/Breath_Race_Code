using UnityEngine;
using UnityEngine.EventSystems;

public class SoundManager : MonoBehaviour
{
    [Header("UI References")]
    // Instead of swapping images, we reference the X overlay object
    [SerializeField] private GameObject xOverlayObject;

    private bool isMuted = false;

    void Start()
    {
        UpdateIcon();
    }

    public void ToggleSound()
    {
        isMuted = !isMuted;

        // Control global volume
        AudioListener.volume = isMuted ? 0 : 1;

        UpdateIcon();

        // Remove UI focus so Enter won't trigger again
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void UpdateIcon()
    {
        // If muted -> enable the X overlay
        // If not muted -> disable the X overlay
        if (xOverlayObject != null)
        {
            xOverlayObject.SetActive(isMuted);
        }
    }
}
