using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class ShadowSeeker : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float fallForce = -9.81f;
    public float lookAroundFrequency = 1f;
    public float planFrequency = 2f;
    public int memorySize = 3;
    public bool debugMessages = true;

    private CharacterController mover;
    private Transform humanTarget;
    private Dictionary<Vector2Int, PlaceType> knownPlaces = new Dictionary<Vector2Int, PlaceType>();
    private List<Vector2Int> travelRoute = new List<Vector2Int>();
    private int routeIndex;
    private float lookTimer;
    private float planTimer;
    private State currentState = State.Idle;
    private Queue<Vector3> targetHistory = new Queue<Vector3>();
    private HashSet<Vector2Int> blockedSpots = new HashSet<Vector2Int>();

    private enum State { Moving, Idle }
    private enum PlaceType { Unknown, Clear, Blocked }

    
    void Awake()
    {
        mover = GetComponent<CharacterController>();
        GameObject foundHuman = GameObject.FindGameObjectWithTag("Player");
        if (foundHuman) humanTarget = foundHuman.transform;
        Vector2Int here = LocateCell(transform.position);
        knownPlaces[here] = PlaceType.Clear;
    }

    void Update()
    {
        if (!humanTarget) return;
        RecordTargetPath();
        lookTimer += Time.deltaTime;
        if (lookTimer >= lookAroundFrequency)
        {
            lookTimer = 0f;
            LookAround();
        }
        planTimer += Time.deltaTime;
        if (planTimer >= planFrequency)
        {
            planTimer = 0f;
            DecideRoute();
        }
        if (currentState == State.Moving) FollowRoute();
    }

    void RecordTargetPath()
    {
        targetHistory.Enqueue(humanTarget.position);
        if (targetHistory.Count > memorySize) targetHistory.Dequeue();
    }

    void LookAround()
    {
        Vector2Int center = LocateCell(transform.position);
        if (!knownPlaces.ContainsKey(center)) knownPlaces[center] = PlaceType.Clear;
        Vector2Int[] nearSpots = 
        {
            center + new Vector2Int(1,0),
            center + new Vector2Int(-1,0),
            center + new Vector2Int(0,1),
            center + new Vector2Int(0,-1)
        };
        foreach (var n in nearSpots)
        {
            if (!knownPlaces.ContainsKey(n))
            {
                Vector3 start = new Vector3(center.x, 1f, center.y);
                Vector3 end   = new Vector3(n.x,      1f, n.y);
                if (BlockedPath(start, end)) knownPlaces[n] = PlaceType.Blocked;
                else knownPlaces[n] = PlaceType.Clear;
            }
        }
    }

    void DecideRoute()
    {
        Vector2Int selfCell = LocateCell(transform.position);
        Vector2Int guessCell = GuessTargetCell();
        List<Vector2Int> pathGuess = AStarPath(selfCell, guessCell);
        if (pathGuess.Count > 0)
        {
            if (debugMessages) Debug.Log("[Seeker] Found path to guess cell.");
            travelRoute = pathGuess;
            routeIndex = 0;
            currentState = State.Moving;
            return;
        }
        Vector2Int border = NearestEdge(selfCell);
        if (border.x >= 0)
        {
            List<Vector2Int> routeEdge = AStarPath(selfCell, border);
            if (routeEdge.Count > 0)
            {
                if (debugMessages) Debug.Log("[Seeker] Going to frontier.");
                travelRoute = routeEdge;
                routeIndex = 0;
                currentState = State.Moving;
                return;
            }
        }
        blockedSpots.Add(selfCell);
        currentState = State.Idle;
        if (debugMessages) Debug.LogWarning("[Seeker] No path to guess or frontier. Marking dead end.");
    }

    Vector2Int GuessTargetCell()
    {
        if (targetHistory.Count < 2)
        {
            if (debugMessages) Debug.Log("[Seeker] Not enough data, using actual position.");
            return LocateCell(humanTarget.position);
        }
        Vector3[] record = targetHistory.ToArray();
        Vector3 older = record[record.Length - 2];
        Vector3 newer = record[record.Length - 1];
        Vector3 trend = newer - older;
        if (trend.magnitude < 0.01f)
        {
            if (debugMessages) Debug.Log("[Seeker] Target not moving, using actual cell.");
            return LocateCell(humanTarget.position);
        }
        Vector3 guide = trend.normalized;
        Vector3 guessPos = newer + guide;
        Vector2Int guessCell = LocateCell(guessPos);
        if (knownPlaces.ContainsKey(guessCell) && knownPlaces[guessCell] == PlaceType.Blocked)
        {
            if (debugMessages) Debug.Log("[Seeker] Next guess is blocked, trying left/right.");
            Vector3 leftDir  = Quaternion.Euler(0, -90, 0) * guide;
            Vector3 rightDir = Quaternion.Euler(0,  90, 0) * guide;
            Vector2Int leftCell  = LocateCell(newer + leftDir);
            Vector2Int rightCell = LocateCell(newer + rightDir);
            bool leftBlocked  = knownPlaces.ContainsKey(leftCell)  && knownPlaces[leftCell]  == PlaceType.Blocked;
            bool rightBlocked = knownPlaces.ContainsKey(rightCell) && knownPlaces[rightCell] == PlaceType.Blocked;
            if (!leftBlocked && rightBlocked) guessCell = leftCell;
            else if (leftBlocked && !rightBlocked) guessCell = rightCell;
            else if (!leftBlocked && !rightBlocked)
            {
                guessCell = (Random.value < 0.5f) ? leftCell : rightCell;
            }
            else guessCell = LocateCell(humanTarget.position);
        }
        else
        {
            if (debugMessages) Debug.Log("[Seeker] Guessing cell: " + guessCell);
        }
        return guessCell;
    }

    Vector2Int NearestEdge(Vector2Int here)
    {
        List<Vector2Int> edges = new List<Vector2Int>();
        foreach (var kvp in knownPlaces)
        {
            if (kvp.Value == PlaceType.Clear && !blockedSpots.Contains(kvp.Key))
            {
                if (HasUnknownNeighbor(kvp.Key)) edges.Add(kvp.Key);
            }
        }
        if (edges.Count == 0)
        {
            if (debugMessages) Debug.Log("[Seeker] No frontier found.");
            return new Vector2Int(-1,-1);
        }
        Queue<Vector2Int> pool = new Queue<Vector2Int>();
        HashSet<Vector2Int> seen = new HashSet<Vector2Int>();
        pool.Enqueue(here);
        seen.Add(here);
        while (pool.Count > 0)
        {
            var current = pool.Dequeue();
            if (edges.Contains(current)) return current;
            foreach (var n in Neighbors(current))
            {
                if (!seen.Contains(n) &&
                    knownPlaces.ContainsKey(n) &&
                    knownPlaces[n] == PlaceType.Clear &&
                    !blockedSpots.Contains(n))
                {
                    seen.Add(n);
                    pool.Enqueue(n);
                }
            }
        }
        return new Vector2Int(-1,-1);
    }

    bool HasUnknownNeighbor(Vector2Int spot)
    {
        foreach (var n in Neighbors(spot))
        {
            if (!knownPlaces.ContainsKey(n)) return true;
        }
        return false;
    }

    void FollowRoute()
    {
        if (travelRoute == null || travelRoute.Count == 0)
        {
            currentState = State.Idle;
            return;
        }
        if (routeIndex >= travelRoute.Count)
        {
            currentState = State.Idle;
            return;
        }
        Vector2Int step = travelRoute[routeIndex];
        Vector3 aimPos = new Vector3(step.x, transform.position.y, step.y);
        Vector3 shift = aimPos - transform.position;
        float distance = shift.magnitude;
        if (distance < 0.2f)
        {
            routeIndex++;
            return;
        }
        MoveAhead(shift.normalized);
    }

    void MoveAhead(Vector3 direction)
    {
        Vector3 velo = direction * walkSpeed;
        velo.y += fallForce * Time.deltaTime;
        mover.Move(velo * Time.deltaTime);
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }

    bool BlockedPath(Vector3 start, Vector3 end)
    {
        Vector3 route = (end - start).normalized;
        float size = Vector3.Distance(start, end);
        if (Physics.Raycast(start, route, out RaycastHit impact, size))
        {
            if (impact.collider.CompareTag("Wall")) return true;
        }
        return false;
    }

    Vector2Int LocateCell(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int z = Mathf.RoundToInt(pos.z);
        return new Vector2Int(x, z);
    }

    List<Vector2Int> Neighbors(Vector2Int spot)
    {
        List<Vector2Int> around = new List<Vector2Int>();
        around.Add(spot + new Vector2Int( 1,  0));
        around.Add(spot + new Vector2Int(-1,  0));
        around.Add(spot + new Vector2Int( 0,  1));
        around.Add(spot + new Vector2Int( 0, -1));
        return around;
    }

    public List<Vector2Int> AStarPath(Vector2Int begin, Vector2Int end)
    {
        if (!knownPlaces.ContainsKey(begin) || knownPlaces[begin] == PlaceType.Blocked || blockedSpots.Contains(begin))
        {
            if (debugMessages) Debug.Log("[A*] Begin invalid.");
            return new List<Vector2Int>();
        }
        if (!knownPlaces.ContainsKey(end) || knownPlaces[end] == PlaceType.Blocked || blockedSpots.Contains(end))
        {
            if (debugMessages) Debug.Log("[A*] End invalid.");
            return new List<Vector2Int>();
        }
        PrioritySet<Vector2Int> openPaths = new PrioritySet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> routeBack = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> costG = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> costF = new Dictionary<Vector2Int, float>();

        costG[begin] = 0f;
        costF[begin] = Heuristic(begin, end);
        openPaths.Enqueue(begin, costF[begin]);

        while(openPaths.Count > 0)
        {
            Vector2Int current = openPaths.Dequeue();
            if (current == end) return MakeRoute(routeBack, current);
            foreach (var n in FourDirections(current))
            {
                if (!knownPlaces.ContainsKey(n)) continue;
                if (knownPlaces[n] == PlaceType.Blocked) continue;
                if (blockedSpots.Contains(n)) continue;
                float gNext = costG[current] + 1f;
                if (!costG.ContainsKey(n) || gNext < costG[n])
                {
                    routeBack[n] = current;
                    costG[n] = gNext;
                    float sum = gNext + Heuristic(n, end);
                    costF[n] = sum;
                    if (!openPaths.Contains(n)) openPaths.Enqueue(n, sum);
                    else openPaths.Update(n, sum);
                }
            }
        }
        if (debugMessages) Debug.Log("[A*] No route found.");
        return new List<Vector2Int>();
    }

    float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2Int> MakeRoute(Dictionary<Vector2Int, Vector2Int> routeBack, Vector2Int node)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(node);
        while (routeBack.ContainsKey(node) && routeBack[node] != node)
        {
            node = routeBack[node];
            path.Add(node);
        }
        path.Reverse();
        return path;
    }

    List<Vector2Int> FourDirections(Vector2Int spot)
    {
        List<Vector2Int> around = new List<Vector2Int>();
        around.Add(spot + new Vector2Int( 1,  0));
        around.Add(spot + new Vector2Int(-1,  0));
        around.Add(spot + new Vector2Int( 0,  1));
        around.Add(spot + new Vector2Int( 0, -1));
        return around;
    }
}

public class PrioritySet<T>
{
    private List<(T item, float order)> data = new List<(T, float)>();

    public int Count => data.Count;

    public void Enqueue(T item, float priority)
    {
        data.Add((item, priority));
    }

    public T Dequeue()
    {
        int topIndex = 0;
        float best = data[0].order;
        for(int i=1; i<data.Count; i++)
        {
            if (data[i].order < best)
            {
                best = data[i].order;
                topIndex = i;
            }
        }
        T chosen = data[topIndex].item;
        data.RemoveAt(topIndex);
        return chosen;
    }

    public bool Contains(T item)
    {
        for(int i=0; i<data.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(data[i].item, item)) return true;
        }
        return false;
    }

    public void Update(T item, float newPriority)
    {
        for(int i=0; i<data.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(data[i].item, item))
            {
                data[i] = (item, newPriority);
                return;
            }
        }
        Enqueue(item, newPriority);
    }
}
