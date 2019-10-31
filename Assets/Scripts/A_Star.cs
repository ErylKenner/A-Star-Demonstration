using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class A_Star
{
    public static IEnumerator CalculatePath(int startNode, int endNode, float[,] adjacencyMatrix, System.Func<int, int, float> heuristic, GroundGrid groundGrid)
    {
        int numNodes = adjacencyMatrix.GetLength(0);

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
        fCost[startNode] = heuristic(startNode, endNode);

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

            for (int neighbor = 0; neighbor < numNodes; ++neighbor)
            {
                if (adjacencyMatrix[cur, neighbor] == Mathf.Infinity || visited.Contains(neighbor) || neighbor == cur)
                {
                    continue;
                }

                float gScore = gCost[cur] + adjacencyMatrix[cur, neighbor];
                if (gScore < gCost[neighbor])
                {
                    parent[neighbor] = cur;
                    gCost[neighbor] = gScore;
                    fCost[neighbor] = gCost[neighbor] + heuristic(neighbor, endNode);
                    if (!toVisit.Contains(neighbor))
                    {
                        toVisit.Add(neighbor);
                    }
                }
            }
            count++;
            if(count >= 500)
            {
                count -= 500;
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
