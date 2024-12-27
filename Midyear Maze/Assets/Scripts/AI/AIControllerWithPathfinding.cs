using UnityEngine;
using System.Collections.Generic;

public class AIControllerWithPathfinding : MonoBehaviour
{
    public float chaseSpeed = 2f;     // Speed if we believe "near"
    public float searchSpeed = .5f;    // Speed if we believe "far"

    public float detectionRange = 10f;   // If < distance, observation suggests "see" the player
    // Transition probabilities (two-state HMM: near=0, far=1)
    public float pNearToNear = 0.7f;
    public float pNearToFar  = 0.3f;
    public float pFarToNear  = 0.4f;
    public float pFarToFar   = 0.6f;

    public float seeIfNear    = 0.8f;
    public float seeIfFar     = 0.2f;
    public float notSeeIfNear = 0.2f;
    public float notSeeIfFar  = 0.8f;

    public float revealInterval = 10f; // Every 20 seconds we get the exact location
    private float revealTimer = 0f;    // Counts up to revealInterval

    public float attackRange    = 2f;
    public float health         = 10f;
    public float maxHealth      = 10f;
    public float damageOnAttack = .5f;

    private CharacterController controller;
    private Transform playerTransform;
    private Pathfinding pathfinding;

    private Vector2Int lastKnownPlayerCell = new Vector2Int(-1, -1);

    private List<Node> currentPath = new List<Node>();
    private int pathIndex = 0;

    private float[] belief = new float[2] { 0.5f, 0.5f };

    private const int NEAR = 0;
    private const int FAR  = 1;

    private enum AIState { Chasing, Searching }
    private AIState currentState = AIState.Searching;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("[AI] No GameObject tagged 'Player' found!");
        }

        pathfinding = FindObjectOfType<Pathfinding>();
        if (!pathfinding)
        {
            Debug.LogError("[AI] No Pathfinding script found in the scene!");
        }
    }

    void Update()
    {
        if (!playerTransform || !pathfinding) return;

        UpdateHMM();

        DecideState();

        revealTimer += Time.deltaTime;
        if (revealTimer >= revealInterval)
        {
            revealTimer = 0f;
            Vector2Int playerCellNow = WorldToCell(playerTransform.position);
            lastKnownPlayerCell = playerCellNow;
            Debug.Log($"[AI] Received EXACT location of player: {playerCellNow} (snapshot).");
        }

        DoPathfindingAndMove();

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist < attackRange)
        {
            PlayerHealth pHealth = playerTransform.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damageOnAttack);
                Debug.Log("[AI] Attacked the Player!");
            }
        }
    }

    private void UpdateHMM()
    {
        float bNearOld = belief[NEAR];
        float bFarOld  = belief[FAR];

        float bNearPrior = bNearOld * pNearToNear + bFarOld * pFarToNear;
        float bFarPrior  = bNearOld * pNearToFar  + bFarOld * pFarToFar;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool see = (dist < detectionRange);

        if (see)
        {
            // If we "see" the player, we also update lastKnownPlayerCell
            // (like if we just glimpsed them between official 10s pings)
            lastKnownPlayerCell = WorldToCell(playerTransform.position);

            bNearPrior *= seeIfNear;
            bFarPrior  *= seeIfFar;
        }
        else
        {
            bNearPrior *= notSeeIfNear;
            bFarPrior  *= notSeeIfFar;
        }

        float sum = bNearPrior + bFarPrior;
        if (sum > 0f)
        {
            bNearPrior /= sum;
            bFarPrior  /= sum;
        }
        else
        {
            bNearPrior = 0.5f;
            bFarPrior  = 0.5f;
        }

        belief[NEAR] = bNearPrior;
        belief[FAR]  = bFarPrior;
    }


    private void DecideState()
    {
        if (belief[NEAR] > 0.5f)
        {
            currentState = AIState.Chasing;
        }
        else
        {
            currentState = AIState.Searching;
        }
    }


    private void DoPathfindingAndMove()
    {
        if (lastKnownPlayerCell.x < 0 || lastKnownPlayerCell.y < 0)
        {
            Debug.Log("[AI] No known player location. Doing nothing or random roaming.");
            return;
        }
        Vector2Int aiCell = WorldToCell(transform.position);

        currentPath = pathfinding.FindPath(aiCell, lastKnownPlayerCell);
        if (currentPath != null && currentPath.Count > 0)
{
    Debug.Log($"[AI] Found path with {currentPath.Count} nodes from {aiCell} to {lastKnownPlayerCell}.");
}
else
{
    Debug.LogWarning($"[AI] No path from {aiCell} to lastKnown {lastKnownPlayerCell}.");
    return;
}

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning($"[AI] No path from {aiCell} to lastKnown {lastKnownPlayerCell}.");
            return;
        }

        pathIndex = 0;
        FollowPath();
    }

    private void FollowPath()
    {
        while (pathIndex < currentPath.Count)
        {
            Node nextNode = currentPath[pathIndex];
            Vector3 targetPos = new Vector3(
                nextNode.GridPosition.x,
                transform.position.y,
                nextNode.GridPosition.y
            );

            float speed = (currentState == AIState.Chasing) ? chaseSpeed : searchSpeed;
            Vector3 dir = (targetPos - transform.position).normalized;
            controller.SimpleMove(dir * speed);

            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist < 1.5f)
            {
                pathIndex++;
            }
            else
            {
                break;
            }
        }
    }

    private Vector2Int WorldToCell(Vector3 pos)
    {
        int cx = Mathf.RoundToInt(pos.x);
        int cy = Mathf.RoundToInt(pos.z);
        return new Vector2Int(cx, cy);
    }


    public void TakeDamage(float dmg)
    {
        health -= dmg;
        if (health <= 0f)
        {
            health = 0f;
            Die();
        }
    }

    public void Heal(float amt)
    {
        health += amt;
        if (health > maxHealth) health = maxHealth;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

}
