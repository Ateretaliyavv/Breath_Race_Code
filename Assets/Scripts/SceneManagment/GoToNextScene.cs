using UnityEngine;
/*
 * Change scene when the player collides with the trigger.
 */

public class GotoNextScene : MonoBehaviour
{
    [SerializeField] string triggeringTag;
    [SerializeField] string sceneName;

    public static bool isNextLevel = false;   // global flag
    bool isLoading = false;                   // Local anti-duplication lock

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;
        if (isLoading) return;
        if (!other.CompareTag(triggeringTag)) return;

        isLoading = true;
        isNextLevel = true;   // Informing the entire game that the stage is over

        // Use central navigator mark as "next level"
        SceneNavigator.LoadScene(sceneName, markAsNextLevel: isNextLevel);
    }
}
