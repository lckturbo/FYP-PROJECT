using System.Collections.Generic;
using UnityEngine;

public class WayPointManager : MonoBehaviour
{
    public static WayPointManager instance;
    [Header("Waypoints")]
    [SerializeField] private GameObject _waypointPrefab;
    private Transform _spawnParent;

    [SerializeField] private List<Vector2> _spawnPositions = new List<Vector2>();
    private List<GameObject> _waypoints = new List<GameObject>();

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        if (!_spawnParent) _spawnParent = this.transform;

        SpawnWayPoints();
    }

    private void SpawnWayPoints()
    {
        foreach (Vector2 pos in _spawnPositions)
        {
            GameObject wp = Instantiate(_waypointPrefab, pos, Quaternion.identity, _spawnParent);
            _waypoints.Add(wp);
        }
    }

    public int GetTotalWayPoints()
    {
        return _waypoints.Count;
    }

    public Vector3 GetWayPoint(int index)
    {
        if (index >= 0 && index < _waypoints.Count)
            return _waypoints[index].transform.position;

        return Vector3.zero;
    }

    public Waypoints GetFreeWayPoint()
    {
        List<Waypoints> free = new List<Waypoints>();
        foreach (GameObject go in _waypoints)
        {
            if (go)
            {
                Waypoints wp = go.GetComponent<Waypoints>();
                if (wp) free.Add(wp);
            }
        }

        if (free.Count == 0) return null;

        return free[Random.Range(0, free.Count)];
    }
}
