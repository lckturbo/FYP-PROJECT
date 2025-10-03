using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    // check if its occupied -> if occupied find another available
    // check nearest waypoint
    
    private bool occupied;
    public void SetOccupied(bool v) { occupied = v; }
    public bool isOccupied() => occupied;
    public List<Waypoints> nearestWaypoints = new List<Waypoints>();

    [SerializeField] private float Range; // check other waypoints in this range
    [SerializeField] private int maxNearest;

    private void Start()
    {
        FindNearestWayPoint();
    }

    public void FindNearestWayPoint()
    {
        // find nearest waypoint to this waypoint
        Waypoints[] allWayPoints = FindObjectsOfType<Waypoints>();
        List<Waypoints> connections = new List<Waypoints>();

        foreach(Waypoints wp in allWayPoints)
        {
            if (wp == this) continue;

            float dist = Vector2.Distance(transform.position, wp.transform.position);
            if(dist <= Range)
                connections.Add(wp);

            connections.Sort((a,b) => Vector2.Distance(transform.position, a.transform.position)
            .CompareTo(Vector2.Distance(transform.position, b.transform.position)));

            nearestWaypoints = connections.GetRange(0, Mathf.Min(maxNearest, connections.Count));
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Range);

        if(nearestWaypoints != null)
        {
            Gizmos.color = Color.green;
            foreach(var wp in nearestWaypoints)
            {
                if (wp != null)
                    Gizmos.DrawLine(transform.position, wp.transform.position);
            }
        }
    }

}
