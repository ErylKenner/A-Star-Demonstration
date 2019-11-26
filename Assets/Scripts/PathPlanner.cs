using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathPlanner
{
    public static IEnumerator A_Star(int startNode, int endNode, GroundGrid groundGrid, List<int> outPath)
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
                foreach (int item in CreatePathFromParent(endNode, parent))
                {
                    outPath.Add(item);
                }
                groundGrid.DisplayPath(outPath);
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

    static List<int> CreatePathFromParent(int endNode, int[] parent)
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


    public static IEnumerator RRT(int startNode, int endNode, GroundGrid groundGrid, Actor actor, List<int> outPath)
    {
        outPath.Clear();
        foreach (Transform child in groundGrid.transform)
        {
            if (child.name == "Line")
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        int numNodes = groundGrid.Rows * groundGrid.Columns;
        const int numAttempts = 2000;
        int[] parent = new int[numNodes];
        List<int> possibleNodesToPick = new List<int>();
        for (int i = 0; i < numNodes; ++i)
        {
            if (!groundGrid.NodeIsOccupied(i))
            {
                possibleNodesToPick.Add(i);
            }
            parent[i] = -1;
        }
        List<int> nodes = new List<int>();
        nodes.Add(startNode);
        possibleNodesToPick.Remove(startNode);

        for (int i = 0; i < numAttempts; ++i)
        {
            if (possibleNodesToPick.Count == 0)
            {
                yield break;
            }
            int randNode = RRT_GetRandomState(endNode, possibleNodesToPick);
            int nearestNeighbor = RRT_GetNearestNeighbor(randNode, nodes, groundGrid);

            //Select input to use
            //For now any input is valid

            //Determine new state
            int newNode = -1;
            if (RRT_StepTowards(nearestNeighbor, randNode, ref newNode, groundGrid, actor))
            {
                possibleNodesToPick.Remove(newNode);
                nodes.Add(newNode);
                DrawLine(groundGrid.GetNodePosition(nearestNeighbor), groundGrid.GetNodePosition(newNode), Color.red, groundGrid.transform, -1);
                parent[newNode] = nearestNeighbor;
                if (newNode == endNode)
                {
                    foreach (int item in CreatePathFromParent(endNode, parent))
                    {
                        outPath.Add(item);
                    }
                    groundGrid.DisplayPath(outPath);
                    yield break;
                }
                groundGrid.SetNodeExplored(newNode, true);
            }
            else
            {
                continue;
            }
            yield return null;
        }

        yield break;
    }

    static int RRT_GetRandomState(int endNode, List<int> possibleNodesToPick)
    {
        int randNode;
        if (Random.value < 0.2)
        {
            randNode = endNode;
        }
        else
        {
            randNode = possibleNodesToPick.ElementAt(Random.Range(0, possibleNodesToPick.Count));
        }
        return randNode;
    }

    static bool RRT_StepTowards(int start, int end, ref int newNode, GroundGrid groundGrid, Actor actor)
    {
        //Only able to move one square (in any direction) in a single step
        float epsilon = 1.45f * 100 / groundGrid.Columns;
        float dist = Mathf.Min(epsilon, Vector3.Distance(groundGrid.GetNodePosition(start), groundGrid.GetNodePosition(end)));
        RaycastHit hit;
        if (Physics.Raycast(groundGrid.GetNodePosition(start), groundGrid.GetNodePosition(end) - groundGrid.GetNodePosition(start), out hit, dist, 1 << LayerMask.NameToLayer("Obstacles")))
        {
            //Obstacle between start and end nodes
            newNode = -1;
            return false;
        }
        RaycastHit[] hits;
        hits = Physics.RaycastAll(groundGrid.GetNodePosition(start), groundGrid.GetNodePosition(end) - groundGrid.GetNodePosition(start), dist, 1 << LayerMask.NameToLayer("Node"));
        float maxDist = 0.0f;
        for (int i = 0; i < hits.Length; ++i)
        {
            Node curNode = hits[i].collider.GetComponent<Node>();
            if (curNode.IsOccupied())
            {
                //Occupied node between start and end nodes
                newNode = -1;
                return false;
            }
            if (hits[i].distance > maxDist)
            {
                maxDist = hits[i].distance;
                newNode = curNode.ID;
            }
        }

        actor.transform.position = groundGrid.GetNodePosition(start);
        int numSteps = 5;
        for(int step = 0; step < numSteps; ++step)
        {
            actor.transform.position = Vector3.Lerp(groundGrid.GetNodePosition(start), groundGrid.GetNodePosition(newNode), (float)step / (numSteps - 1));
            Collider[] colliderHits = Physics.OverlapBox(actor.transform.position, actor.GetComponent<Collider>().bounds.extents, actor.transform.rotation, 1 << LayerMask.NameToLayer("Obstacles"));
            for(int i = 0; i < colliderHits.Length; ++i)
            {
                //Actor intersects an obstacle on its path from the start to end node
                newNode = -1;
                return false;
            }
        }
        return true;
    }

    static int RRT_GetNearestNeighbor(int randNode, List<int> nodes, GroundGrid groundGrid)
    {
        //TODO: Make sure the nearest neighbor which is selected has Line-of-Sight to the selected randNode
        int nearestNeighbor = nodes.ElementAt(0);
        float minDist = Vector3.Distance(groundGrid.GetNodePosition(nearestNeighbor), groundGrid.GetNodePosition(randNode));
        for (int n = 0; n < nodes.Count; ++n)
        {
            float newDist = Vector3.Distance(groundGrid.GetNodePosition(nodes.ElementAt(n)), groundGrid.GetNodePosition(randNode));
            if (newDist < minDist)
            {
                minDist = newDist;
                nearestNeighbor = nodes.ElementAt(n);
            }
        }
        return nearestNeighbor;
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
