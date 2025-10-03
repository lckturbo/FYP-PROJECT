using UnityEngine;

public class EnemyCleanUp : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(CleanupEnemies), 0.1f);
    }

    private void CleanupEnemies()
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
