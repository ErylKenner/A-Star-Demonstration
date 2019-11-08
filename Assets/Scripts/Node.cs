using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class Node : MonoBehaviour
{
    public Color emptyColor = new Color32(255, 200, 200, 255);
    public Color occupiedColor = new Color32(255, 0, 0, 255);
    public Color pathColor = new Color32(0, 255, 0, 255);
    public Color exploredColor = new Color32(0, 0, 255, 255);

    public int ID;

    bool isPath;
    bool isExplored;
    int intersectCount = 0;

    float cost;
    public float Cost {
        get { return cost; }
        set {
            cost = value;
            if (cost < 0.0f)
            {
                cost = 0.0f;
            }
            else if (cost > 100.0f)
            {
                cost = 100.0f;
            }
            GetComponent<Renderer>().material.color = Color.Lerp(emptyColor, occupiedColor, cost / 101.0f);
        }
    }


    void Awake()
    {
        Cost = 0.0f;
    }


    public void SetExplored(bool explored)
    {
        if (explored && IsOccupied())
        {
            return;
        }
        isExplored = explored;
        if (isExplored)
        {
            float t = cost / 101.0f;
            exploredColor.b = Mathf.Lerp(0.8f, 0.0f, Mathf.Sqrt(t));
            GetComponent<Renderer>().material.color = exploredColor;
        }
        else if (IsOccupied())
        {
            SetOccupied();
        }
        else if (isPath)
        {
            SetPath(true);
        }
        else
        {
            Cost = cost;
        }
    }


    public void SetOccupied()
    {
        Cost = 101.0f; //Set color correctly
        cost = 101.0f; //Set cost value correctly
    }


    public void SetPath(bool path)
    {
        if (path && IsOccupied())
        {
            return;
        }
        isPath = path;
        if (isPath)
        {
            GetComponent<Renderer>().material.color = pathColor;
        }
        else if (IsOccupied())
        {
            SetOccupied();
        }
        else if (isExplored)
        {
            SetExplored(true);
        }
        else
        {
            Cost = cost;
        }
    }


    public bool IsOccupied()
    {
        return cost > 100.0f;
    }


    public bool IsPath()
    {
        return isPath;
    }


    private void OnTriggerEnter(Collider collided)
    {

        if (collided.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            intersectCount++;
            if (intersectCount > 0)
            {
                SetOccupied();
            }
            else
            {
                Cost = 0.0f;
            }
        }
    }


    private void OnTriggerExit(Collider collided)
    {
        if (collided.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            intersectCount--;
            if (intersectCount > 0)
            {
                SetOccupied();
            }
            else
            {
                Cost = 0.0f;
            }
        }

    }
}
