using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class A_Star
{
    public static IEnumerator CalculatePath(int startNode, int endNode, GroundGrid groundGrid)
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
            int cur = getLowestCost(toVisit, fCost);
            if (cur == endNode)
            {
                groundGrid.DisplayPath(createPathFromParent(endNode, parent));
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

    static List<int> createPathFromParent(int endNode, int[] parent)
    {
        List<int> path = new List<int>();
        for (int cur = endNode; cur != -1; cur = parent[cur])
        {
            path.Insert(0, cur);
        }
        return path;
    }

    static int getLowestCost(List<int> toVisit, float[] cost)
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

}
