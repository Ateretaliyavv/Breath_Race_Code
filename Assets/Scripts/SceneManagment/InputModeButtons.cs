using UnityEngine;

public class InputModeButtons : MonoBehaviour
{
    public void ChooseKeyboard()
    {
        GlobalInputModeManager.Instance.SetKeyboard();
    }

    public void ChooseBreath()
    {
        GlobalInputModeManager.Instance.SetBreath();
    }
}
