using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
  //  [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 5f;

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

                // Dialogue BEFORE battle
                if (enemy is StaticEnemy staticEnemy && staticEnemy.attackDialogue != null)
                {
                    DialogueManager.Instance.StartDialogue(staticEnemy.attackDialogue);

                    // DO NOT DISABLE ARROW HERE
                    StartCoroutine(ArrowBattleFlow(party));
                    return;
                }

                // No dialogue? Start battle immediately
                if (party != null)
                {
                    BattleManager.instance.HandleBattleTransition(party);
                    BattleManager.instance.SetBattleMode(true);

                    // Only disable AFTER battle starts
                    gameObject.SetActive(false);
                    return;
                }
            }

            // Enemy but no party? Still disable
            gameObject.SetActive(false);
            return;
        }

        // Breakable object
        BreakableObject breakable = collision.GetComponent<BreakableObject>();
        if (breakable != null)
        {
            breakable.TakeHit();
            gameObject.SetActive(false);
            return;
        }

        // Other collisions
        gameObject.SetActive(false);
    }


    private IEnumerator ArrowBattleFlow(EnemyParty party)
    {
        // Wait until dialogue ends
        while (DialogueManager.Instance.IsDialogueActive)
            yield return null;

        if (party != null)
        {
            BattleManager.instance.HandleBattleTransition(party);
            BattleManager.instance.SetBattleMode(true);
        }

        // NOW disable arrow after battle starts
        gameObject.SetActive(false);
    }



}
