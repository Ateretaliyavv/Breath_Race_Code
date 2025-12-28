using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Allows the player to push a crate only between PushStart / PushEnd markers
 * while holding an InputAction button.
 *
 * - Attach this script to the PLAYER.
 * - The crate must have a Rigidbody2D and a tag (e.g. "PushBox").
 * - PushStart / PushEnd are empty GameObjects placed in the scene;
 *   their tags are set in the inspector.
 */

public class PushBox : MonoBehaviour
{
    [Header("Input (New Input System)")]
    [Tooltip("Input action used to push the box (Button type).")]
    [SerializeField]
    private InputAction pushAction = new InputAction(type: InputActionType.Button);

    [Header("Tags")]
    [Tooltip("Tag of objects that mark where pushing can start")]
    [SerializeField] private string pushStartTag = "PushStart";

    [Tooltip("Tag of objects that mark where pushing must end")]
    [SerializeField] private string pushEndTag = "PushEnd";

    [Tooltip("Tag of the pushable crate")]
    [SerializeField] private string pushBoxTag = "PushBox";

    [Header("Optional Push Limits")]
    [Tooltip("Maximum horizontal speed for the box when pushing (for safety)")]
    [SerializeField] private float maxBoxSpeedX = 5f;

    private Transform[] pushStarts;
    private Transform[] pushEnds;

    // The crate we are currently touching (if any)
    private Rigidbody2D currentBoxRb;

    private void Awake()
    {
        // --- Find all PushStart and PushEnd objects by tag ---
        GameObject[] startObjs = GameObject.FindGameObjectsWithTag(pushStartTag);
        GameObject[] endObjs = GameObject.FindGameObjectsWithTag(pushEndTag);

        pushStarts = new Transform[startObjs.Length];
        pushEnds = new Transform[endObjs.Length];

        for (int i = 0; i < startObjs.Length; i++)
            pushStarts[i] = startObjs[i].transform;

        for (int i = 0; i < endObjs.Length; i++)
            pushEnds[i] = endObjs[i].transform;

        if (pushStarts.Length == 0)
            Debug.LogWarning("PlayerPushBox: No objects found with tag " + pushStartTag);
        if (pushEnds.Length == 0)
            Debug.LogWarning("PlayerPushBox: No objects found with tag " + pushEndTag);
    }

    private void OnEnable()
    {
        // ���� ��Input System ����
        pushAction.Enable();
    }

    private void OnDisable()
    {
        pushAction.Disable();
    }

    private void FixedUpdate()
    {
        if (currentBoxRb == null)
            return;

        // ����� ������ ���� �����
        bool isKeyHeld = pushAction.IsPressed();
        bool inAllowedZone = IsInPushZone(transform.position.x);

        if (isKeyHeld && inAllowedZone)
        {
            // ���� �����: ������� �� �-X (������� FreezeRotation)
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // ����� ������ ������ (�� ���� ��� ����)
            Vector2 v = currentBoxRb.linearVelocity;
            v.x = Mathf.Clamp(v.x, -maxBoxSpeedX, maxBoxSpeedX);
            currentBoxRb.linearVelocity = v;
        }
        else
        {
            // ���� �����: ������� �� ��� �-X
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation |
                                       RigidbodyConstraints2D.FreezePositionX;
        }
    }

    /// <summary>
    /// Checks if the given x-position is inside an allowed push region.
    /// ����� ��� �-BridgeBuilder:
    /// - ������ �� PushStart ������ ��������
    /// - ������ �� PushEnd ������ ��������
    /// - ���� ����� �� ����� PushStart ����� ������� ������ �������� ��� Start.
    /// </summary>
    private bool IsInPushZone(float playerX)
    {
        float lastStartX = float.NegativeInfinity;
        float lastEndX = float.NegativeInfinity;

        foreach (Transform s in pushStarts)
        {
            if (s == null) continue;
            if (s.position.x <= playerX && s.position.x > lastStartX)
                lastStartX = s.position.x;
        }

        foreach (Transform e in pushEnds)
        {
            if (e == null) continue;
            if (e.position.x <= playerX && e.position.x > lastEndX)
                lastEndX = e.position.x;
        }

        // �� �� ����� ����� PushStart ��� � ���� �����
        if (lastStartX == float.NegativeInfinity)
            return false;

        // �� ������ ������ �������� ��� End (�� ����) � ���� �����
        if (lastEndX >= lastStartX)
            return false;

        // ����: ������ ������ ��� Start � ����
        return true;
    }

    // ��������� �� ����� � ������ �� �-Rigidbody ���
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(pushBoxTag))
            return;

        currentBoxRb = collision.collider.attachedRigidbody;
        if (currentBoxRb != null)
        {
            // ���� ������� � ����� ���� ���� X ������ ����
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation |
                                       RigidbodyConstraints2D.FreezePositionX;
        }
    }

    // ��������� ���� ����� � ������ ����
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (currentBoxRb == null)
            return;

        if (collision.collider.attachedRigidbody == currentBoxRb)
        {
            // ������� �� ����� � ����� ���� ���� �-X
            currentBoxRb.constraints = RigidbodyConstraints2D.FreezeRotation |
                                       RigidbodyConstraints2D.FreezePositionX;
            currentBoxRb = null;
        }
    }
}
