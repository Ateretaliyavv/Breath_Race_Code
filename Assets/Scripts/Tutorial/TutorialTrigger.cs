using UnityEngine;

/*
 * Attached to trigger zones in the game world.
 * When the player enters the trigger, it calls the TutorialManager to display instructions.
 * Configurable via Inspector for different tutorial types and messages.
 */

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [TextArea]
    public string instructions; // The text to display in the UI
    public TutorialType tutorialType; // Jump or Bridge

    [Header("Manager Reference")]
    [SerializeField] private TutorialManager manager;

    // Prevents the tutorial from triggering multiple times
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If already triggered, do nothing
        if (hasTriggered) return;

        // Check if the object entering is the Player
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (manager != null)
            {
                manager.TriggerTutorial(instructions, tutorialType);
            }
            else
            {
                Debug.LogError("TutorialTrigger: Manager reference is missing!");
            }
        }
    }
}
