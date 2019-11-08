using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathPlanner
{
    public static IEnumerator A_Star(int startNode, int endNode, GroundGrid groundGrid)
    {
        int numNodes = groundGrid.AdjacencyMatrix.GetLength(0);
        int numNeighbors = groundGrid.AdjacencyMatrix.GetLength(1);
        //Debug.Log("num nodes: " + numNodes);
        //Debug.Log("num neighbors: " + numNeighbors);

        List<int> toVisit = new List<int>();
        List<int> visited = new List<int>();
        int[] parent = new int[numNodes];
        float[] gCost = new float[numNodes];
        float[] fCost = new float[numNodes];
        for (int i = 0; i < numNodes; ++i)
        {
            parent[i] = -1;
            fCost[i] = gCost[i] = float.PositiveInfinity;
        }

        toVisit.Add(startNode);
        gCost[startNode] = 0.0f;
        fCost[startNode] = groundGrid.Heuristic(startNode, endNode);

        int count = 0;
        while (toVisit.Any())
        {
            int cur = A_Star_getLowestCost(toVisit, fCost);
            if (cur == endNode)
            {
                groundGrid.DisplayPath(A_Star_createPathFromParent(endNode, parent));
                yield break;
            }


            toVisit.Remove(cur);
            visited.Add(cur);
            groundGrid.SetNodeExplored(cur, true);

            for (int i = 0; i < numNeighbors; ++i)
            {
                int neighbor = groundGrid.ConvertNeighborIndexToNodeIndex(cur, i);
                if (groundGrid.AdjacencyMatrix[cur, i] == Mathf.Infinity || visited.Contains(neighbor) || neighbor == cur || neighbor == -1)
                {
                    continue;
                }

                float gScore = gCost[cur] + groundGrid.AdjacencyMatrix[cur, i];
                if (gScore < gCost[neighbor])
                {
                    parent[neighbor] = cur;
                    gCost[neighbor] = gScore;
                    fCost[neighbor] = gCost[neighbor] + groundGrid.Heuristic(neighbor, endNode);
                    if (!toVisit.Contains(neighbor))
                    {
                        toVisit.Add(neighbor);
                    }
                }
            }
            count++;
            const int nodesExploredPerFrame = 50;
            if (count >= nodesExploredPerFrame)
            {
                count -= nodesExploredPerFrame;
                yield return null;
            }

        }

    }

    static List<int> A_Star_createPathFromParent(int endNode, int[] parent)
    {
        List<int> path = new List<int>();
        for (int cur = endNode; cur != -1; cur = parent[cur])
        {
            path.Insert(0, cur);
        }
        return path;
    }

    static int A_Star_getLowestCost(List<int> toVisit, float[] cost)
    {
        int bestNode = -1;
        float bestCost = float.PositiveInfinity;
        foreach (int cur in toVisit)
        {
            if (cost[cur] < bestCost)
            {
                bestCost = cost[cur];
                bestNode = cur;
            }
        }
        return bestNode;
    }


    public static IEnumerator RRT(int startNode, int endNode, GroundGrid groundGrid)
    {
        foreach (Transform child in groundGrid.transform)
        {
            if (child.name == "Line")
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        const int numPoints = 2000;
        List<int> nodes = new List<int>();
        nodes.Add(startNode);

        for (int i = 0; i < numPoints; ++i)
        {
            //Random State
            int randNode = Random.Range(0, groundGrid.Rows * groundGrid.Columns);
            if (groundGrid.NodeIsOccupied(randNode) || nodes.Contains(randNode))
            {
                i--;
                continue;
            }

            //Nearest Neighbor
            int closestNeighbor = nodes.ElementAt(0);
            float minDist = Vector3.Distance(groundGrid.GetNodePosition(closestNeighbor), groundGrid.GetNodePosition(randNode));
            for (int n = 0; n < nodes.Count; ++n)
            {
                float newDist = Vector3.Distance(groundGrid.GetNodePosition(nodes.ElementAt(n)), groundGrid.GetNodePosition(randNode));
                if (newDist < minDist)
                {
                    minDist = newDist;
                    closestNeighbor = nodes.ElementAt(n);
                }
            }

            //Select Input

            //New State
            int newNode = RRT_StepTowards(closestNeighbor, randNode, groundGrid);
            if (newNode == -1)
            {
                i--;
                continue;
            }

            nodes.Add(newNode);
            DrawLine(groundGrid.GetNodePosition(closestNeighbor), groundGrid.GetNodePosition(newNode), Color.red, groundGrid.transform, -1);
            yield return null;
        }

        yield break;
    }

    static int RRT_StepTowards(int start, int end, GroundGrid groundGrid)
    {
        const float epsilon = 1.5f;
        int ret = -1;
        float dist = Vector3.Distance(groundGrid.GetNodePosition(start), groundGrid.GetNodePosition(end));
        if (dist < epsilon)
        {
            ret = end;
            RaycastHit[] hits;
            hits = Physics.RaycastAll(groundGrid.GetNodePosition(start), groundGrid.GetNodePosition(end) - groundGrid.GetNodePosition(start), dist, 1 << LayerMask.NameToLayer("Node"));
            for (int i = 0; i < hits.Length; ++i)
            {
                Node curNode = hits[i].collider.GetComponent<Node>();
                if (curNode.IsOccupied())
                {
                    ret = -1;
                    break;
                }
            }
        }
        else
        {
            RaycastHit[] hits;
            hits = Physics.RaycastAll(groundGrid.GetNodePosition(start), groundGrid.GetNodePosition(end) - groundGrid.GetNodePosition(start), epsilon, 1 << LayerMask.NameToLayer("Node"));
            float maxDist = 0.0f;
            for (int i = 0; i < hits.Length; ++i)
            {
                RaycastHit cur = hits[i];
                Node curNode = cur.collider.GetComponent<Node>();
                if (curNode.IsOccupied())
                {
                    ret = -1;
                    break;
                }
                if(cur.distance > maxDist)
                {
                    maxDist = cur.distance;
                    ret = curNode.ID;
                }
            }
        }
        RaycastHit hit;
        if (Physics.Raycast(groundGrid.GetNodePosition(start), groundGrid.GetNodePosition(end) - groundGrid.GetNodePosition(start), out hit, epsilon, 1 << LayerMask.NameToLayer("Obstacles")))
        {
            ret = -1;
        }
        return ret;
    }


    static void DrawLine(Vector3 start, Vector3 end, Color color, Transform parent, float duration = -1.0f)
    {
        start.y += 0.1f;
        end.y += 0.1f;
        GameObject myLine = new GameObject("Line");
        myLine.transform.parent = parent;
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = color;
        lr.startWidth = lr.endWidth = 0.2f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        if (duration >= 0.0f)
        {
            GameObject.Destroy(myLine, duration);
        }
    }
}
