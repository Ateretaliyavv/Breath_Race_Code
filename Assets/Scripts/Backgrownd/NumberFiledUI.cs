using TMPro;
using UnityEngine;

/**
 * This component should be attached to a TextMeshPro object.
 * It allows to feed an integer number to the text field.
 */
[RequireComponent(typeof(TextMeshProUGUI))]
public class NumberFieldUI : MonoBehaviour
{
    [SerializeField] private int number;

    public int GetNumberUI()
    {
        return this.number;
    }

    public void SetNumberUI(int newNumber)
    {
        this.number = newNumber;
        GetComponent<TextMeshProUGUI>().text = newNumber.ToString();
        StarsNumberKeeper.StarsCollected = this.number;
    }

    public void AddNumberUI(int toAdd)
    {
        SetNumberUI(this.number + toAdd);
    }
}
