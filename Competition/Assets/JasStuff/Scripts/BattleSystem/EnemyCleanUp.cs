using UnityEngine;

public class EnemyCleanUp : MonoBehaviour
{
    void Start()
    {
        var parties = FindObjectsOfType<EnemyParty>();
        foreach (var p in parties)
        {
            if (EnemyTracker.instance.IsDefeated(p.GetID()))
            {
                Destroy(p.gameObject);
            }
        }
    }
}
