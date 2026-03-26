using UnityEngine;

public class BreathSettingsPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;

    public void OpenPanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void ClosePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void TogglePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(!panelRoot.activeSelf);
    }
}
