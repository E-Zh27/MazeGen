using UnityEngine;
using System.Collections;

public class RandomEventManager : MonoBehaviour
{
    public float damageInterval = 10f;
    public float damageAmount = .5f;

    public float itemSpawnInterval = 30f;
    public GameObject healthItemPrefab;

    private MazeGenerator mazeGenerator;
    private PlayerHealth playerHealth;
    private AIHealth aiHealth;

    void Start()
    {
        mazeGenerator = FindObjectOfType<MazeGenerator>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        GameObject aiObj = GameObject.Find("AI");
        if (aiObj)
        {
            aiHealth = aiObj.GetComponent<AIHealth>();
        }

        StartCoroutine(DamageTick());
        StartCoroutine(SpawnHealthItem());
    }

    IEnumerator DamageTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(damageInterval);

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }

            if (aiHealth != null)
            {
                aiHealth.TakeDamage(damageAmount);
            }
        }
    }

    IEnumerator SpawnHealthItem()
    {
        while (true)
        {
            yield return new WaitForSeconds(itemSpawnInterval);

            Vector2Int cell = mazeGenerator.PickRandomFloorCell();
            Vector3 spawnPos = new Vector3(cell.x, 1f, cell.y);

            Instantiate(healthItemPrefab, spawnPos, Quaternion.identity);
        }
    }
}
