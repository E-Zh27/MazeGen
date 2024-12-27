using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public void Heal(float amt)
    {
        currentHealth += amt;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
    }

    void Die()
    {
        Debug.Log("Player died!");
    }
}
