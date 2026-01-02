using UnityEngine;

public class FallSound : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The Tag of the invisible floor objects that trigger the fall sound")]
    [SerializeField] private string fallZoneTag = "FallZone"; // התגית שנחפש

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip failSound;

    private bool hasPlayed = false;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // הפונקציה הזו מחליפה את ה-Update והחישובים
    // היא נקראת אוטומטית כשהשחקן נכנס לתוך טריגר כלשהו
    private void OnTriggerEnter2D(Collider2D other)
    {
        // בדיקה: האם נכנסנו לתוך אובייקט שיש לו את התגית "FallZone"?
        // וגם: האם הצליל עוד לא נוגן?
        if (other.CompareTag(fallZoneTag) && !hasPlayed)
        {
            PlayFailSound();
        }
    }

    private void PlayFailSound()
    {
        if (audioSource != null && failSound != null)
        {
            audioSource.PlayOneShot(failSound);
            hasPlayed = true;
            Debug.Log("Player hit the FallZone!");
        }
    }

    // אופציונלי: פונקציה לאיפוס (אם יש Respawn במשחק)
    public void ResetFallSound()
    {
        hasPlayed = false;
    }
}
