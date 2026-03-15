using TMPro;
using UnityEngine;

// Passes the player name as a dynamic argument to the localized welcome text.
public class DisplayPlayerName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI welcomeText;

    private LocalizedTMP localizedTmp;

    // Caches the LocalizedTMP component from the target text object.
    private void Awake()
    {
        if (welcomeText != null)
            localizedTmp = welcomeText.GetComponent<LocalizedTMP>();
    }

    // Reads the saved player name and sends it to the localized text component.
    private void Start()
    {
        string name = LevelProgressData.Username;

        if (string.IsNullOrEmpty(name))
            name = "";

        if (localizedTmp != null)
            localizedTmp.SetArgs(name);
    }
}
