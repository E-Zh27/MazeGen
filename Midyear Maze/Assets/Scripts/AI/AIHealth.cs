using UnityEngine;
using UnityEngine.UI;

public class AIHealth : MonoBehaviour
{
    public float maxHealth = 10f;
    private float currentHealth;

    public Slider healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (healthBar) healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (healthBar) healthBar.value = currentHealth;
    }
}
