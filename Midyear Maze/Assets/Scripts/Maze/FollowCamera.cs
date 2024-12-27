using UnityEngine;

public class CameraFollowAI : MonoBehaviour
{
    public Transform aiTarget;      // Drag your AI GameObject’s transform here
    public Vector3 offset = new Vector3(0f, 10f, -10f);
    public float followSpeed = 5f;

    void Update()
    {
        if (aiTarget == null) return;

        Vector3 desiredPosition = aiTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        transform.position = smoothedPosition;

        // Optionally, look down at the AI:
        transform.LookAt(aiTarget.position);
    }
}
