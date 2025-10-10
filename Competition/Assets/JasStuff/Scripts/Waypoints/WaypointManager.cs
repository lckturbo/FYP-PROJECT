using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager instance;
    [Header("Waypoint Areas")]
    [SerializeField] private List<WayPointArea> waypointAreas = new List<WayPointArea>();

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }

    public WayPointArea GetAreaByID(int id)
    {
        if (id < 0 || id >= waypointAreas.Count) return null;
        return waypointAreas[id];
    }
}
