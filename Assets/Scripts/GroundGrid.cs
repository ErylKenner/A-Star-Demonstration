using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GroundGrid : MonoBehaviour
{
    public Node NodePrefab;
    public GameObject StartSprite;
    public GameObject EndSprite;
    public Node startNode;
    public Node endNode;
    public ObstacleManager obstacleManager;
    public float[,] AdjacencyMatrix { get => adjacencyMatrix; }
    public int Rows { get => rows; }
    public int Columns { get => columns; }

    private int columns, rows;
    private List<Node> nodes;
    private float[,] adjacencyMatrix;
    private int numNeighbors = 8;


    private void Awake()
    {
        nodes = new List<Node>();
    }


    public void CreateGrid(int _rows, int _columns)
    {
        if (_rows == rows || _columns == columns)
        {
            return;
        }
        columns = _columns;
        rows = _rows;
        deleteGrid();
        adjacencyMatrix = new float[rows * columns, numNeighbors];
        for (int i = 0; i < rows * columns; ++i)
        {
            for (int j = 0; j < numNeighbors; ++j)
            {
                adjacencyMatrix[i, j] = Mathf.Infinity;
            }
        }

        //Create new nodes
        float widthPerNode = transform.localScale.x / columns;
        float lengthPerNode = transform.localScale.y / rows;
        for (int row = 0; row < rows; ++row)
        {
            for (int col = 0; col < columns; ++col)
            {
                float xPos = (col - 0.5f * (columns - 1.0f)) * widthPerNode;
                float yPos = (row - 0.5f * (rows - 1.0f)) * lengthPerNode;
                Node cur = Instantiate(NodePrefab, transform.position + new Vector3(xPos, 0.0f, yPos), Quaternion.Euler(90.0f, 0.0f, 0.0f), this.transform);
                cur.transform.localScale = new Vector3(1.0f / columns, 1.0f / rows, 1.0f);
                cur.ID = row * columns + col;
                nodes.Add(cur);
            }
        }
    }


    public void UpdateObstacleCollisions(bool useZones)
    {
        for (int i = 0; i < nodes.Count; ++i)
        {
            if (!nodes.ElementAt(i).IsOccupied())
            {
                nodes.ElementAt(i).Cost = 0.0f;
                if (useZones)
                {
                    foreach (Obstacle obstacle in obstacleManager.Obstacles)
                    {
                        nodes.ElementAt(i).Cost += obstacle.GetCost(nodes.ElementAt(i).transform.position);
                    }
                }
            }
            markAdjacencymatrix(i);
        }
    }


    public void SetNodeExplored(int index, bool explored)
    {
        nodes.ElementAt(index).SetExplored(explored);
    }

    public bool NodeIsOccupied(int nodeIndex)
    {
        return nodes.ElementAt(nodeIndex).IsOccupied();
    }


    public void HidePath()
    {
        for (int i = 0; i < nodes.Count; ++i)
        {
            if (!nodes.ElementAt(i).IsOccupied())
            {
                nodes.ElementAt(i).SetPath(false);
                nodes.ElementAt(i).SetExplored(false);
            }
        }
    }


    public int GetStartNodeIndex()
    {
        return nodes.IndexOf(startNode);
    }


    public int GetEndNodeIndex()
    {
        return nodes.IndexOf(endNode);
    }


    public Vector3 GetNodePosition(int nodeIndex)
    {
        return nodes.ElementAt(nodeIndex).transform.position;
    }


    public int ConvertNeighborIndexToNodeIndex(int cur, int neighborIndex)
    {
        int curRow = cur / columns;
        int curCol = cur % columns;
        switch (neighborIndex)
        {
            case 0:
                {
                    if (curRow == rows - 1 || curCol == 0)
                    {
                        return -1;
                    }
                    int neighborRow = curRow + 1;
                    int neighborColumn = curCol - 1;
                    return neighborRow * columns + neighborColumn;
                }
            case 1:
                {
                    if (curRow == rows - 1)
                    {
                        return -1;
                    }
                    int neighborRow = curRow + 1;
                    int neighborColumn = curCol;
                    return neighborRow * columns + neighborColumn;
                }
            case 2:
                {
                    if (curRow == rows - 1 || curCol == columns - 1)
                    {
                        return -1;
                    }
                    int neighborRow = curRow + 1;
                    int neighborColumn = curCol + 1;
                    return neighborRow * columns + neighborColumn;
                }
            case 3:
                {
                    if (curCol == 0)
                    {
                        return -1;
                    }
                    int neighborRow = curRow;
                    int neighborColumn = curCol - 1;
                    return neighborRow * columns + neighborColumn;
                }
            case 4:
                {
                    if (curCol == columns - 1)
                    {
                        return -1;
                    }
                    int neighborRow = curRow;
                    int neighborColumn = curCol + 1;
                    return neighborRow * columns + neighborColumn;
                }
            case 5:
                {
                    if (curRow == 0 || curCol == 0)
                    {
                        return -1;
                    }
                    int neighborRow = curRow - 1;
                    int neighborColumn = curCol - 1;
                    return neighborRow * columns + neighborColumn;
                }
            case 6:
                {
                    if (curRow == 0)
                    {
                        return -1;
                    }
                    int neighborRow = curRow - 1;
                    int neighborColumn = curCol;
                    return neighborRow * columns + neighborColumn;
                }
            case 7:
                {
                    if (curRow == 0 || curCol == columns - 1)
                    {
                        return -1;
                    }
                    int neighborRow = curRow - 1;
                    int neighborColumn = curCol + 1;
                    return neighborRow * columns + neighborColumn;
                }
            default:
                {
                    return -1;
                }
        }
    }


    public void DisplayPath(List<int> path)
    {
        for (int i = 0; i < path.Count; ++i)
        {
            nodes.ElementAt(path.ElementAt(i)).SetPath(true);
        }
    }


    public float Heuristic(int cur, int endNode)
    {
        const float sqrt2_minus_1 = 0.41421356237f;
        float diffX = Mathf.Abs(nodes.ElementAt(cur).transform.position.x - nodes.ElementAt(endNode).transform.position.x);
        float diffZ = Mathf.Abs(nodes.ElementAt(cur).transform.position.z - nodes.ElementAt(endNode).transform.position.z);
        return Mathf.Min(diffX, diffZ) * sqrt2_minus_1 + Mathf.Max(diffX, diffZ);

        Vector3 diff = nodes.ElementAt(cur).transform.position - nodes.ElementAt(endNode).transform.position;
        return Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z);
        //return Mathf.Abs(cur.xPos - endNode.xPos) + Mathf.Abs(cur.yPos - endNode.yPos);
    }


    public void ResetPath()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "Line")
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        startNode = endNode = null;
        StartSprite.transform.position = new Vector3(10000.0f, StartSprite.transform.position.y, StartSprite.transform.position.z);
        EndSprite.transform.position = new Vector3(10000.0f, EndSprite.transform.position.y, EndSprite.transform.position.z);
        HidePath();
    }


    void markAdjacencymatrix(int cur)
    {
        int neighbor;
        /*
         * Each row of the adjacency matrix represents all the nodes that connect to that row's node.
         * So, we mark this node as a neighbor in each of our neighbor's rows
         * */
        int row = cur / columns;
        int column = cur % columns;
        const float movementCost = 10.0f;

        float cost = Mathf.Infinity;
        if (!nodes.ElementAt(cur).IsOccupied())
        {
            cost = nodes.ElementAt(cur).Cost;
        }

        int count = 0;

        //Mark top left diagonal
        neighbor = cur - 1 + columns;
        if (column > 0 && row < rows - 1 && !nodes.ElementAt(neighbor).IsOccupied())
        {
            adjacencyMatrix[cur, count] = cost + Mathf.Sqrt(2) * movementCost;
        }
        count++;

        //Mark above
        neighbor = cur + columns;
        if (row < rows - 1 && !nodes.ElementAt(neighbor).IsOccupied())
        {
            adjacencyMatrix[cur, count] = cost + movementCost;
        }
        count++;

        //Mark top right diagonal
        neighbor = cur + 1 + columns;
        if (column < columns - 1 && row < rows - 1 && !nodes.ElementAt(neighbor).IsOccupied())
        {
            adjacencyMatrix[cur, count] = cost + Mathf.Sqrt(2) * movementCost;
        }
        count++;

        //Mark to the left
        neighbor = cur - 1;
        if (column > 0 && !nodes.ElementAt(neighbor).IsOccupied())
        {
            adjacencyMatrix[cur, count] = cost + movementCost;
        }
        count++;

        //Mark to the right
        neighbor = cur + 1;
        if (column < columns - 1 && !nodes.ElementAt(neighbor).IsOccupied())
        {
            adjacencyMatrix[cur, count] = cost + movementCost;
        }
        count++;

        //Mark bottom left diagonal
        neighbor = cur - 1 - columns;
        if (column > 0 && row > 0 && !nodes.ElementAt(neighbor).IsOccupied())
        {
            adjacencyMatrix[cur, count] = cost + Mathf.Sqrt(2) * movementCost;
        }
        count++;

        //Mark below
        neighbor = cur - columns;
        if (row > 0 && !nodes.ElementAt(neighbor).IsOccupied())
        {
            adjacencyMatrix[cur, count] = cost + movementCost;
        }
        count++;

        //Mark bottom right diagonal
        neighbor = cur + 1 - columns;
        if (column < columns - 1 && row > 0 && !nodes.ElementAt(neighbor).IsOccupied())
        {
            adjacencyMatrix[cur, count] = cost + Mathf.Sqrt(2) * movementCost;
        }
    }


    void deleteGrid()
    {
        ResetPath();
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        nodes.Clear();
    }




}
