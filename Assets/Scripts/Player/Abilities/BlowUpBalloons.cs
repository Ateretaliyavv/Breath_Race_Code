using UnityEngine;
using UnityEngine.InputSystem;

public class BlowUpBalloons : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SimpleMove[] simpleMove;    // Movement script to control

    [Header("Blow Up Button")]
    [SerializeField] InputAction BlowUpButton = new InputAction(type: InputActionType.Button);

    [Header("Blow Up Zone Tags")]
    [SerializeField] private string blowStartTag = "BlowStart";
    [SerializeField] private string blowEndTag = "BlowEnd";

    // Arrays of all BlowStart / BlowEnd markers in the scene
    private Transform[] blowStarts;
    private Transform[] blowEnds;

    // True only while key is held AND we are inside a valid blow zone
    private bool isHeld = false;

    private void Awake()
    {
        // If no reference was set in the Inspector, try to find SimpleMove on this GameObject
        if (simpleMove == null)
        {
            Debug.LogError("BlowUpBalloons: No SimpleMove found on " + gameObject.name);
        }
        else
        {
            // At start, we disable movement until the conditions are met
            foreach (SimpleMove s in simpleMove)
            {
                s.enabled = false;
            }
        }

        // Find all BlowStart and BlowEnd objects by tag
        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(blowStartTag);
        GameObject[] endObjs = GameObject.FindGameObjectsWithTag(blowEndTag);

        blowStarts = new Transform[startObjs.Length];
        blowEnds = new Transform[endObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            blowStarts[i] = startObjs[i].transform;

        for (int i = 0; i < endObjs.Length; i++)
            blowEnds[i] = endObjs[i].transform;

        if (blowStarts.Length == 0)
            Debug.LogWarning("BlowUpBalloons: No objects found with tag " + blowStartTag);

        if (blowEnds.Length == 0)
            Debug.LogWarning("BlowUpBalloons: No objects found with tag " + blowEndTag);
    }

    private void OnEnable()
    {
        BlowUpButton.Enable();
        BlowUpButton.performed += OnBlowPressed;
    }

    private void OnDisable()
    {
        BlowUpButton.performed -= OnBlowPressed;
        BlowUpButton.Disable();

    }

    private void OnBlowPressed(InputAction.CallbackContext ctx)
    {
        // Allow movement only if inside a blow zone
        if (IsInsideBlowZone())
        {
            isHeld = true;
            Debug.Log("BlowUpBalloons: blow started inside blow zone");
        }
        else
        {
            isHeld = false;
            Debug.Log("BlowUpBalloons: NOT inside blow zone - ignored");
        }
    }

    private void Update()
    {
        if (simpleMove == null)
            return;

        // Movement is allowed only when key is held AND we are still inside the zone
        bool allowMovement = isHeld && IsInsideBlowZone();

        foreach (SimpleMove s in simpleMove)
        {
            s.enabled = allowMovement;
        }
    }

    // Returns true if the object is between a BlowStart behind it
    // and the closest BlowEnd in front of it (same logic as in Jump).
    private bool IsInsideBlowZone()
    {
        if (blowStarts == null || blowStarts.Length == 0 ||
            blowEnds == null || blowEnds.Length == 0)
            return false;

        float x = transform.position.x;

        // Find the last BlowStart behind (<= x)
        float lastStartX = float.NegativeInfinity;
        foreach (Transform s in blowStarts)
        {
            if (s == null) continue;
            if (s.position.x <= x && s.position.x > lastStartX)
                lastStartX = s.position.x;
        }

        // If we didn't pass any BlowStart yet – not allowed
        if (lastStartX == float.NegativeInfinity)
            return false;

        // Find the closest BlowEnd ahead or at x
        float nextEndX = float.PositiveInfinity;
        foreach (Transform e in blowEnds)
        {
            if (e == null) continue;
            if (e.position.x >= x && e.position.x < nextEndX)
                nextEndX = e.position.x;
        }

        // If there's no BlowEnd ahead – no defined zone
        if (nextEndX == float.PositiveInfinity)
            return false;

        // We are between BlowStart and BlowEnd
        return x >= lastStartX && x <= nextEndX;
    }
}
