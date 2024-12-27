using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;       // The player to follow
    public Vector3 offset;         // Offset from the player
    public float rotationSpeed = 5.0f;
    public float smoothSpeed = 0.125f;
    private float yaw = 0f;        // Horizontal rotation around the player
    private float pitch = 0f;      // Vertical rotation around the player
    private float minPitch = -180f; // Minimum pitch angle
    private float maxPitch = 120f;  // Maximum pitch angle

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor
    }

    void LateUpdate()
    {
        HandleMouseRotation();
        AlignPlayerWithCamera();
        FollowPlayer();
    }

    private void HandleMouseRotation()
    {
        // Get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        yaw += mouseX;          // Adjust yaw based on horizontal mouse movement
        pitch += mouseY;        // Adjust pitch based on vertical mouse movement
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); // Clamp pitch

        // Create rotation based on pitch and yaw
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        transform.position = target.position + rotation * offset; // Rotate camera around the target based on offset

        transform.LookAt(target); // Make sure camera looks at player
    }

    private void AlignPlayerWithCamera()
    {
        // Rotate the player to face the same direction as the camera on the horizontal plane
        Vector3 cameraForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        target.rotation = Quaternion.LookRotation(cameraForward);
    }

    private void FollowPlayer()
    {
        // Smoothly interpolate the camera's position for smooth following
        Vector3 desiredPosition = target.position + (transform.position - target.position).normalized * offset.magnitude;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
