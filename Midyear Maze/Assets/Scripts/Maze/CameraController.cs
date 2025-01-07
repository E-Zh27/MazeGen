using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;       // The player to follow
    public Vector3 offset;         // Offset from the player
    public float rotationSpeed = 5.0f;
    public float smoothSpeed = 0.125f;
    private float yaw = 0f;        // Horizontal rotation around the player
    private float pitch = 0f;      // Vertical rotation around the player
    private float minPitch = -90f; // Minimum pitch angle for first-person
    private float maxPitch = 90f;  // Maximum pitch angle for first-person

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
    }

    void LateUpdate()
    {
        HandleMouseRotation();

        if (offset.magnitude > 0) // Third-person view
        {
            FollowPlayerWithOffset();
        }
        else // First-person view
        {
            FollowPlayerDirectly();
        }
    }

    private void HandleMouseRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed; // Inverted mouse fix

        yaw += mouseX;
        pitch += mouseY; // Fixed to be non-inverted
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (offset.magnitude == 0) // First-person view
        {
            target.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        else // Third-person view
        {
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    private void FollowPlayerWithOffset()
    {
        Vector3 desiredPosition = target.position + Quaternion.Euler(pitch, yaw, 0) * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.LookAt(target.position);
    }

    private void FollowPlayerDirectly()
    {
        transform.position = target.position; // Match the target's position exactly
        transform.rotation = Quaternion.Euler(pitch, yaw, 0); // Use camera rotation
    }
}
