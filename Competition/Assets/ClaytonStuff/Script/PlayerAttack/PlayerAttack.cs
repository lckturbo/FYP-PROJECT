using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Config")]
    [SerializeField] private Transform attackPoint;  // Empty GameObject at player's hand/weapon
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;

    [Header("Attack Settings")]
    [SerializeField] private float attackRate = 1f;  // attacks per second
    private float nextAttackTime = 0f;

    [Header("Attack Offsets")]
    [SerializeField] private Vector2 rightAttackOffset = new Vector2(1f, 0f);
    [SerializeField] private Vector2 leftAttackOffset = new Vector2(-1f, 0f);


    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (attackPoint == null)
        {
            attackPoint = new GameObject("AttackPoint").transform;
            attackPoint.SetParent(transform);
            attackPoint.localPosition = Vector3.right; // default to right
        }
    }

    private void Update()
    {
        // Attack on right mouse click
        if (Time.time >= nextAttackTime && Input.GetMouseButtonDown(0))
        {
            Attack();
            Debug.Log("PressedAttacked");
            nextAttackTime = Time.time + 1f / attackRate;
        }

        if (playerMovement != null)
        {
            attackPoint.localPosition = playerMovement.SRFlipX
                ? (Vector3)leftAttackOffset
                : (Vector3)rightAttackOffset;
        }

    }

    private void Attack()
    {
        // Detect enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            //var health = enemy.GetComponent<EnemyHealth>();
            //if (health != null)
            //    health.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
