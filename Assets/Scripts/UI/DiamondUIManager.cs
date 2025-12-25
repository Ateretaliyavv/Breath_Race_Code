using TMPro;          // Required for reading the text
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
    [Tooltip("Drag the score text here")]
    public TextMeshProUGUI scoreText;

    [Tooltip("List of diamond pairs")]
    public DiamondPair[] diamonds;

    private int lastKnownScore = -1;

    void Start()
    {
        // Initial setup - reset the display
        UpdateDiamondsVisibility(0);
    }

    void Update()
    {
        if (scoreText == null) return;

        int currentScore = 0;
        // Read the score from the text
        if (int.TryParse(scoreText.text, out currentScore))
        {
            // Update only if the number has changed
            if (currentScore != lastKnownScore)
            {
                lastKnownScore = currentScore;
                UpdateDiamondsVisibility(currentScore);
            }
        }
    }

    void UpdateDiamondsVisibility(int score)
    {
        for (int i = 0; i < diamonds.Length; i++)
        {
            // If the index is lower than the score, this diamond is collected
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
