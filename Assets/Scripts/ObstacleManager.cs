using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    //public GameObject ObstacleCubePrefab;
    //public GameObject ObstacleCylinderPrefab;
    public Obstacle ObstacleShipPrefab;
    public int numObstacles = 1;
    public GroundGrid groundGrid;

    public List<Obstacle> Obstacles { get => obstacles; }
    List<Obstacle> obstacles;

    private void Awake()
    {
        obstacles = new List<Obstacle>();
    }

    public void ResetObstacles()
    {
        while (obstacles.Any())
        {
            const float delay = 0.5f;

            //NECESSARY so that the OnCollisionExit() function is called for nodes that this used to intersect with
            obstacles.First().transform.position = new Vector3(0.0f, -10000.0f, 0.0f);

            //Destroy the object after a delay so that the physics engine has time to call OnCollisionExit() for intersecting nodes
            Destroy(obstacles.First(), delay);

            obstacles.RemoveAt(0);
        }
    }

    public void CreateObstacles()
    {
        for (int i = 0; i < numObstacles; ++i)
        {
            Vector3 pos = new Vector3(Random.Range(-groundGrid.transform.localScale.x / 2, groundGrid.transform.localScale.x / 2), 0.0f, Random.Range(-groundGrid.transform.localScale.y / 2, groundGrid.transform.localScale.y / 2));
            CreateObstacle(pos);
        }
    }

    public void CreateObstacle(Vector3 pos)
    {
        /*GameObject cur;
        if (Random.value < 0.5f)
        {
            cur = Instantiate(ObstacleCubePrefab, pos, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), this.transform);
        }
        else
        {
            cur = Instantiate(ObstacleCylinderPrefab, pos, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), this.transform);
        }
        cur.transform.localScale = new Vector3(Random.Range(5.0f, 30.0f), cur.transform.localScale.y, Random.Range(5.0f, 30.0f));
        cur.layer = LayerMask.NameToLayer("Obstacles");
        obstacles.Add(cur);*/

        Obstacle cur = Instantiate(ObstacleShipPrefab, pos, Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f), this.transform) as Obstacle;
        cur.gameObject.layer = LayerMask.NameToLayer("Obstacles");
        obstacles.Add(cur);
    }
}
