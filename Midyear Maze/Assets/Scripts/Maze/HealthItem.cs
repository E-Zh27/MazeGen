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
                float amount = pHealth.maxHealth * healFraction;
                pHealth.Heal(amount);
                Destroy(gameObject);
            }
        }
        else
        {
            AIHealth ai = other.GetComponent<AIHealth>();
            if (ai != null)
            {
                float amount = ai.maxHealth * healFraction;
                ai.Heal(amount);
                Destroy(gameObject);
            }
        }
    }
}
