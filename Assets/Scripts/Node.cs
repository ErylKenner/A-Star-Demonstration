using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class Node : MonoBehaviour
{
    public enum NodeState { occupied, empty, path, explored };

    public Material emptyMaterial;
    public Material occupiedMaterial;
    public Material pathMaterial;
    public Material exploredMaterial;

    private int intersectCount = 0;

    private NodeState state;
    public NodeState State {
        get { return state; }
        set {
            if(value != state)
            {
                state = value;
                switch (value)
                {
                    case NodeState.empty:
                        GetComponent<MeshRenderer>().material = emptyMaterial;
                        break;
                    case NodeState.occupied:
                        GetComponent<MeshRenderer>().material = occupiedMaterial;
                        break;
                    case NodeState.path:
                        GetComponent<MeshRenderer>().material = pathMaterial;
                        break;
                    case NodeState.explored:
                        GetComponent<MeshRenderer>().material = exploredMaterial;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    void Awake()
    {
        State = NodeState.empty;
    }

    void Start()
    {

    }

    void Update()
    {

    }
    
    private void OnTriggerEnter(Collider collided)
    {
        
        if (collided.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            intersectCount++;
            if (intersectCount > 0)
            {
                State = NodeState.occupied;
            }
            else
            {
                State = NodeState.empty;
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
                State = NodeState.occupied;
            }
            else
            {
                State = NodeState.empty;
            }
        }

    }
}
