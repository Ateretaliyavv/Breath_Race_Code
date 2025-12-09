using UnityEngine;
using UnityEngine.SceneManagement;
/* 
 * This script allows a GameObject to load a specified scene when a method is called.
*/

public class GoToSceneByClick : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    public void LoadGameScene()
    {
        // For menu buttons we usually do NOT mark "next level"
        SceneNavigator.LoadScene(sceneToLoad, markAsNextLevel: false);
    }
}
