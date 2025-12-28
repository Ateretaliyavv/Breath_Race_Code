using UnityEngine;
using UnityEngine.SceneManagement; // חובה כדי לטעון שלבים

public class LevelSelector : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject selectionWindow; // גרירת הפאנל שיצרנו לכאן

    // פונקציה זו תופעל ע"י הכפתור הראשי שפותח את הבחירה
    public void OpenSelectionMenu()
    {
        selectionWindow.SetActive(true); // מציג את החלונית
    }

    // פונקציה זו תופעל ע"י כפתור הביטול (אם יש)
    public void CloseSelectionMenu()
    {
        selectionWindow.SetActive(false); // מסתיר את החלונית
    }

    // פונקציה זו תופעל ע"י כפתורי הבחירה
    // string levelName = השם המדויק של הסצנה ביוניטי
    public void LoadLevelByType(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
