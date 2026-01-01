using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Allows balloons to start moving (SimpleMove) when the player triggers a blow
 * inside a BlowStart zone, and only for balloons in front of the player
 * within a certain X distance.
 *
 * - Attach this script to the PLAYER.
 * - Each balloon uses a SimpleMove component (disabled at start).
 * - BlowStart objects have this tag and each has one child as BlowEnd,
 *   defining the zone in which blowing is allowed.
 */

public class BlowUpBalloons : MonoBehaviour
{
    public enum BlowControlMode
    {
        Keyboard,
        Breath
    }

    [Header("Control Mode")]
    [SerializeField] private BlowControlMode controlMode = BlowControlMode.Keyboard;

    [Header("Components")]
    [SerializeField] private SimpleMove[] simpleMove;

    [Header("Blow Up Button (Keyboard)")]
    [SerializeField] private InputAction blowUpButton = new InputAction(type: InputActionType.Button);

    [Header("Blow Zone Tags")]
    [SerializeField] private string blowStartTag = "BlowStart";

    [Header("Blow Distance")]
    [SerializeField] private float maxBlowDistanceX = 10f;

    [Header("Breath Control")]
    [SerializeField] private PressureReaderFromSerial pressureSource;
    [SerializeField] private float breathThresholdKPa = 1.0f;

    // All BlowStart objects in the scene
    private Transform[] blowStarts;

    // For each balloon: true if it should move after a valid blow in this zone
    private bool[] balloonShouldBlow;

    // True if blow was successfully triggered inside a zone
    private bool blowTriggered = false;

    // Track breath state to detect rising edge
    private bool wasBreathStrong = false;

    // Initialize balloons and find all BlowStart markers
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

        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(blowStartTag);
        blowStarts = new Transform[startObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            blowStarts[i] = startObjs[i].transform;

        if (blowStarts.Length == 0)
            Debug.LogWarning("BlowUpBalloons: No BlowStart objects found.");
    }

    // Enable keyboard input only when using keyboard mode
    private void OnEnable()
    {
        if (controlMode == BlowControlMode.Keyboard)
        {
            blowUpButton.Enable();
            blowUpButton.performed += OnBlowPressed;
        }
    }

    // Disable keyboard input and reset state on disable
    private void OnDisable()
    {
        if (controlMode == BlowControlMode.Keyboard)
        {
            blowUpButton.performed -= OnBlowPressed;
            blowUpButton.Disable();
        }

        blowTriggered = false;
        ResetBalloons();
        wasBreathStrong = false;
    }

    // Handle keyboard blow press
    private void OnBlowPressed(InputAction.CallbackContext ctx)
    {
        if (controlMode != BlowControlMode.Keyboard)
            return;

        TriggerBlow();
    }

    // Shared blow trigger logic for both keyboard and breath
    private void TriggerBlow()
    {
        if (!IsInsideBlowZone())
        {
            Debug.Log("BlowUpBalloons: blow triggered outside blow zone (ignored)");
            return;
        }

        if (simpleMove == null || simpleMove.Length == 0)
            return;

        float playerX = transform.position.x;
        bool anyNewBalloonFound = false;

        // When blow is triggered, select balloons in front and within distance
        for (int i = 0; i < simpleMove.Length; i++)
        {
            SimpleMove s = simpleMove[i];
            if (s == null)
                continue;

            if (balloonShouldBlow[i])
                continue;

            float balloonX = s.transform.position.x;
            float distanceX = balloonX - playerX;

            if (distanceX > 0 && distanceX <= maxBlowDistanceX)
            {
                balloonShouldBlow[i] = true;
                anyNewBalloonFound = true;
            }
        }

        if (anyNewBalloonFound)
        {
            blowTriggered = true;
            Debug.Log("BlowUpBalloons: New balloons added to flight");
        }
        else if (blowTriggered)
        {
            Debug.Log("BlowUpBalloons: Triggered again but no new balloons found (keeping old ones flying)");
        }
        else
        {
            Debug.Log("BlowUpBalloons: No balloons in front within distance, blow ignored");
        }
    }

    // Handle breath-based triggering when in Breath mode
    private void UpdateBreathControl()
    {
        if (pressureSource == null)
            return;

        float pressure = pressureSource.lastPressureKPa;
        bool breathStrong = pressure >= breathThresholdKPa;

        // When breath crosses the threshold upward inside a zone, act like a blow press
        if (breathStrong && !wasBreathStrong)
        {
            if (IsInsideBlowZone())
            {
                TriggerBlow();
                Debug.Log($"BlowUpBalloons: Blow triggered by breath, pressure={pressure:0.00} kPa");
            }
        }

        wasBreathStrong = breathStrong;
    }

    // Update moving state of balloons based on zone and trigger state
    private void Update()
    {
        if (controlMode == BlowControlMode.Breath)
        {
            UpdateBreathControl();
        }

        if (simpleMove == null || simpleMove.Length == 0)
            return;

        bool insideZone = IsInsideBlowZone();

        // Leaving the zone resets everything
        if (!insideZone)
        {
            blowTriggered = false;
            ResetBalloons();
            return;
        }

        // Inside zone but blow was never triggered here
        if (!blowTriggered)
        {
            ResetBalloons();
            return;
        }

        // Inside zone and blow was triggered: keep only selected balloons moving
        for (int i = 0; i < simpleMove.Length; i++)
        {
            SimpleMove s = simpleMove[i];
            if (s == null)
                continue;

            s.enabled = balloonShouldBlow[i];
        }
    }

    // Disable all balloons and clear selection
    private void ResetBalloons()
    {
        if (simpleMove != null)
        {
            foreach (SimpleMove s in simpleMove)
            {
                if (s != null)
                    s.enabled = false;
            }
        }

        if (balloonShouldBlow != null)
        {
            for (int i = 0; i < balloonShouldBlow.Length; i++)
                balloonShouldBlow[i] = false;
        }
    }

    // Check if player is inside any blow zone (between BlowStart and its child BlowEnd)
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
