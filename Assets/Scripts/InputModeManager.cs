using UnityEngine;

/*
 * Global manager for input mode (Keyboard / Breath).
 * Applies the chosen mode to all relevant scripts in the scene.
 */

public class InputModeManager : MonoBehaviour
{
    public enum InputMode
    {
        Keyboard,
        Breath
    }

    [Header("Global Input Mode")]
    [SerializeField] private InputMode currentMode = InputMode.Keyboard;

    [Header("Controlled Scripts")]
    [SerializeField] private BridgeBuilder bridgeBuilder;
    [SerializeField] private PushBox pushBox;
    [SerializeField] private BlowUpBalloons blowUpBalloons;
    [SerializeField] private Jump jump;
    [SerializeField] private InflatingBalloon inflatingBalloon;
    [SerializeField] private BalloonExplod balloonExplod; // currently not input-based, but kept for future use


    private void Start()
    {
        ApplyModeToAll();
    }

    // Call this from UI button (for example) to switch mode at runtime
    public void SetModeKeyboard()
    {
        currentMode = InputMode.Keyboard;
        ApplyModeToAll();
    }

    public void SetModeBreath()
    {
        currentMode = InputMode.Breath;
        ApplyModeToAll();
    }

    private void ApplyModeToAll()
    {
        bool useBreath = (currentMode == InputMode.Breath);

        if (bridgeBuilder != null)
            bridgeBuilder.SetControlMode(useBreath);

        if (pushBox != null)
            pushBox.SetControlMode(useBreath);

        if (blowUpBalloons != null)
            blowUpBalloons.SetControlMode(useBreath);

        if (jump != null)
            jump.SetControlMode(useBreath);

        if (inflatingBalloon != null)
            inflatingBalloon.SetControlMode(useBreath);

        if (balloonExplod != null)
            balloonExplod.SetControlMode(useBreath);

        Debug.Log("InputModeManager: Switched mode to " + currentMode);
    }
}
