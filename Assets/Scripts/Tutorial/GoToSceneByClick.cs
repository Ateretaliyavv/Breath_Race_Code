using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToSceneByClick : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    public void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("No scene name assigned in the Inspector!");
        }
    }
}
