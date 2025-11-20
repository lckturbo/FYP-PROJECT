using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : MonoBehaviour, IDataPersistence
{
    public static EnemyTracker instance;
    private HashSet<string> defeatedEnemies = new HashSet<string>();

    private void Awake()
    {
        if (!instance) 
            instance = this;
        else 
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);
    }

    public void MarkDefeated(string id)
    {
        defeatedEnemies.Add(id);
    }

    public bool IsDefeated(string id)
    {
        return defeatedEnemies.Contains(id);
    }

    public int GetAliveEnemyCount()
    {
        int alive = 0;
        EnemyParty[] parties = FindObjectsOfType<EnemyParty>(true); 
        foreach (var party in parties)
        {
            if (!IsDefeated(party.GetID()))
                alive++;
        }
        return alive;
    }

    public void LoadData(GameData data)
    {
        defeatedEnemies = new HashSet<string>(data.defeatedEnemies);
    }

    public void SaveData(ref GameData data)
    {
        data.defeatedEnemies = new List<string>(defeatedEnemies);
    }

    public void ResetEnemies()
    {
        defeatedEnemies.Clear();
    }
}
