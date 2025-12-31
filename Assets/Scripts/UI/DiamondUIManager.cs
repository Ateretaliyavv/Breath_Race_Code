using TMPro;
using UnityEngine;

public class DiamondUIManager : MonoBehaviour
{
    [System.Serializable]
    public class DiamondPair
    {
        public GameObject greyObject;   // The grey diamond (empty)
        public GameObject purpleObject; // The purple diamond (full/collected)
    }

    [Header("References")]
    [Tooltip("Drag the score text here (optional, only for display)")]
    public TextMeshProUGUI scoreText;

    [Tooltip("List of diamond pairs")]
    public DiamondPair[] diamonds;

    private int lastKnownScore = -1;

    void Start()
    {
        // Use the saved stars from the current run
        int initialScore = DiamondRunKeeper.DimondsCollected;

        // Optional: sync the text with the saved value
        if (scoreText != null)
            scoreText.text = initialScore.ToString();

        lastKnownScore = initialScore;
        UpdateDiamondsVisibility(initialScore);
    }

    void Update()
    {
        // Get current score from the keeper
        int currentScore = DiamondRunKeeper.DimondsCollected;


        if (currentScore != lastKnownScore)
        {
            lastKnownScore = currentScore;
            UpdateDiamondsVisibility(currentScore);

            // Optional: update the text each time
            if (scoreText != null)
                scoreText.text = currentScore.ToString();
        }
    }

    void UpdateDiamondsVisibility(int score)
    {
        for (int i = 0; i < diamonds.Length; i++)
        {
            //If the index is lower than the score, this diamond is collected
            if (i < score)
            {
                // Enable purple, disable grey
                if (diamonds[i].purpleObject) diamonds[i].purpleObject.SetActive(true);
                if (diamonds[i].greyObject) diamonds[i].greyObject.SetActive(false);
            }
            else
            {
                // Enable grey, disable purple (default state)

                if (diamonds[i].purpleObject) diamonds[i].purpleObject.SetActive(false);
                if (diamonds[i].greyObject) diamonds[i].greyObject.SetActive(true);
            }
        }
    }
}
