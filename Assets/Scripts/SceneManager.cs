using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public ObstacleManager obstacleManager;
    public GroundGrid groundGrid;

    public GameObject LoadingText;
    public Slider ResolutionSlider;

    private enum State { None, RegenerateObstacles, RegenerateObstacles2, RegenerateObstacles3, CalculatePath, CalculatePath2, CalculateObstacleCollisions, RegenerateGrid, CreateObstacles };
    private State state;

    void Start()
    {
        groundGrid.CreateGrid((int)ResolutionSlider.value, (int)ResolutionSlider.value);
        state = State.CreateObstacles;
    }

    public void SetBoardResolution()
    {
        LoadingText.SetActive(true);
        state = State.RegenerateGrid;
    }

    void Update()
    {
        LoadingText.SetActive(true);
        switch (state)
        {
            case State.CalculatePath:
                groundGrid.SetUnoccupiedNodesToEmpty();
                state = State.CalculatePath2;
                break;
            case State.CalculatePath2:
                groundGrid.ColorPath(A_Star.CalculatePath(groundGrid.GetStart(), groundGrid.GetEnd(), groundGrid.AdjacencyMatrix, groundGrid.heuristic));
                state = State.None;
                break;

            case State.RegenerateObstacles:
                groundGrid.ResetPath();
                state = State.RegenerateObstacles2;
                break;
            case State.RegenerateObstacles2:
                obstacleManager.ResetObstacles();
                state = State.RegenerateObstacles3;
                break;
            case State.CreateObstacles:
            case State.RegenerateObstacles3:
                obstacleManager.CreateObstacles();
                state = State.CalculateObstacleCollisions;
                break;

            case State.RegenerateGrid:
                groundGrid.CreateGrid((int)ResolutionSlider.value, (int)ResolutionSlider.value);
                state = State.CalculateObstacleCollisions;
                break;

            case State.CalculateObstacleCollisions:
                groundGrid.UpdateObstacleCollisions();
                state = State.None;
                break;
            case State.None:
                LoadingText.SetActive(false);
                break;
            default:
                LoadingText.SetActive(false);
                break;
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            state = State.RegenerateObstacles;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                state = State.CalculateObstacleCollisions;
                //Create obstacle
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Node")))
                {
                    groundGrid.ResetPath();
                    obstacleManager.CreateObstacle(hit.point);
                    state = State.CalculateObstacleCollisions;
                }
            }
            else
            {
                state = State.None;
                //Set start node
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Node")))
                {
                    Node collidedNode = hit.collider.GetComponent<Node>();
                    if (collidedNode.State != Node.NodeState.occupied && collidedNode != groundGrid.startNode && collidedNode != groundGrid.endNode)
                    {
                        groundGrid.StartSprite.transform.position = new Vector3(collidedNode.transform.position.x, groundGrid.StartSprite.transform.position.y, collidedNode.transform.position.z);
                        groundGrid.startNode = collidedNode;
                        if (groundGrid.startNode != null && groundGrid.endNode != null)
                        {
                            state = State.CalculatePath;
                        }
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            state = State.None;
            //Set end node
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Node")))
            {
                Node collidedNode = hit.collider.GetComponent<Node>();
                if (collidedNode.State != Node.NodeState.occupied && collidedNode != groundGrid.startNode && collidedNode != groundGrid.endNode)
                {
                    groundGrid.EndSprite.transform.position = new Vector3(collidedNode.transform.position.x, groundGrid.EndSprite.transform.position.y, collidedNode.transform.position.z);
                    groundGrid.endNode = collidedNode;
                    if (groundGrid.startNode != null && groundGrid.endNode != null)
                    {
                        state = State.CalculatePath;
                    }
                }
            }
        }

    }
}
