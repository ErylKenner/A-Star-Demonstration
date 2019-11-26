using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Actor : MonoBehaviour
{
    public class Waypoint
    {
        public Waypoint(Vector3 _position, Quaternion _orientation)
        {
            position = _position;
            orientation = _orientation;
        }
        public Vector3 position;
        public Quaternion orientation;
    }




    public List<Waypoint> waypoints;
    public float speed = 5.0f;

    void Awake()
    {
        waypoints = new List<Waypoint>();
    }

    void FixedUpdate()
    {
        if (waypoints.Count == 0)
        {
            return;
        }
        float distToTravel = speed * Time.fixedDeltaTime;
        float traveled = 0f;
        while (traveled < distToTravel && waypoints.Count > 0)
        {
            float distToNext = Vector3.Distance(transform.position, waypoints.ElementAt(0).position);
            if (traveled + distToNext <= distToTravel)
            {
                transform.position = waypoints.ElementAt(0).position;
                traveled += distToNext;
                waypoints.RemoveAt(0);
            }
            else
            {
                transform.position += Vector3.Normalize(waypoints.ElementAt(0).position - transform.position) * (distToTravel - traveled);
                traveled = distToTravel;
            }
        }
    }
}
