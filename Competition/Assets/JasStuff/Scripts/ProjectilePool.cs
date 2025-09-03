using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool instance;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize;

    private Queue<GameObject> projectiles = new Queue<GameObject>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        InitPool();
    }

    private void InitPool()
    {
        for(int i = 0; i < poolSize; i++)
        {
            GameObject proj = Instantiate(projectilePrefab);
            proj.transform.SetParent(gameObject.transform);
            proj.SetActive(false);
            projectiles.Enqueue(proj);
        }
    }

    public GameObject GetProjectile()
    {
        GameObject proj;
        if (projectiles.Count > 0)
            proj = projectiles.Dequeue();
        else
            proj = Instantiate(projectilePrefab);

        proj.SetActive(true);
        return proj;
    }

    public void ReturnProjectile(GameObject p)
    {
        p.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        p.SetActive(false);
        projectiles.Enqueue(p);
    }

}
