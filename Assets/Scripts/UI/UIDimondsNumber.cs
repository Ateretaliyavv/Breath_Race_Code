using TMPro;
using UnityEngine;

public class UIDimondsNumber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starsText;

    private void Start()
    {
        //Take the number of stars collected from StarsNumberKeeper
        int stars = StarsNumberKeeper.StarsCollected;

        // Update the text to show the number of stars collected
        if (starsText != null)
            starsText.text = stars.ToString();
    }
}
