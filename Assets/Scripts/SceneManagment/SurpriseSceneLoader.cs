using UnityEngine;

public class SurpriseSceneLoader : MonoBehaviour
{
    [Header("Target To Watch")]
    [SerializeField] private GameObject targetObject;   // Object we monitor moving on X

    [Header("Scene To Load")]
    [SerializeField] private string sceneName;          // Scene to load when X is passed
    [SerializeField] private bool markAsNextLevel = false;

    private bool hasTriggered = false;

    private void Update()
    {
        if (hasTriggered)
            return;

        if (targetObject == null)
            return;

        // Check if target passed the X position of this object
        if (targetObject.transform.position.x >= transform.position.x)
        {
            hasTriggered = true;
            SceneNavigator.LoadScene(sceneName, markAsNextLevel);
        }
    }
}
