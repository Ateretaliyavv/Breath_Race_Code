using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Allows balloons to start moving (SimpleMove) when the player triggers a blow
 * inside a BlowStart zone.
 * * UPDATES:
 * - Added Audio Support.
 * - Added Sound Duration Limit (cuts the sound if it's too long).
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

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip blowSound;
    [Tooltip("Stop the sound after this many seconds")]
    [SerializeField] private float soundDurationLimit = 1.0f; // Limit of 1 second

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
    private bool[] balloonShouldBlow;
    private bool blowTriggered = false;
    private bool wasBreathStrong = false;

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

    private void OnEnable()
    {
        if (controlMode == BlowControlMode.Keyboard)
        {
            blowUpButton.Enable();
            blowUpButton.performed += OnBlowPressed;
        }
    }

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

    private void OnBlowPressed(InputAction.CallbackContext ctx)
    {
        if (controlMode != BlowControlMode.Keyboard)
            return;

        TriggerBlow();
    }

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

        for (int i = 0; i < simpleMove.Length; i++)
        {
            SimpleMove s = simpleMove[i];
            if (s == null) continue;
            if (balloonShouldBlow[i]) continue;

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

            // --- Play sound with time limit ---
            if (audioSource != null && blowSound != null)
            {
                audioSource.clip = blowSound;
                audioSource.Play();
                // Stop the sound after the defined duration
                StartCoroutine(StopSoundAfterDelay(soundDurationLimit));
            }
        }
        else if (blowTriggered)
        {
            Debug.Log("BlowUpBalloons: Triggered again but no new balloons found");
        }
    }

    // Coroutine to stop the sound
    private IEnumerator StopSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (audioSource != null && audioSource.isPlaying)
        {
            // Check if the current clip is the one we want to stop
            // (In case another sound started playing in the meantime)
            if (audioSource.clip == blowSound)
            {
                audioSource.Stop();
            }
        }
    }

    private void UpdateBreathControl()
    {
        if (pressureSource == null) return;

        float pressure = pressureSource.lastPressureKPa;
        bool breathStrong = pressure >= breathThresholdKPa;

        if (breathStrong && !wasBreathStrong)
        {
            if (IsInsideBlowZone())
            {
                TriggerBlow();
            }
        }

        wasBreathStrong = breathStrong;
    }

    private void Update()
    {
        if (controlMode == BlowControlMode.Breath)
        {
            UpdateBreathControl();
        }

        if (simpleMove == null || simpleMove.Length == 0) return;

        bool insideZone = IsInsideBlowZone();

        if (!insideZone)
        {
            blowTriggered = false;
            ResetBalloons();
            return;
        }

        if (!blowTriggered)
        {
            ResetBalloons();
            return;
        }

        for (int i = 0; i < simpleMove.Length; i++)
        {
            SimpleMove s = simpleMove[i];
            if (s == null) continue;

            s.enabled = balloonShouldBlow[i];
        }
    }

    private void ResetBalloons()
    {
        if (simpleMove != null)
        {
            foreach (SimpleMove s in simpleMove)
            {
                if (s != null) s.enabled = false;
            }
        }

        if (balloonShouldBlow != null)
        {
            for (int i = 0; i < balloonShouldBlow.Length; i++)
                balloonShouldBlow[i] = false;
        }
    }

    private bool IsInsideBlowZone()
    {
        if (blowStarts == null || blowStarts.Length == 0) return false;

        float playerX = transform.position.x;

        foreach (Transform start in blowStarts)
        {
            if (start == null) continue;

            Transform end = null;
            if (start.childCount > 0) end = start.GetChild(0);
            else continue;

            float x1 = start.position.x;
            float x2 = end.position.x;

            float minX = Mathf.Min(x1, x2);
            float maxX = Mathf.Max(x1, x2);

            if (playerX >= minX && playerX <= maxX) return true;
        }

        return false;
    }
}
