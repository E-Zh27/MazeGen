// Pathfinding.cs
using UnityEngine;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    private Node[,] nodes;
    private MazeGenerator mazeGenerator;
    private int width;
    private int height;


    void Awake()
    {
        mazeGenerator = FindObjectOfType<MazeGenerator>();
    }

    public List<Node> FindPath(Vector2Int startCell, Vector2Int endCell)
    {
        if (nodes == null || nodes.GetLength(0) != mazeGenerator.width || nodes.GetLength(1) != mazeGenerator.height)
        {
            InitializeNodes();
        }

        if (!IsInBounds(startCell) || !IsInBounds(endCell)) return new List<Node>();

        Node startNode = nodes[startCell.x, startCell.y];
        Node endNode = nodes[endCell.x, endCell.y];

        if (!startNode.IsWalkable || !endNode.IsWalkable)
        {
            return new List<Node>();
        }

        List<Node> openList = new List<Node> { startNode };
        HashSet<Node> closedList = new HashSet<Node>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nodes[x, y].GCost = int.MaxValue;
                nodes[x, y].ParentNode = null;
            }
        }
        startNode.GCost = 0;
        startNode.HCost = GetManhattanDistance(startNode, endNode);

        while (openList.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                return RetracePath(startNode, endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.IsWalkable || closedList.Contains(neighbor))
                    continue;

                int tentativeGCost = currentNode.GCost + 1; 

                if (tentativeGCost < neighbor.GCost || !openList.Contains(neighbor))
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetManhattanDistance(neighbor, endNode);
                    neighbor.ParentNode = currentNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
        return new List<Node>();
    }

    private void InitializeNodes()
    {
        width = mazeGenerator.width;
        height = mazeGenerator.height;
        nodes = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isWalkable = !mazeGenerator.grid[x, y].IsWall;
                nodes[x, y] = new Node(new Vector2Int(x, y), isWalkable);
            }
        }
    }

    private bool IsInBounds(Vector2Int cell)
    {
        return (cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height);
    }

    private int GetManhattanDistance(Node a, Node b)
    {
        return Mathf.Abs(a.GridPosition.x - b.GridPosition.x) + Mathf.Abs(a.GridPosition.y - b.GridPosition.y);
    }

    private Node GetLowestFCostNode(List<Node> nodeList)
    {
        Node lowestFCostNode = nodeList[0];

        foreach (Node node in nodeList)
        {
            if (node.FCost < lowestFCostNode.FCost ||
                (node.FCost == lowestFCostNode.FCost && node.HCost < lowestFCostNode.HCost))
            {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        int x = node.GridPosition.x;
        int y = node.GridPosition.y;

        if (x - 1 >= 0) neighbors.Add(nodes[x - 1, y]);
        if (x + 1 < width) neighbors.Add(nodes[x + 1, y]);
        if (y - 1 >= 0) neighbors.Add(nodes[x, y - 1]);
        if (y + 1 < height) neighbors.Add(nodes[x, y + 1]);

        return neighbors;
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.ParentNode;
        }
        path.Add(startNode);
        path.Reverse();
        return path;
    }
}
