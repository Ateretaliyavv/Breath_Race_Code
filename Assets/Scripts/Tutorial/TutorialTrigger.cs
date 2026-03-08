using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [TextArea]
    public string instructionsKey;
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
            hasTriggered = true;

            if (manager != null)
            {
                string text = LocalizationManager.I.Tr(instructionsKey);
                manager.TriggerTutorial(text, tutorialType, tutorialImage);
            }
            else
            {
                Debug.LogError("TutorialTrigger: Manager reference is missing!");
            }
        }
    }
}
