using System.Collections;
using TMPro;
using UnityEngine;

/*
 * Collects objects with a specified tag when the player collides with them,
 * EXCEPT when there is a cover object exactly at the same position.
 */
public class CollectObjects : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Cheese counter UI text")]
    TextMeshProUGUI StarsCounterText;

    [SerializeField] string triggeringTag;   // Tag of the collectible (diamond)
    [SerializeField] string coverObjectTag;  // Tag of the cover (balloon)

    private void OnTriggerEnter2D(Collider2D other)
    {
        // React only to the collectible objects
        if (!other.CompareTag(triggeringTag))
            return;

        // Position of THIS specific diamond we just touched
        Vector3 diamondPos = other.transform.position;

        // If this diamond has a cover on it - do NOT collect
        if (!HasCoverOnTop(diamondPos))
        {
            // Get the NumberFieldUI component on the UI text
            NumberFieldUI uiCounter = StarsCounterText.GetComponent<NumberFieldUI>();

            if (uiCounter != null)
            {
                // Increase counter in the UI
                uiCounter.AddNumberUI(1);

                // Update the global keeper based on the UI counter current value
                StarsNumberKeeper.StarsCollected = uiCounter.GetNumberUI();
            }

            // Mark this diamond as collected for this run
            DiamondPersistent persistent = other.GetComponent<DiamondPersistent>();
            if (persistent != null)
            {
                persistent.MarkCollected();
            }

            // Play collect animation and then remove the diamond
            StartCoroutine(PlayCollectAnimationAndDestroy(other));
        }
    }

    // Coroutine: plays collect animation then destroys the diamond when animation ends
    private IEnumerator PlayCollectAnimationAndDestroy(Collider2D diamond)
    {
        Animator anim = diamond.GetComponent<Animator>();
        float duration = 0f;

        if (anim != null)
        {
            // Activate the "Collect" animation using trigger
            anim.SetTrigger("Collect");

            // Get the real animation duration dynamically
            AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                // Searching for feedback animation clip by name
                if (clip.name.Contains("feedback") || clip.name.Contains("feed"))
                {
                    duration = clip.length;
                    break;
                }
            }
        }

        // Wait until the animation clip actually finishes
        yield return new WaitForSeconds(duration);

        // Remove the collected diamond from the scene
        Destroy(diamond.gameObject);
    }

    // Checks if there is ANY cover object at the same XY as the given diamond position
    private bool HasCoverOnTop(Vector3 diamondPos)
    {
        GameObject[] covers = GameObject.FindGameObjectsWithTag(coverObjectTag);

        foreach (GameObject cover in covers)
        {
            if (cover == null) continue;

            Vector3 coverPos = cover.transform.position;

            if (AreSameXY(coverPos, diamondPos))
            {
                return true;
            }
        }
        return false;
    }

    // Returns true if two positions share the same X and Y (within a small epsilon)
    public static bool AreSameXY(Vector3 a, Vector3 b, float epsilon = 0.35f)
    {
        bool sameX = Mathf.Abs(a.x - b.x) < epsilon;
        bool sameY = Mathf.Abs(a.y - b.y) < epsilon;

        return sameX && sameY;
    }
}
