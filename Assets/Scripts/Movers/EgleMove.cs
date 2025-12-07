using UnityEngine;

public class MoveByDirection : MonoBehaviour
{
    [SerializeField] private Vector3 direction = Vector3.right; // Movement direction (set in Inspector)
    [SerializeField] private float speed = 5f;                  // Movement speed

    private void Update()
    {
        // Normalize to ensure consistent speed in any direction
        Vector3 normalizedDir = direction.normalized;

        // Move the object
        transform.position += normalizedDir * speed * Time.deltaTime;
    }
}
