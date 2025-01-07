using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public float health = 10f;
    public Slider healthBar;
    private bool canTakeDamage = true;
    public float damageCooldown = 1.5f; 
    public AudioSource audioData;


    public float maxHealth = 10f;

    void Start()
    {
        healthBar.maxValue = health;
        healthBar.value = health;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        healthBar.value = health;

        if (health <= 0)
        {
            audioData = GetComponent<AudioSource>();
            audioData.Play(0);
            Destroy(gameObject);  
            SceneManager.LoadScene(3);
        }
        else
            StartCoroutine(DamageCooldown()); 
    }

    private IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canTakeDamage = true;
    }

    public void Heal(float amt)
    {
        health += amt;
        if (health > maxHealth) 
            health = maxHealth;
        healthBar.value = health;

    }

    private void OnCollisionEnter(Collision collision)
{
    Debug.Log($"Collision detected with: {collision.gameObject.name}");

    if (collision.gameObject.CompareTag("AI"))
    {
        Debug.Log("Collided with AI. Destroying player.");
        Destroy(gameObject); // Destroy the player
    }
}

}



