using UnityEngine;
using UnityEngine.EventSystems;

public class SoundManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject xOverlayObject;

    private bool isMuted = false;

    void Start()
    {
        if (AudioListener.volume == 0)
        {
            isMuted = true;
        }
        else
        {
            isMuted = false;
        }

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
        if (xOverlayObject != null)
        {
            xOverlayObject.SetActive(isMuted);
        }
    }
}
