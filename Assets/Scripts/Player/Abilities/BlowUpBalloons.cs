using UnityEngine;
using UnityEngine.InputSystem;

public class BlowUpBalloons : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SimpleMove[] simpleMove;

    [Header("Blow Up Button")]
    [SerializeField] private InputAction blowUpButton = new InputAction(type: InputActionType.Button);

    [Header("Blow Up Zone Tags")]
    [SerializeField] private string blowStartTag = "BlowStart";
    [SerializeField] private string blowEndTag = "BlowEnd";

    [Header("Blow Distance")]
    [SerializeField] private float maxBlowDistanceX = 20f;

    private Transform[] blowStarts;
    private Transform[] blowEnds;

    // True only while key is pressed and we are inside a valid blow zone
    private bool isHeld = false;

    // These are used internally by IsInsideBlowZone
    private float currentZoneStartX = float.NegativeInfinity;
    private float currentZoneEndX = float.PositiveInfinity;

    private void Awake()
    {
        if (simpleMove == null)
        {
            Debug.LogError("BlowUpBalloons: simpleMove array is not assigned on " + gameObject.name);
        }
        else
        {
            // Disable all movement scripts at start
            foreach (SimpleMove s in simpleMove)
            {
                if (s != null)
                    s.enabled = false;
            }
        }

        // Find all BlowStart and BlowEnd markers in the scene
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
        blowUpButton.Enable();
        blowUpButton.performed += OnBlowPressed;
    }

    private void OnDisable()
    {
        blowUpButton.performed -= OnBlowPressed;
        blowUpButton.Disable();
    }

    private void OnBlowPressed(InputAction.CallbackContext ctx)
    {
        // Allow blowing only if inside a valid blow zone when the key is pressed
        if (IsInsideBlowZone())
        {
            isHeld = true;
            Debug.Log("BlowUpBalloons: blow started inside blow zone");
        }
        else
        {
            isHeld = false;
            Debug.Log("BlowUpBalloons: blow started outside blow zone");
        }
    }

    private void Update()
    {
        if (simpleMove == null)
            return;

        // If key is not held, disable all balloons
        if (!isHeld)
        {
            foreach (SimpleMove s in simpleMove)
            {
                if (s != null)
                    s.enabled = false;
            }
            return;
        }

        // Check if player is still inside a valid blow zone
        bool insideZone = IsInsideBlowZone();
        if (!insideZone)
        {
            foreach (SimpleMove s in simpleMove)
            {
                if (s != null)
                    s.enabled = false;
            }
            return;
        }

        // Enable movement only for balloons close enough on the X axis
        float playerX = transform.position.x;

        foreach (SimpleMove s in simpleMove)
        {
            if (s == null)
                continue;

            float balloonX = s.transform.position.x;
            float distanceX = Mathf.Abs(balloonX - playerX);

            bool shouldBlow = distanceX <= maxBlowDistanceX;
            s.enabled = shouldBlow;
        }
    }

    // Returns true if the player is between a BlowStart behind it
    // and the closest BlowEnd in front of it
    private bool IsInsideBlowZone()
    {
        if (blowStarts == null || blowStarts.Length == 0 ||
            blowEnds == null || blowEnds.Length == 0)
            return false;

        float playerX = transform.position.x;

        // Find the last BlowStart behind or at the player
        float lastStartX = float.NegativeInfinity;
        foreach (Transform s in blowStarts)
        {
            if (s == null) continue;
            if (s.position.x <= playerX && s.position.x > lastStartX)
                lastStartX = s.position.x;
        }

        if (lastStartX == float.NegativeInfinity)
        {
            currentZoneStartX = float.NegativeInfinity;
            currentZoneEndX = float.PositiveInfinity;
            return false;
        }

        // Find the closest BlowEnd ahead of or at the player
        float nextEndX = float.PositiveInfinity;
        foreach (Transform e in blowEnds)
        {
            if (e == null) continue;
            if (e.position.x >= playerX && e.position.x < nextEndX)
                nextEndX = e.position.x;
        }

        if (nextEndX == float.PositiveInfinity)
        {
            currentZoneStartX = float.NegativeInfinity;
            currentZoneEndX = float.PositiveInfinity;
            return false;
        }

        currentZoneStartX = lastStartX;
        currentZoneEndX = nextEndX;

        return playerX >= lastStartX && playerX <= nextEndX;
    }
}
