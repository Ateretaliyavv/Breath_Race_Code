using TMPro;
using UnityEngine;

public class GameOverStars : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starsText;

    private void Start()
    {
        //Take the number of stars collected from StarsNumberKeeper
        int stars = StarsNumberKeeper.StarsCollected;

        // Update the text to show the number of stars collected
        starsText.text = stars.ToString();
    }
}
