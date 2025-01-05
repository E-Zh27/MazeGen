using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class AIControllerWithPathFinding : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float gravityForce = -9.81f;
    public float checkInterval = 1f;
    public float pathInterval = 2f;
    public float viewRange = 10f;
    public bool debugLogs = true;

    private CharacterController mover;
    private Transform target;
    private Dictionary<Vector2Int, CellType> knownCells = new Dictionary<Vector2Int, CellType>();
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int pathIndex;
    private float checkTimer;
    private float pathTimer;
    private BrainState brainState = BrainState.Searching;
    private bool seesHuman;
    private HashSet<Vector2Int> deadEnds = new HashSet<Vector2Int>();

    private enum BrainState
    {
        Searching,
        Chasing
    }

    private enum CellType
    {
        Unknown,
        Clear,
        Blocked
    }

    void Awake()
    {
        mover = GetComponent<CharacterController>();
        GameObject foundTarget = GameObject.FindGameObjectWithTag("Player");
        if (foundTarget) target = foundTarget.transform;
        Vector2Int startCell = CellFromWorld(transform.position);
        knownCells[startCell] = CellType.Clear;
    }

    void Update()
    {
        if (!target) return;
        seesHuman = CheckVisibility();
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            DiscoverCells();
        }
        pathTimer += Time.deltaTime;
        if (pathTimer >= pathInterval)
        {
            pathTimer = 0f;
            RefreshBrainState();
            PerformBrainActions();
        }
        if (brainState == BrainState.Chasing || brainState == BrainState.Searching)
        {
            FollowRoute();
        }
    }

    bool CheckVisibility()
    {
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > viewRange) return false;
        Vector3 direction = (target.position - transform.position).normalized;
        float range = dist;
        Vector3 start = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(start, direction, out RaycastHit hit, range))
        {
            if (hit.collider.CompareTag("Player")) return true;
            return false;
        }
        return false;
    }

    void RefreshBrainState()
    {
        if (seesHuman) brainState = BrainState.Chasing;
        else brainState = BrainState.Searching;
    }

    void PerformBrainActions()
    {
        switch (brainState)
        {
            case BrainState.Chasing:
                PlanPathToPlayer();
                break;
            case BrainState.Searching:
                PlanPathSearching();
                break;
        }
    }

    void PlanPathToPlayer()
    {
        Vector2Int start = CellFromWorld(transform.position);
        Vector2Int end = CellFromWorld(target.position);
        List<Vector2Int> route = AStarFindPath(start, end);
        if (route.Count > 0)
        {
            if (debugLogs) Debug.Log("[FSM] Chasing route found.");
            currentPath = route;
            pathIndex = 0;
        }
        else
        {
            if (debugLogs) Debug.LogWarning("[FSM] Cannot reach player. Searching instead.");
            brainState = BrainState.Searching;
            PlanPathSearching();
        }
    }

    void PlanPathSearching()
    {
        Vector2Int selfCell = CellFromWorld(transform.position);
        Vector2Int frontier = FindFrontier(selfCell);
        if (frontier.x >= 0)
        {
            List<Vector2Int> route = AStarFindPath(selfCell, frontier);
            if (route.Count > 0)
            {
                currentPath = route;
                pathIndex = 0;
                return;
            }
        }
        deadEnds.Add(selfCell);
        if (debugLogs) Debug.LogWarning("[FSM] No frontier or path found, marking dead end.");
    }

    void FollowRoute()
    {
        if (currentPath == null || currentPath.Count == 0) return;
        if (pathIndex >= currentPath.Count) return;
        Vector2Int step = currentPath[pathIndex];
        Vector3 stepPos = new Vector3(step.x, transform.position.y, step.y);
        Vector3 moveDir = stepPos - transform.position;
        float dist = moveDir.magnitude;
        if (dist < 0.2f)
        {
            pathIndex++;
            return;
        }
        MoveForward(moveDir.normalized);
    }

    void DiscoverCells()
    {
        Vector2Int center = CellFromWorld(transform.position);
        if (!knownCells.ContainsKey(center)) knownCells[center] = CellType.Clear;
        Vector2Int[] neighborOffsets =
        {
            center + new Vector2Int( 1,  0),
            center + new Vector2Int(-1,  0),
            center + new Vector2Int( 0,  1),
            center + new Vector2Int( 0, -1)
        };
        foreach (var n in neighborOffsets)
        {
            if (!knownCells.ContainsKey(n))
            {
                Vector3 start = new Vector3(center.x, 1f, center.y);
                Vector3 end   = new Vector3(n.x,      1f, n.y);
                if (CheckWall(start, end)) knownCells[n] = CellType.Blocked;
                else knownCells[n] = CellType.Clear;
            }
        }
    }

    bool CheckWall(Vector3 start, Vector3 end)
    {
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);
        if (Physics.Raycast(start, dir, out RaycastHit hit, dist))
        {
            if (hit.collider.CompareTag("Wall")) return true;
        }
        return false;
    }

    Vector2Int FindFrontier(Vector2Int from)
    {
        List<Vector2Int> edges = new List<Vector2Int>();
        foreach (var kvp in knownCells)
        {
            if (kvp.Value == CellType.Clear && !deadEnds.Contains(kvp.Key))
            {
                if (HasUnknownNeighbor(kvp.Key)) edges.Add(kvp.Key);
            }
        }
        if (edges.Count == 0) return new Vector2Int(-1, -1);
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(from);
        visited.Add(from);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (edges.Contains(current)) return current;
            foreach (var n in GetNeighbors(current))
            {
                if (!visited.Contains(n) &&
                    knownCells.ContainsKey(n) &&
                    knownCells[n] == CellType.Clear &&
                    !deadEnds.Contains(n))
                {
                    visited.Add(n);
                    queue.Enqueue(n);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    bool HasUnknownNeighbor(Vector2Int spot)
    {
        foreach (var n in GetNeighbors(spot))
        {
            if (!knownCells.ContainsKey(n)) return true;
        }
        return false;
    }

    List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        List<Vector2Int> near = new List<Vector2Int>();
        near.Add(cell + new Vector2Int( 1,  0));
        near.Add(cell + new Vector2Int(-1,  0));
        near.Add(cell + new Vector2Int( 0,  1));
        near.Add(cell + new Vector2Int( 0, -1));
        return near;
    }

    void MoveForward(Vector3 dir)
    {
        Vector3 velocity = dir * walkSpeed;
        velocity.y += gravityForce * Time.deltaTime;
        mover.Move(velocity * Time.deltaTime);
        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    Vector2Int CellFromWorld(Vector3 pos)
    {
        int cx = Mathf.RoundToInt(pos.x);
        int cz = Mathf.RoundToInt(pos.z);
        return new Vector2Int(cx, cz);
    }

    List<Vector2Int> AStarFindPath(Vector2Int start, Vector2Int goal)
    {
        if (!knownCells.ContainsKey(start) || knownCells[start] == CellType.Blocked || deadEnds.Contains(start))
        {
            if (debugLogs) Debug.Log("[A*] Start invalid.");
            return new List<Vector2Int>();
        }
        if (!knownCells.ContainsKey(goal) || knownCells[goal] == CellType.Blocked || deadEnds.Contains(goal))
        {
            if (debugLogs) Debug.Log("[A*] Goal invalid.");
            return new List<Vector2Int>();
        }
        PrioritySet<Vector2Int> openPoints = new PrioritySet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> routeMap = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> costG = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> costF = new Dictionary<Vector2Int, float>();

        costG[start] = 0f;
        costF[start] = Manhattan(start, goal);
        openPoints.Enqueue(start, costF[start]);

        while (openPoints.Count > 0)
        {
            Vector2Int current = openPoints.Dequeue();
            if (current == goal) return RebuildPath(routeMap, current);
            foreach (var n in FourDirs(current))
            {
                if (!knownCells.ContainsKey(n)) continue;
                if (knownCells[n] == CellType.Blocked) continue;
                if (deadEnds.Contains(n)) continue;
                float tentative = costG[current] + 1f;
                if (!costG.ContainsKey(n) || tentative < costG[n])
                {
                    routeMap[n] = current;
                    costG[n] = tentative;
                    float f = tentative + Manhattan(n, goal);
                    costF[n] = f;
                    if (!openPoints.Contains(n)) openPoints.Enqueue(n, f);
                    else openPoints.Update(n, f);
                }
            }
        }
        if (debugLogs) Debug.Log("[A*] No path found.");
        return new List<Vector2Int>();
    }

    float Manhattan(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2Int> RebuildPath(Dictionary<Vector2Int, Vector2Int> routeMap, Vector2Int node)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(node);
        while (routeMap.ContainsKey(node) && routeMap[node] != node)
        {
            node = routeMap[node];
            path.Add(node);
        }
        path.Reverse();
        return path;
    }

    List<Vector2Int> FourDirs(Vector2Int cell)
    {
        List<Vector2Int> around = new List<Vector2Int>();
        around.Add(cell + new Vector2Int( 1,  0));
        around.Add(cell + new Vector2Int(-1,  0));
        around.Add(cell + new Vector2Int( 0,  1));
        around.Add(cell + new Vector2Int( 0, -1));
        return around;
    }
}

public class PrioritySet<T>
{
    private List<(T item, float priority)> elements = new List<(T, float)>();
    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        float bestPriority = elements[0].priority;
        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < bestPriority)
            {
                bestPriority = elements[i].priority;
                bestIndex = i;
            }
        }
        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }

    public bool Contains(T item)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(elements[i].item, item)) return true;
        }
        return false;
    }

    public void Update(T item, float newPriority)
    {
        for(int i = 0; i < elements.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(elements[i].item, item))
            {
                elements[i] = (item, newPriority);
                return;
            }
        }
        Enqueue(item, newPriority);
    }
}
