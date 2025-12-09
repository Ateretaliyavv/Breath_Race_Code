using TMPro;
using UnityEngine;

/*
 * Collects objects with a specified tag when the player collides with them.
 */

public class CollectObjects : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Cheese counter UI text")]
    TextMeshProUGUI StarsCounterText;

    [SerializeField] string triggeringTag;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(triggeringTag))
            return;

        // Get the NumberFieldUI component on the UI text
        NumberFieldUI uiCounter = StarsCounterText.GetComponent<NumberFieldUI>();

        if (uiCounter != null)
        {
            uiCounter.AddNumberUI(1);
        }

        // Remove the collected star from the scene
        Destroy(other.gameObject);
    }
}
