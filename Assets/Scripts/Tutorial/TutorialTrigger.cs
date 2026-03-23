using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public string[] instructionsKeys;
    public TutorialType tutorialType;
    public Sprite tutorialImage;

    [Header("Manager Reference")]
    [SerializeField] private TutorialManager manager;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            if (manager != null)
            {
                hasTriggered = true;

                string[] translatedMessages = new string[instructionsKeys.Length];

                for (int i = 0; i < instructionsKeys.Length; i++)
                {
                    translatedMessages[i] = LocalizationManager.I.Tr(instructionsKeys[i]);
                }

                manager.TriggerTutorial(translatedMessages, tutorialType, tutorialImage);
            }
            else
            {
                Debug.LogError("TutorialTrigger: Manager reference is missing!");
            }
        }
    }
}
