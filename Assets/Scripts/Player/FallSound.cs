using UnityEngine;

public class FallSound : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How many units below the start position to trigger the sound")]
    [SerializeField] private float fallThreshold = 2.0f; // The drop distance required to trigger the sound

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip failSound; // The sound clip for failure/falling

    private float startY;
    private bool hasPlayed = false; // Ensures the sound plays only once

    private void Start()
    {
        // Record the player's starting Y position
        startY = transform.position.y;

        // Automatically find AudioSource if not assigned in Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // If sound has already played, stop checking
        if (hasPlayed) return;

        // Check if current Y is below (Start Y - Threshold)
        // Example: Start at 0, threshold is 2. If Y drops below -2, sound plays.
        if (transform.position.y < (startY - fallThreshold))
        {
            PlayFailSound();
        }
    }

    private void PlayFailSound()
    {
        if (audioSource != null && failSound != null)
        {
            audioSource.PlayOneShot(failSound);
            hasPlayed = true; // Lock the flag to prevent looping
            Debug.Log("Player fell! Playing sound.");
        }
    }
}
