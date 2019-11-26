using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public ObstacleManager obstacleManager;
    public GroundGrid groundGrid;
    public Actor actor;

    public GameObject LoadingText;
    public Slider ResolutionSlider;
    public Toggle UseZones;
    public Toggle UseRRT;

    private enum State { None, RegenerateObstacles, RegenerateObstacles2, RegenerateObstacles3, CalculatePath, CalculatePath2, CalculateObstacleCollisions, RegenerateGrid, CreateObstacles, ShowPath };
    private State state;
    private IEnumerator calculatePathCoroutine;
    private bool useZones = false;
    private bool useRRT = false;
    private List<int> outPath;
    private bool finishedCalculatingPath = false;

    void Start()
    {
        groundGrid.CreateGrid((int)ResolutionSlider.value, (int)ResolutionSlider.value);
        state = State.CreateObstacles;
        calculatePathCoroutine = null;
        outPath = new List<int>();
        useRRT = UseRRT.isOn;
        useZones = UseZones.isOn;
    }

    public void SetBoardResolution()
    {
        LoadingText.SetActive(true);
        state = State.RegenerateGrid;
        actor.transform.position = new Vector3(10000, 0, 0);
        actor.waypoints.Clear();
        groundGrid.ResetPath();
    }

    public void SetZoneUsage()
    {
        useZones = UseZones.isOn;
        groundGrid.ResetPath();
        state = State.CalculateObstacleCollisions;
        if (UseZones.isOn)
        {
            UseRRT.isOn = false;
        }
        actor.transform.position = new Vector3(10000, 0, 0);
        actor.waypoints.Clear();
    }

    public void SetRRTUsage()
    {
        groundGrid.ResetPath();
        useRRT = UseRRT.isOn;
        if (UseRRT.isOn)
        {
            UseZones.isOn = false;
        }
        else
        {
            UseZones.isOn = true;
        }
        actor.transform.position = new Vector3(10000, 0, 0);
        actor.waypoints.Clear();
    }

    void Update()
    {
        if (state != State.CalculatePath2 && state != State.ShowPath && state != State.None)
        {
            if (calculatePathCoroutine != null)
            {
                StopCoroutine(calculatePathCoroutine);
                calculatePathCoroutine = null;
            }
        }
        switch (state)
        {
            case State.CalculatePath:
                groundGrid.HidePath();
                outPath.Clear();
                finishedCalculatingPath = false;
                state = State.CalculatePath2;
                break;
            case State.CalculatePath2:
                if (useRRT)
                {
                    if (calculatePathCoroutine == null)
                    {
                        calculatePathCoroutine = PathPlanner.RRT(groundGrid.GetStartNodeIndex(), groundGrid.GetEndNodeIndex(), groundGrid, actor, outPath);
                        StartCoroutine(calculatePathCoroutine);
                    }
                    state = State.ShowPath;
                }
                else
                {
                    if (calculatePathCoroutine == null)
                    {
                        calculatePathCoroutine = PathPlanner.A_Star(groundGrid.GetStartNodeIndex(), groundGrid.GetEndNodeIndex(), groundGrid, outPath);
                        StartCoroutine(calculatePathCoroutine);
                    }
                    state = State.ShowPath;
                }
                break;
            case State.ShowPath:
                if (outPath.Count > 0)
                {
                    finishedCalculatingPath = true;
                    actor.transform.position = groundGrid.GetNodePosition(outPath.ElementAt(0));
                    actor.waypoints.Clear();
                    foreach (int item in outPath)
                    {
                        actor.waypoints.Add(new Actor.Waypoint(groundGrid.GetNodePosition(item), Quaternion.Euler(0, 0, 0)));
                    }
                    state = State.None;
                }
                else if (finishedCalculatingPath)
                {
                    finishedCalculatingPath = false;
                    state = State.None;
                }
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
                groundGrid.UpdateObstacleCollisions(useZones);
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
            actor.transform.position = new Vector3(10000, 0, 0);
            actor.waypoints.Clear();
            state = State.RegenerateObstacles;
            LoadingText.SetActive(true);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                //Create obstacle
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Node")))
                {
                    groundGrid.ResetPath();
                    obstacleManager.CreateObstacle(hit.point);
                    actor.transform.position = new Vector3(10000, 0, 0);
                    actor.waypoints.Clear();
                    state = State.CalculateObstacleCollisions;
                    LoadingText.SetActive(true);
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
                    if (!collidedNode.IsOccupied() && collidedNode != groundGrid.startNode && collidedNode != groundGrid.endNode)
                    {
                        groundGrid.StartSprite.transform.position = new Vector3(collidedNode.transform.position.x, groundGrid.StartSprite.transform.position.y, collidedNode.transform.position.z);
                        groundGrid.startNode = collidedNode;
                        actor.transform.position = new Vector3(10000, 0, 0);
                        actor.waypoints.Clear();
                        if (groundGrid.startNode != null && groundGrid.endNode != null)
                        {
                            actor.transform.position = groundGrid.StartSprite.transform.position;
                            state = State.CalculatePath;
                            LoadingText.SetActive(true);
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
                if (!collidedNode.IsOccupied() && collidedNode != groundGrid.startNode && collidedNode != groundGrid.endNode)
                {
                    groundGrid.EndSprite.transform.position = new Vector3(collidedNode.transform.position.x, groundGrid.EndSprite.transform.position.y, collidedNode.transform.position.z);
                    groundGrid.endNode = collidedNode;
                    actor.transform.position = new Vector3(10000, 0, 0);
                    actor.waypoints.Clear();
                    if (groundGrid.startNode != null && groundGrid.endNode != null)
                    {
                        actor.transform.position = groundGrid.StartSprite.transform.position;
                        state = State.CalculatePath;
                        LoadingText.SetActive(true);
                    }
                }
            }
        }

    }
}
