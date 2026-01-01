using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DiamondCollectible : MonoBehaviour, ICollectible
{
    [Header("UI")]
    [SerializeField]
    [Tooltip("Cheese counter UI text")]
    private TextMeshProUGUI starsCounterText;

    [Header("Cover Settings")]
    [SerializeField] private string coverObjectTag;

    // --- Audio Settings ---
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound;

    public void Collect()
    {
        Vector3 diamondPos = transform.position;

        if (HasCoverOnTop(diamondPos))
            return;

        NumberFieldUI uiCounter = starsCounterText.GetComponent<NumberFieldUI>();

        if (uiCounter != null)
        {
            uiCounter.AddNumberUI(1);
            DiamondRunKeeper.DimondsCollected = uiCounter.GetNumberUI();
        }

        DiamondPersistent persistent = GetComponent<DiamondPersistent>();
        if (persistent != null)
        {
            persistent.MarkCollected();
        }

        // --- Play Sound ---
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        StartCoroutine(PlayCollectAnimationAndDestroy());
    }

    private IEnumerator PlayCollectAnimationAndDestroy()
    {
        Animator anim = GetComponent<Animator>();
        float animDuration = 0f;

        // Calculate animation duration
        if (anim != null)
        {
            anim.SetTrigger("Collect");
            AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name.Contains("feedback") || clip.name.Contains("feed"))
                {
                    animDuration = clip.length;
                    break;
                }
            }
        }

        // --- Calculate total wait time ---
        // We want to wait for either the animation or the sound to finish (whichever is longer)
        // to prevent the object from being destroyed while the sound is playing
        float soundDuration = (collectSound != null) ? collectSound.length : 0f;

        // Take the longer duration
        float waitTime = Mathf.Max(animDuration, soundDuration);

        // Wait
        yield return new WaitForSeconds(waitTime);

        Destroy(gameObject);
    }

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

    public static bool AreSameXY(Vector3 a, Vector3 b, float epsilon = 0.35f)
    {
        bool sameX = Mathf.Abs(a.x - b.x) < epsilon;
        bool sameY = Mathf.Abs(a.y - b.y) < epsilon;
        return sameX && sameY;
    }
}
