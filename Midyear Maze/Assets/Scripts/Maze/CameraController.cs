using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float rotationSpeed = 5.0f;
    public float smoothSpeed = 0.125f;
    public float cameraCollisionRadius = 0.2f;   // how “thick” the camera collision is
    public LayerMask collisionMask;             // layer(s) for walls/obstacles

    private float yaw = 0f;
    private float pitch = 0f;
    private float minPitch = -180f;
    private float maxPitch = 120f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        HandleMouseRotation();
        AlignPlayerWithCamera();
        FollowPlayer();
    }

    void HandleMouseRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        yaw += mouseX;
        pitch += mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Ideal position ignoring walls
        Vector3 desiredPos = target.position + rotation * offset;

        // Adjust for camera collision
        Vector3 actualPos = CheckWallCollision(target.position, desiredPos);

        transform.position = actualPos;
        transform.LookAt(target);
    }

    void AlignPlayerWithCamera()
    {
        Vector3 camForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        target.rotation = Quaternion.LookRotation(camForward);
    }

    void FollowPlayer()
    {
        // You can optionally do a smoothing step here if you want
        // But we already handle smoothing or camera collision in CheckWallCollision
    }

    Vector3 CheckWallCollision(Vector3 startPos, Vector3 desiredCamPos)
    {
        Vector3 dir = desiredCamPos - startPos;
        float dist = dir.magnitude;
        dir.Normalize();

        // If we hit something, shorten the distance
        if (Physics.SphereCast(startPos, cameraCollisionRadius, dir, out RaycastHit hit, dist, collisionMask))
        {
            // Position the camera just in front of the obstacle
            return hit.point - dir * cameraCollisionRadius;
        }
        return desiredCamPos;
    }
}
