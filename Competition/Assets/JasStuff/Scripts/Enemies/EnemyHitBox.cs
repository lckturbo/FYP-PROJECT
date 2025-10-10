using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private EnemyBase owner;

    public void init(EnemyBase enemy)
    {
        owner = enemy;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enemy hit player!");
            owner?.TriggerAttack();
            
        }
    }
}

