using UnityEngine;
using System.Collections;

public class RandomEventManager : MonoBehaviour
{
    public float damageInterval = 10f; 
    public float damageAmount = 1f;

    public float itemSpawnInterval = 20f;
    public GameObject healthItemPrefab;

    private MazeGenerator mazeGenerator;
    private PlayerHealth playerHealth;
    private AIControllerWithPathfinding aiController;

    void Start()
    {
        mazeGenerator = FindObjectOfType<MazeGenerator>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player) playerHealth = player.GetComponent<PlayerHealth>();

        GameObject aiObj = GameObject.Find("AI");
        if (aiObj) aiController = aiObj.GetComponent<AIControllerWithPathfinding>();

        StartCoroutine(DamageTick());
        StartCoroutine(SpawnHealthItem());
    }

    IEnumerator DamageTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(damageInterval);

            // Damage both
            if (playerHealth != null)
                playerHealth.TakeDamage(damageAmount);

            if (aiController != null)
                aiController.TakeDamage(damageAmount);
        }
    }

    IEnumerator SpawnHealthItem()
    {
        while (true)
        {
            yield return new WaitForSeconds(itemSpawnInterval);

            // Pick random floor cell
            Vector2Int cell = mazeGenerator.PickRandomFloorCell();
            Vector3 spawnPos = new Vector3(cell.x, 1f, cell.y);

            Instantiate(healthItemPrefab, spawnPos, Quaternion.identity);
        }
    }
}
