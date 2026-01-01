using System.Collections;
using UnityEngine;

/*
 * Script that handles the balloon explosion via trigger collision.
 * - Plays "Explode" sound.
 * - Triggers Animator.
 * - Transitions to surprise scene.
 */
[RequireComponent(typeof(Animator))]
public class BalloonExplod : MonoBehaviour
{
    [Header("Explosion Trigger Settings")]
    [Tooltip("The specific object that causes the balloon to explode (e.g., Needle/Spike)")]
    [SerializeField] private GameObject explodingObject;

    [Header("Explosion Visuals")]
    [Tooltip("Scale multiplier applied instantly when exploding")]
    [SerializeField] private float explosionScaleMultiplier = 1.5f;

    [Header("Scene Transition")]
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float sceneLoadDelay = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explodeSound; // Sound for "BOOM"

    private Animator animator;
    private bool hasExploded = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("IsExploded", false);

        // Auto-find AudioSource if missing
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        // Check if the collided object is the designated "Exploder"
        if (other.gameObject == explodingObject)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        // 1. Play Explosion Sound
        if (audioSource != null && explodeSound != null)
        {
            audioSource.pitch = 1f; // Reset pitch to normal for explosion
            audioSource.PlayOneShot(explodeSound);
        }

        // 2. Visual Effects
        transform.localScale *= explosionScaleMultiplier;
        animator.SetBool("IsExploded", true);

        // 3. Start Scene Transition
        StartCoroutine(SceneTransitionAfterDelay());
    }

    private IEnumerator SceneTransitionAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        // Ensure SceneNavigator exists in your project, otherwise use SceneManager
        SceneNavigator.LoadScene(sceneToLoad, markAsNextLevel: false);
    }

    // For compatibility with InputModeManager.
    // Currently explosion logic is not input-based, so this does nothing.
    public void SetControlMode(bool useBreath)
    {
        // No-op for now
    }
}
