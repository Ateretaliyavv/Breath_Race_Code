using TMPro;
using UnityEngine;

// Updates a TMP text component according to the current selected language.
[RequireComponent(typeof(TMP_Text))]
public class LocalizedTMP : MonoBehaviour
{
    // Translation key used to fetch the localized text.
    public string key;

    // Cached reference to the TMP text component.
    private TMP_Text tmp;

    // Gets the TMP_Text component on this object.
    private void Awake() => tmp = GetComponent<TMP_Text>();

    // Subscribes to language changes and refreshes the text.
    private void OnEnable()
    {
        if (LocalizationManager.I != null)
            LocalizationManager.I.OnLanguageChanged += Refresh;

        Refresh();
    }

    // Unsubscribes from language changes when disabled.
    private void OnDisable()
    {
        if (LocalizationManager.I != null)
            LocalizationManager.I.OnLanguageChanged -= Refresh;
    }

    // Sets a new translation key at runtime and refreshes the text immediately.
    public void SetKey(string newKey)
    {
        key = newKey;
        Refresh();
    }

    // Applies the translated text and updates text direction and alignment.
    public void Refresh()
    {
        if (LocalizationManager.I == null) return;

        var translated = LocalizationManager.I.Tr(key);
        bool isHebrew = LocalizationManager.I.CurrentLang == Lang.HE;

        // Fix Hebrew text for RTL display, otherwise use the text as is.
        tmp.text = isHebrew ? RtlTextHelper.FixForceRTL(translated, true, true) : translated;
        tmp.isRightToLeftText = isHebrew;

        // Update alignment only for left/right based alignments.
        if (tmp.alignment == TextAlignmentOptions.Left ||
            tmp.alignment == TextAlignmentOptions.TopLeft ||
            tmp.alignment == TextAlignmentOptions.BottomLeft ||
            tmp.alignment == TextAlignmentOptions.Right ||
            tmp.alignment == TextAlignmentOptions.TopRight ||
            tmp.alignment == TextAlignmentOptions.BottomRight)
        {
            tmp.alignment = isHebrew ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
        }
    }
}
