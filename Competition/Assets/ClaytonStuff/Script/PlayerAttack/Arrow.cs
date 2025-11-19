using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
  //  [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 3f;

    private float timer;
    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // ensures no unwanted physics forces
    }

    private void OnEnable()
    {
        timer = lifeTime;
    }

    public void Fire(Vector2 dir)
    {
        direction = dir.normalized;
        rb.velocity = direction * speed; // physics-based movement
        timer = lifeTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Reset velocity when pooled
        if (rb != null) rb.velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        // Hit Enemy
        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
        {
            EnemyBase enemy = collision.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                var stats = enemy.GetEnemyStats();
                if (stats != null && stats.type == EnemyStats.EnemyTypes.MiniBoss)
                {
                    Debug.Log("Cannot attack boss directly.");
                    gameObject.SetActive(false);
                    return;
                }

                EnemyParty party = enemy.GetComponent<EnemyParty>();

                // Check if the enemy has a battle-start dialogue
                if (enemy is StaticEnemy staticEnemy && staticEnemy.attackDialogue != null)
                {
                    // Play dialogue first
                    DialogueManager.Instance.StartDialogue(staticEnemy.attackDialogue);

                    // Continue to battle AFTER dialogue finishes
                    StartCoroutine(StartBattleAfterDialogueArrow(party));

                    gameObject.SetActive(false);
                    return;
                }

                // No dialogue ? start battle immediately
                if (party != null)
                {
                    BattleManager.instance.HandleBattleTransition(party);
                    BattleManager.instance.SetBattleMode(true);
                }
            }

            gameObject.SetActive(false);
            return;
        }


        // Hit Breakable Object
        BreakableObject breakable = collision.GetComponent<BreakableObject>();
        if (breakable != null)
        {
            breakable.TakeHit();
            gameObject.SetActive(false);
            return;
        }

        // Other collisions ? disable arrow
        gameObject.SetActive(false);
    }

    private IEnumerator StartBattleAfterDialogueArrow(EnemyParty party)
    {
        // Wait until all dialogue text is finished
        while (DialogueManager.Instance.IsDialogueActive)
            yield return null;

        if (party != null)
        {
            BattleManager.instance.HandleBattleTransition(party);
            BattleManager.instance.SetBattleMode(true);
        }
    }


}
