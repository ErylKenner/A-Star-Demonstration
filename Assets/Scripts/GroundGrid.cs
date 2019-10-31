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

    private int Columns, Rows;
    private List<Node> nodes;
    private float[,] adjacencyMatrix;


    private void Awake()
    {
        nodes = new List<Node>();
    }


    public void CreateGrid(int rows, int columns)
    {
        if (rows == Rows || columns == Columns)
        {
            return;
        }
        Columns = columns;
        Rows = rows;
        DeleteGrid();
        adjacencyMatrix = new float[Rows * Columns, Rows * Columns];
        for (int i = 0; i < Rows * Columns; ++i)
        {
            for (int j = 0; j < Rows * Columns; ++j)
            {
                adjacencyMatrix[i, j] = Mathf.Infinity;
            }
        }

        //Create new nodes
        float widthPerNode = transform.localScale.x / Columns;
        float lengthPerNode = transform.localScale.y / Rows;
        for (int row = 0; row < Rows; ++row)
        {
            for (int col = 0; col < Columns; ++col)
            {
                float xPos = (col - 0.5f * (Columns - 1.0f)) * widthPerNode;
                float yPos = (row - 0.5f * (Rows - 1.0f)) * lengthPerNode;
                Node cur = Instantiate(NodePrefab, transform.position + new Vector3(xPos, 0.0f, yPos), Quaternion.Euler(90.0f, 0.0f, 0.0f), this.transform);
                cur.transform.localScale = new Vector3(1.0f / Columns, 1.0f / Rows, 1.0f);
                nodes.Add(cur);
                int curIndex = col + row * Columns;
                adjacencyMatrix[curIndex, curIndex] = 1.0f;
            }
        }
    }


    public void UpdateObstacleCollisions()
    {
        for (int i = 0; i < nodes.Count; ++i)
        {
            foreach (GameObject obstacle in obstacleManager.Obstacles)
            {
                //Add to node value based on distance to obstacle
            }
            markAdjacencymatrix(i);
        }
    }


    public void HidePath()
    {
        for (int i = 0; i < nodes.Count; ++i)
        {
            if (!nodes.ElementAt(i).IsOccupied())
            {
                nodes.ElementAt(i).SetPath(false);
            }
        }
    }


    public int GetStart()
    {
        return nodes.IndexOf(startNode);
    }


    public int GetEnd()
    {
        return nodes.IndexOf(endNode);
    }


    public void ColorPath(List<int> path)
    {
        for (int i = 0; i < path.Count; ++i)
        {
            nodes.ElementAt(path.ElementAt(i)).SetPath(true);
        }
    }


    void markAdjacencymatrix(int cur)
    {
        if (nodes.ElementAt(cur).IsOccupied())
        {
            for (int n = 0; n < nodes.Count; ++n)
            {
                adjacencyMatrix[n, cur] = Mathf.Infinity;
            }
        }
        else
        {
            int row = cur / Columns;
            int column = cur % Columns;
            int neighbor;
            /*
             * Each row of the adjacency matrix represents all the nodes that connect to that row's node.
             * So, we mark this node as a neighbor in each of our neighbor's rows
             * */

            //Mark the current square
            adjacencyMatrix[cur, cur] = 1.0f;

            //Mark to the left
            neighbor = cur - 1;
            if (column > 0 && !nodes.ElementAt(neighbor).IsOccupied())
            {
                adjacencyMatrix[neighbor, cur] = 1.0f;
            }

            //Mark to the right
            neighbor = cur + 1;
            if (column < Columns - 1 && !nodes.ElementAt(neighbor).IsOccupied())
            {
                adjacencyMatrix[neighbor, cur] = 1.0f;
            }

            //Mark below
            neighbor = cur - Columns;
            if (row > 0 && !nodes.ElementAt(neighbor).IsOccupied())
            {
                adjacencyMatrix[neighbor, cur] = 1.0f;
            }

            //Mark above
            neighbor = cur + Columns;
            if (row < Rows - 1 && !nodes.ElementAt(neighbor).IsOccupied())
            {
                adjacencyMatrix[neighbor, cur] = 1.0f;
            }

            //Mark bottom left diagonal
            neighbor = cur - 1 - Columns;
            if (column > 0 && row > 0 && !nodes.ElementAt(neighbor).IsOccupied())
            {
                adjacencyMatrix[neighbor, cur] = 1.0f;
            }

            //Mark bottom right diagonal
            neighbor = cur + 1 - Columns;
            if (column < Columns - 1 && row > 0 && !nodes.ElementAt(neighbor).IsOccupied())
            {
                adjacencyMatrix[neighbor, cur] = 1.0f;
            }

            //Mark top left diagonal
            neighbor = cur - 1 + Columns;
            if (column > 0 && row < Rows - 1 && !nodes.ElementAt(neighbor).IsOccupied())
            {
                adjacencyMatrix[neighbor, cur] = 1.0f;
            }

            //Mark top right diagonal
            neighbor = cur + 1 + Columns;
            if (column < Columns - 1 && row < Rows - 1 && !nodes.ElementAt(neighbor).IsOccupied())
            {
                adjacencyMatrix[neighbor, cur] = 1.0f;
            }
        }
    }


    public float heuristic(int cur, int endNode)
    {
        Vector3 diff = nodes.ElementAt(cur).transform.position - nodes.ElementAt(endNode).transform.position;
        return Mathf.Abs(diff.x) + Mathf.Abs(diff.y) + Mathf.Abs(diff.z);
        //return Mathf.Abs(cur.xPos - endNode.xPos) + Mathf.Abs(cur.yPos - endNode.yPos);
    }


    public void ResetPath()
    {
        startNode = endNode = null;
        StartSprite.transform.position = new Vector3(10000.0f, StartSprite.transform.position.y, StartSprite.transform.position.z);
        EndSprite.transform.position = new Vector3(10000.0f, EndSprite.transform.position.y, EndSprite.transform.position.z);
        HidePath();
    }


    void DeleteGrid()
    {
        ResetPath();
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        nodes.Clear();
    }




}
