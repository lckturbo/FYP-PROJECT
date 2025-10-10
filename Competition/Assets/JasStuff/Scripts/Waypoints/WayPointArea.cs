using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointArea : MonoBehaviour
{
    [Header("Waypoint Settings")]
    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] private List<Vector2> spawnPosition = new List<Vector2>();

    private List<Transform> waypoints = new List<Transform>();

    private void Awake() { SpawnWaypoints(); }
    private void SpawnWaypoints()
    {
        foreach (Vector2 pos in spawnPosition)
        {
            GameObject wp = Instantiate(waypointPrefab, pos, Quaternion.identity);
            wp.transform.SetParent(transform);
            waypoints.Add(wp.transform);
        }
    }
    public List<Transform> GetWaypoints() => waypoints;

}
