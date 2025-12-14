using UnityEngine;
using UnityEngine.InputSystem;

public class BlowUpBalloons : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SimpleMove[] simpleMove;

    [Header("Blow Up Button")]
    [SerializeField] private InputAction blowUpButton = new InputAction(type: InputActionType.Button);

    [Header("Blow Zone Tags")]
    [SerializeField] private string blowStartTag = "BlowStart";

    [Header("Blow Distance")]
    [SerializeField] private float maxBlowDistanceX = 20f;

    // All BlowStart objects in the scene
    private Transform[] blowStarts;

    // For each balloon: should it blow in the current zone after a valid press
    private bool[] balloonShouldBlow;

    // True if the player has pressed the blow button at least once
    // while inside a valid blow zone and with at least one balloon in front
    private bool blowTriggered = false;

    private void Awake()
    {
        if (simpleMove != null && simpleMove.Length > 0)
        {
            balloonShouldBlow = new bool[simpleMove.Length];

            foreach (SimpleMove s in simpleMove)
            {
                if (s != null)
                    s.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning("BlowUpBalloons: simpleMove array is empty or null on " + gameObject.name);
        }

        // Find all BlowStart markers in the scene
        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(blowStartTag);
        blowStarts = new Transform[startObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            blowStarts[i] = startObjs[i].transform;

        if (blowStarts.Length == 0)
            Debug.LogWarning("BlowUpBalloons: No BlowStart objects found.");
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

        blowTriggered = false;
        ResetBalloons();
    }

    private void OnBlowPressed(InputAction.CallbackContext ctx)
    {
        // A single press is only valid if:
        // 1) The player is inside a blow zone
        // 2) There is at least one balloon in front of the player within maxBlowDistanceX
        if (!IsInsideBlowZone())
        {
            Debug.Log("BlowUpBalloons: blow pressed outside blow zone (ignored)");
            return;
        }

        if (simpleMove == null || simpleMove.Length == 0)
            return;

        float playerX = transform.position.x;
        bool anyBalloonInFront = false;

        // Reset previous selection
        for (int i = 0; i < balloonShouldBlow.Length; i++)
            balloonShouldBlow[i] = false;

        // Check only at press time which balloons are in front and within distance
        for (int i = 0; i < simpleMove.Length; i++)
        {
            SimpleMove s = simpleMove[i];
            if (s == null)
                continue;

            float balloonX = s.transform.position.x;
            float distanceX = balloonX - playerX;

            if (distanceX > 0 && distanceX <= maxBlowDistanceX)
            {
                balloonShouldBlow[i] = true;
                anyBalloonInFront = true;
            }
        }

        if (anyBalloonInFront)
        {
            blowTriggered = true;
            Debug.Log("BlowUpBalloons: blow triggered with balloons in front");
        }
        else
        {
            blowTriggered = false;
            Debug.Log("BlowUpBalloons: no balloons in front within distance, blow ignored");
        }
    }

    private void Update()
    {
        if (simpleMove == null || simpleMove.Length == 0)
            return;

        bool insideZone = IsInsideBlowZone();

        // If player leaves the zone, stop blowing and reset trigger and selection
        if (!insideZone)
        {
            blowTriggered = false;
            ResetBalloons();
            return;
        }

        // Player is inside a zone, but blow was not triggered here
        if (!blowTriggered)
        {
            ResetBalloons();
            return;
        }

        // Player is inside a zone and blow was triggered in this zone:
        // keep balloons moving according to the selection made at press time,
        // even if the player moved past them.
        for (int i = 0; i < simpleMove.Length; i++)
        {
            SimpleMove s = simpleMove[i];
            if (s == null)
                continue;

            s.enabled = balloonShouldBlow[i];
        }
    }

    private void ResetBalloons()
    {
        if (simpleMove == null)
            return;

        foreach (SimpleMove s in simpleMove)
        {
            if (s != null)
                s.enabled = false;
        }

        if (balloonShouldBlow != null)
        {
            for (int i = 0; i < balloonShouldBlow.Length; i++)
                balloonShouldBlow[i] = false;
        }
    }

    // Each BlowStart has its own BlowEnd as a child.
    // Player must be between parent.x and child.x.
    private bool IsInsideBlowZone()
    {
        if (blowStarts == null || blowStarts.Length == 0)
            return false;

        float playerX = transform.position.x;

        foreach (Transform start in blowStarts)
        {
            if (start == null)
                continue;

            Transform end = null;

            if (start.childCount > 0)
            {
                end = start.GetChild(0);
            }
            else
            {
                Debug.LogWarning("BlowStart " + start.name + " has no BlowEnd child.");
                continue;
            }

            float x1 = start.position.x;
            float x2 = end.position.x;

            float minX = Mathf.Min(x1, x2);
            float maxX = Mathf.Max(x1, x2);

            if (playerX >= minX && playerX <= maxX)
                return true;
        }

        return false;
    }
}
