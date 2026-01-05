using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Global input mode keeper that persists across scenes.
 * Applies the selected mode to all relevant scripts in every loaded scene.
 */
public class GlobalInputModeManager : MonoBehaviour
{
    public static GlobalInputModeManager Instance { get; private set; }

    public enum InputMode { Keyboard, Breath }

    [SerializeField] private InputMode currentMode = InputMode.Keyboard;

    public InputMode CurrentMode => currentMode;
    public bool UseBreath => currentMode == InputMode.Breath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SetKeyboard()
    {
        currentMode = InputMode.Keyboard;
        ApplyModeToScene();
    }

    public void SetBreath()
    {
        currentMode = InputMode.Breath;
        ApplyModeToScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyModeToScene();
    }

    private void ApplyModeToScene()
    {
        bool useBreath = UseBreath;

        // Find every relevant controller in the active scene and apply.
        foreach (var x in FindObjectsOfType<MonoBehaviour>(true))
        {
            // Call SetControlMode(bool) if the component has it.
            var m = x.GetType().GetMethod("SetControlMode");
            if (m != null && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(bool))
                m.Invoke(x, new object[] { useBreath });
        }

        Debug.Log("GlobalInputModeManager: Mode = " + currentMode);
    }
}
