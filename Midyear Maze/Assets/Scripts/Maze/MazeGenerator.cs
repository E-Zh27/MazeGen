using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    public GameObject aiPrefab;
    public GameObject exitPrefab;

    public Cell[,] grid;
    public int width = 25;
    public int height = 25;

    private List<Vector2Int> wallList;

    public Vector2Int playerStartPos;
    public Vector2Int aiStartPos;

    void Start()
    {
        InitializeGrid();
        GenerateMaze();
        DrawMaze();

        playerStartPos = PickRandomFloorCell();
        do
        {
            aiStartPos = PickRandomFloorCell();
        }
        while (aiStartPos == playerStartPos);

        SpawnEntities(playerStartPos, aiStartPos);
    }

    void InitializeGrid()
    {
        grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Cell();
            }
        }
    }

    void GenerateMaze()
    {
        wallList = new List<Vector2Int>();

        int startX = Random.Range(1, width - 1);
        int startY = Random.Range(1, height - 1);

        startX = (startX % 2 == 0) ? startX - 1 : startX;
        startY = (startY % 2 == 0) ? startY - 1 : startY;

        grid[startX, startY].IsVisited = true;
        grid[startX, startY].IsWall = false;

        AddWallsToList(startX, startY);

        while (wallList.Count > 0)
        {
            int randomIndex = Random.Range(0, wallList.Count);
            Vector2Int wallPos = wallList[randomIndex];
            wallList.RemoveAt(randomIndex);

            ProcessWall(wallPos);
        }
    }

    void AddWallsToList(int x, int y)
    {
        if (x - 2 > 0) wallList.Add(new Vector2Int(x - 1, y));
        if (x + 2 < width - 1) wallList.Add(new Vector2Int(x + 1, y));
        if (y - 2 > 0) wallList.Add(new Vector2Int(x, y - 1));
        if (y + 2 < height - 1) wallList.Add(new Vector2Int(x, y + 1));
    }

    void ProcessWall(Vector2Int wall)
    {
        int x = wall.x;
        int y = wall.y;

        List<Vector2Int> neighbors = GetNeighbors(x, y);

        if (neighbors.Count == 2)
        {
            Cell cell1 = grid[neighbors[0].x, neighbors[0].y];
            Cell cell2 = grid[neighbors[1].x, neighbors[1].y];

            if (cell1.IsVisited != cell2.IsVisited)
            {
                grid[x, y].IsWall = false;

                if (!cell1.IsVisited)
                {
                    cell1.IsVisited = true;
                    cell1.IsWall = false;
                    AddWallsToList(neighbors[0].x, neighbors[0].y);
                }
                else
                {
                    cell2.IsVisited = true;
                    cell2.IsWall = false;
                    AddWallsToList(neighbors[1].x, neighbors[1].y);
                }
            }
        }
    }

    List<Vector2Int> GetNeighbors(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (x % 2 == 1)
        {
            if (y - 1 >= 0) neighbors.Add(new Vector2Int(x, y - 1));
            if (y + 1 < height) neighbors.Add(new Vector2Int(x, y + 1));
        }
        else if (y % 2 == 1)
        {
            if (x - 1 >= 0) neighbors.Add(new Vector2Int(x - 1, y));
            if (x + 1 < width) neighbors.Add(new Vector2Int(x + 1, y));
        }

        return neighbors;
    }

    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x, 0, y);

                if (grid[x, y].IsWall)
                {
                    // Instantiate the wall and tag it as "Wall"
                    GameObject wallObj = Instantiate(wallPrefab, position, Quaternion.identity, transform);
                    wallObj.tag = "Wall";  // <-- auto-tag the wall
                }
                else
                {
                    Instantiate(floorPrefab, position, Quaternion.identity, transform);
                }
            }
        }
    }

    public Vector2Int PickRandomFloorCell()
    {
        while (true)
        {
            int rx = Random.Range(1, width - 1);
            int ry = Random.Range(1, height - 1);

            if (!grid[rx, ry].IsWall)
            {
                return new Vector2Int(rx, ry);
            }
        }
    }

    private Vector2Int FindFurthestCellFrom(Vector2Int startCell)
    {
        int[,] distance = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                distance[x, y] = -1;
            }
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(startCell);
        distance[startCell.x, startCell.y] = 0;

        Vector2Int furthestCell = startCell;
        int maxDist = 0;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int curDist = distance[current.x, current.y];

            if (curDist > maxDist)
            {
                maxDist = curDist;
                furthestCell = current;
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                if (!grid[nx, ny].IsWall && distance[nx, ny] == -1)
                {
                    distance[nx, ny] = curDist + 1;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        return furthestCell;
    }

    void SpawnEntities(Vector2Int playerPos, Vector2Int aiPos)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
    {
        Vector3 pPos = new Vector3(playerPos.x, 1f, playerPos.y);
        playerObj.transform.position = pPos;
    }
        Vector3 aPos = new Vector3(aiPos.x, 1f, aiPos.y);
        GameObject aiObj = Instantiate(aiPrefab, aPos, Quaternion.identity);
        aiObj.name = "AI";

        Vector2Int exitCell = FindFurthestCellFrom(playerPos);
        Vector3 exitPos = new Vector3(exitCell.x, 1f, exitCell.y);
        GameObject exitObj = Instantiate(exitPrefab, exitPos, Quaternion.identity);
        exitObj.name = "Exit";
    }
}
