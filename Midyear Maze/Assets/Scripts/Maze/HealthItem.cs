using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public float healFraction = 0.25f; 

    void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth pHealth = other.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                float healAmt = pHealth.maxHealth * healFraction;
                pHealth.Heal(healAmt);
                Destroy(gameObject);
            }
        }
        else
        {
            AIControllerWithPathfinding ai = other.GetComponent<AIControllerWithPathfinding>();
            if (ai != null)
            {
                float healAmt = ai.maxHealth * healFraction;
                ai.Heal(healAmt);
                Destroy(gameObject);
            }
        }
    }
}
