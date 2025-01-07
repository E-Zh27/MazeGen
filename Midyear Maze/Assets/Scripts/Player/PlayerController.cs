using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpHeight = 1.0f;
    public float gravity = -9.81f;
    public float turnSmoothTime = 0.1f;
    public Transform cameraTransform; // Reference to the camera's Transform

    private CharacterController controller;
    private Vector3 velocity;
    private float turnSmoothVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float sprintSpeed = speed * 1.5f;
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

           
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            if (Input.GetKey(KeyCode.LeftShift))
                controller.Move(moveDir.normalized * sprintSpeed * Time.deltaTime);
            else
                controller.Move(moveDir.normalized * speed * Time.deltaTime);
            
        }

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

}
