using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public Animator animator;  
    private float screamTimer = 0f;  
    private float screamCooldown = 10f; 

    private CharacterController controller; 
    private ShadowSeeker shadowSeeker; 
    private float speed = 0f;  

    void Start()
    {
        
        controller = GetComponent<CharacterController>();

        
        if (controller == null)
        {
            Debug.LogWarning("CharacterController not found on " + gameObject.name + ". Speed will be set manually.");
        }

        
        GameObject aiObject = GameObject.FindGameObjectWithTag("AI");
        if (aiObject != null)
        {
            shadowSeeker = aiObject.GetComponent<ShadowSeeker>();
            if (shadowSeeker != null)
            {
                speed = shadowSeeker.walkSpeed;  
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
        if (controller != null)
        {
            speed = controller.velocity.magnitude; 
        }

        animator.SetFloat("Speed", speed);

        screamTimer -= Time.deltaTime;
        if (screamTimer <= 0f)
        {
            if (Random.value < 0.01f)  
            {
                animator.SetTrigger("ScreamTrigger");
                speed = 0;
                screamTimer = screamCooldown; 
            }
        }
    }
}

