using UnityEngine;

/*
 * Loads a scene from a UI Button.
 * Supports: fixed scene, random scene, or scene-by-input-mode (Keyboard/Breath).
 * Optional: override a specific chosen scene into mode-variant scenes.
 */
public class GoToSceneByClick : MonoBehaviour
{
    [Header("Scene Loading Mode")]
    [Tooltip("If true, chooses a scene by the global input mode (Keyboard/Breath).")]
    [SerializeField] private bool useInputModeScene = false;

    [Tooltip("If true, loads a random scene from the list. If false, uses fixed sceneToLoad.")]
    [SerializeField] private bool useRandomScene = false;

    [Header("Fixed Scene")]
    [SerializeField] private string sceneToLoad;

    [Header("Random Scene List")]
    [Tooltip("Possible scenes to load when useRandomScene is enabled.")]
    [SerializeField] private string[] randomSceneNames;

    [Header("Input Mode Scenes")]
    [Tooltip("Scene to load when the global mode is Keyboard.")]
    [SerializeField] private string keyboardSceneToLoad;

    [Tooltip("Scene to load when the global mode is Breath.")]
    [SerializeField] private string breathSceneToLoad;

    [Header("Optional Overrides By Mode")]
    [Tooltip("If chosen scene equals this name, replace it by mode variant.")]
    [SerializeField] private string baseSceneName; // e.g., "Balloons"

    [Tooltip("Replacement when global mode is Keyboard.")]
    [SerializeField] private string keyboardVariantSceneName; // e.g., "BalloonsKeyboard"

    [Tooltip("Replacement when global mode is Breath.")]
    [SerializeField] private string breathVariantSceneName; // e.g., "Balloons" (can be empty)

    [Header("Run Reset Options")]
    [Tooltip("If true, stars and collected diamonds will be reset before loading the scene.")]
    [SerializeField] private bool resetRunData = false;

    public void LoadGameScene()
    {
        // Decide which scene to load based on the selected mode
        string selectedScene = GetSceneToLoad();
        if (string.IsNullOrEmpty(selectedScene))
            return;

        // Optional reset for diamonds (used for Next Level, Home, Back buttons)
        if (resetRunData)
        {
            DiamondRunKeeper.ClearAll();
        }

        // For menu buttons we usually do NOT mark "next level"
        SceneNavigator.LoadScene(selectedScene, markAsNextLevel: false);
    }

    private string GetSceneToLoad()
    {
        string chosen;

        // Mode 1: choose scene by global input mode
        if (useInputModeScene)
        {
            bool useBreath = false;

            if (GlobalInputModeManager.Instance != null)
                useBreath = GlobalInputModeManager.Instance.UseBreath;

            chosen = useBreath ? breathSceneToLoad : keyboardSceneToLoad;
            return ApplyOverrideByMode(chosen);
        }

        // Mode 2: fixed scene
        if (!useRandomScene)
        {
            chosen = sceneToLoad;
            return ApplyOverrideByMode(chosen);
        }

        // Mode 3: random scene
        if (randomSceneNames == null || randomSceneNames.Length == 0)
            return null;

        int idx = Random.Range(0, randomSceneNames.Length);
        chosen = randomSceneNames[idx];

        return ApplyOverrideByMode(chosen);
    }

    // Applies an optional replacement for one specific chosen scene based on input mode
    private string ApplyOverrideByMode(string chosen)
    {
        if (string.IsNullOrEmpty(chosen))
            return chosen;

        if (string.IsNullOrEmpty(baseSceneName))
            return chosen;

        if (chosen != baseSceneName)
            return chosen;

        bool useBreath = GlobalInputModeManager.Instance != null && GlobalInputModeManager.Instance.UseBreath;

        if (useBreath)
            return string.IsNullOrEmpty(breathVariantSceneName) ? chosen : breathVariantSceneName;

        return string.IsNullOrEmpty(keyboardVariantSceneName) ? chosen : keyboardVariantSceneName;
    }
}
