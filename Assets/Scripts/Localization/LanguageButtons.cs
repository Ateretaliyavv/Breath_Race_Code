using UnityEngine;

public class LanguageButtons : MonoBehaviour
{
    public void SetHebrew()
    {
        LocalizationManager.I.SetLanguage(Lang.HE);
    }

    public void SetEnglish()
    {
        LocalizationManager.I.SetLanguage(Lang.EN);
    }
}
