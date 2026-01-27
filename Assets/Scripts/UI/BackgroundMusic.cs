using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;

    void Awake()
    {
        // Check if an instance already exists
        if (instance == null)
        {
            instance = this;
            // make this object persistent across scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // if an instance already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }
}
