﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class Node : MonoBehaviour
{
    Color emptyColor = new Color32(173, 173, 173, 255);
    Color occupiedColor = new Color32(255, 0, 0, 255);
    Color pathColor = new Color32(0, 255, 0, 255);

    bool isPath;
    int intersectCount = 0;

    float cost;
    public float Cost {
        get { return cost; }
        set {
            cost = Mathf.Clamp(value, 0.0f, 100.0f);
            GetComponent<Renderer>().material.color = Color.Lerp(emptyColor, occupiedColor, cost / 100.0f);
        }
    }


    void Awake()
    {
        Cost = 0.0f;
    }


    public void SetOccupied()
    {
        Cost = 100.0f;
    }


    public void SetPath(bool path)
    {
        isPath = path;
        if (isPath)
        {
            GetComponent<Renderer>().material.color = pathColor;
        }
        else
        {
            Cost = cost;
        }
    }


    public bool IsOccupied()
    {
        return cost == 100.0f;
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
