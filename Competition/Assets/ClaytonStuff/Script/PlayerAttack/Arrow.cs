using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Arrow : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
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
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log($"Arrow hit enemy {collision.name}, dealt {damage}");

            EnemyBase enemy = collision.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                EnemyParty party = enemy.GetComponent<EnemyParty>();
                if (party != null)
                {
                    BattleManager.instance.HandleBattleTransition(
                        GameObject.FindWithTag("Player"),
                        party
                    );
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
            Debug.Log($"Arrow hit breakable object: {collision.name}");
            breakable.TakeHit();
            gameObject.SetActive(false);
            return;
        }

        // Optional: stop arrow on any other collision
        gameObject.SetActive(false);
    }
}
