using UnityEngine;
using UnityEngine.UI;

/*
 * Intro panel that blocks gameplay input until Start is pressed.
 * Disables all SimpleBlow components while the panel is visible.
 */
public class IntroPanelBalloons : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Buttons")]
    [SerializeField] private Button startButton;

    private SimpleBlow[] blowScripts;

    // Cache references and wire button
    private void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        // Find also inactive objects if needed
        blowScripts = FindObjectsOfType<SimpleBlow>(includeInactive: true);
    }

    // When panel becomes visible, lock gameplay
    private void OnEnable()
    {
        LockGameplay();
    }

    // Disable all blow scripts so no keyboard/breath input works
    private void LockGameplay()
    {
        if (blowScripts == null || blowScripts.Length == 0)
            blowScripts = FindObjectsOfType<SimpleBlow>(includeInactive: true);

        foreach (var s in blowScripts)
        {
            if (s != null) s.enabled = false;
        }
    }

    // Enable gameplay after Start
    private void UnlockGameplay()
    {
        if (blowScripts == null || blowScripts.Length == 0)
            blowScripts = FindObjectsOfType<SimpleBlow>(includeInactive: true);

        foreach (var s in blowScripts)
        {
            if (s != null) s.enabled = true;
        }
    }

    // Start button handler: hide panel and enable gameplay
    private void OnStartClicked()
    {
        UnlockGameplay();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}
