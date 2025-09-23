using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 10;

    private List<GameObject> pool = new List<GameObject>();
    private int currentIndex = 0;

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject Get()
    {
        // Cycle through the pool like a circular buffer
        GameObject obj = pool[currentIndex];
        currentIndex = (currentIndex + 1) % pool.Count;

        // If it's still active, recycle it (force reset)
        if (obj.activeInHierarchy)
            obj.SetActive(false);

        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        // Optional: you don’t need to enqueue again, since we’re cycling
    }
}
