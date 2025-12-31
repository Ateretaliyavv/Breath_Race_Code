using UnityEngine;

/*
 * Generic collector on the player.
 * Detects trigger with collectibles by tag and calls their Collect method.
 */
public class Collector : MonoBehaviour
{
    [Header("Tag of coiiected object")]
    [SerializeField] private string triggeringTag;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // React only to objects with the correct tag
        if (!other.CompareTag(triggeringTag))
            return;

        // Ask the other object if it is collectible
        ICollectible collectible = other.GetComponent<ICollectible>();
        if (collectible != null)
        {
            collectible.Collect();
        }
    }
}
