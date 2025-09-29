using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    public static EnemyTracker instance;
    private HashSet<string> defeatedEnemies = new HashSet<string>();

    private void Awake()
    {
        if (!instance) 
            instance = this;
        else 
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void MarkDefeated(string id)
    {
        defeatedEnemies.Add(id);
    }

    public bool IsDefeated(string id)
    {
        return defeatedEnemies.Contains(id);
    }
}
