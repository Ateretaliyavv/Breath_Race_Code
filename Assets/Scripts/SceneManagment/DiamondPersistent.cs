using UnityEngine;
using UnityEngine.SceneManagement;

public class DiamondPersistent : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Unique id for this diamond inside the level")]
    private string diamondId;

    private void Awake()
    {
        // If no id set in Inspector, use the GameObject name
        if (string.IsNullOrEmpty(diamondId))
            diamondId = gameObject.name;

        string key = BuildKey();

        // If this diamond was already collected in this run - hide it
        if (DiamondRunKeeper.IsCollected(key))
        {
            gameObject.SetActive(false);
        }
    }

    private string BuildKey()
    {
        string levelName = SceneManager.GetActiveScene().name;
        return levelName + "|" + diamondId;
    }

    public void MarkCollected()
    {
        DiamondRunKeeper.MarkCollected(BuildKey());
    }
}

