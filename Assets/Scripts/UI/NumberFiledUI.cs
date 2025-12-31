using TMPro;
using UnityEngine;

/*
 * This component should be attached to a TextMeshPro object.
 * It allows to feed an integer number to the text field.
 */
[RequireComponent(typeof(TextMeshProUGUI))]
public class NumberFieldUI : MonoBehaviour
{
    [SerializeField] private int number;
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        // Initialize number from the global keeper (so respawn from checkpoint keeps the value)
        number = DiamondRunKeeper.DimondsCollected;

        if (text != null)
            text.text = number.ToString();
    }

    public int GetNumberUI()
    {
        return this.number;
    }

    public void SetNumberUI(int newNumber)
    {
        this.number = newNumber;

        if (text != null)
            text.text = newNumber.ToString();

        // Keep the global value in sync
        DiamondRunKeeper.DimondsCollected = this.number;
    }

    public void AddNumberUI(int toAdd)
    {
        SetNumberUI(this.number + toAdd);
    }
}
