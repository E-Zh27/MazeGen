using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public Animator animator;  // Reference to the Animator component
    private float screamTimer = 0f;  // Timer to control random scream timing
    private float screamCooldown = 10f; // How often to scream (in seconds)

    private CharacterController controller;  // Reference to the CharacterController component
    private ShadowSeeker shadowSeeker; // Reference to the ShadowSeeker script (which has walkSpeed)
    private float speed = 0f;  // The current speed of the character

    void Start()
    {
        // Try to get the CharacterController component
        controller = GetComponent<CharacterController>();

        // If there's no CharacterController, log a warning but continue without it
        if (controller == null)
        {
            Debug.LogWarning("CharacterController not found on " + gameObject.name + ". Speed will be set manually.");
        }

        // Find the AI object and get the ShadowSeeker component
        GameObject aiObject = GameObject.FindGameObjectWithTag("AI");
        if (aiObject != null)
        {
            shadowSeeker = aiObject.GetComponent<ShadowSeeker>();
            if (shadowSeeker != null)
            {
                speed = shadowSeeker.walkSpeed;  // Initialize speed with ShadowSeeker's walkSpeed
            }
            else
            {
                Debug.LogWarning("ShadowSeeker component not found on AI object.");
            }
        }
        else
        {
            Debug.LogWarning("No object found with tag 'AI'.");
        }
    }

    void Update()
    {
        // If a CharacterController exists, use its velocity to calculate speed
        if (controller != null)
        {
            speed = controller.velocity.magnitude; // Use the magnitude of the velocity for speed
        }

        // Update the Animator speed parameter
        animator.SetFloat("Speed", speed);

        // Random scream logic
        screamTimer -= Time.deltaTime;
        if (screamTimer <= 0f)
        {
            // Randomly trigger the scream
            if (Random.value < 0.01f)  // Adjust this probability to control how often scream happens
            {
                animator.SetTrigger("ScreamTrigger");
                speed = 0;
                screamTimer = screamCooldown; // Reset the timer
            }
        }
    }
}

