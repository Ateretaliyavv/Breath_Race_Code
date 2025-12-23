using UnityEngine;

/* 
 * This script allows a GameObject (usually a UI Button) to load a scene when a method is called.
 * It can load a fixed scene, or a random scene from a list (selectable in the Inspector).
 */

public class GoToSceneByClick : MonoBehaviour
{
    [Header("Scene Loading Mode")]
    [Tooltip("If true, loads a random scene from the list. If false, loads the fixed sceneToLoad.")]
    [SerializeField] private bool useRandomScene = false;

    [Header("Fixed Scene")]
    [SerializeField] private string sceneToLoad;

    [Header("Random Scene List")]
    [Tooltip("Possible scenes to load when useRandomScene is enabled.")]
    [SerializeField] private string[] randomSceneNames;

    public void LoadGameScene()
    {
        // Decide which scene to load based on the selected mode
        string selectedScene = GetSceneToLoad();
        if (string.IsNullOrEmpty(selectedScene))
            return;

        // For menu buttons we usually do NOT mark "next level"
        SceneNavigator.LoadScene(selectedScene, markAsNextLevel: false);
    }

    private string GetSceneToLoad()
    {
        // Fixed scene mode
        if (!useRandomScene)
            return sceneToLoad;

        // Random scene mode
        if (randomSceneNames == null || randomSceneNames.Length == 0)
            return null;

        int idx = Random.Range(0, randomSceneNames.Length);
        return randomSceneNames[idx];
    }
}
