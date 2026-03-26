using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Global input mode keeper that persists across scenes.
 * Applies the selected mode to all relevant scripts in every loaded scene.
 */
public class GlobalInputModeManager : MonoBehaviour
{
    public static GlobalInputModeManager Instance { get; private set; }

    public enum InputMode
    {
        Keyboard,
        Breath
    }

    [SerializeField] private InputMode currentMode = InputMode.Keyboard;

    public InputMode CurrentMode => currentMode;
    public bool UseBreath => currentMode == InputMode.Breath;

    // Notifies UI and other listeners whenever the mode changes.
    public event Action<bool> OnModeChanged;

    private void Awake()
    {
        // Keep only one global instance alive across scenes.
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
        OnModeChanged?.Invoke(UseBreath);
    }

    public void SetBreath()
    {
        currentMode = InputMode.Breath;

        ApplyModeToScene();
        OnModeChanged?.Invoke(UseBreath);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-apply the current mode after every scene load.
        ApplyModeToScene();
        OnModeChanged?.Invoke(UseBreath);
    }

    private void ApplyModeToScene()
    {
        bool useBreath = UseBreath;

        // Apply the current mode to any component that exposes SetControlMode(bool).
        foreach (var x in FindObjectsOfType<MonoBehaviour>(true))
        {
            var m = x.GetType().GetMethod("SetControlMode");
            if (m != null &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(bool))
            {
                m.Invoke(x, new object[] { useBreath });
            }
        }

        Debug.Log("GlobalInputModeManager: Mode = " + currentMode);
    }
}
