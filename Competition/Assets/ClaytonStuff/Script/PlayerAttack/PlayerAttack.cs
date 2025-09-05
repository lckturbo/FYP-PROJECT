using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Config")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int attackDamage = 1;

    [Header("Attack Settings")]
    [SerializeField] private float attackRate = 1f;
    private float nextAttackTime = 0f;

    [Header("Attack Offsets")]
    [SerializeField] private Vector2 rightAttackOffset = new Vector2(1f, 0f);
    [SerializeField] private Vector2 leftAttackOffset = new Vector2(-1f, 0f);

    private PlayerMovement playerMovement;
    private PlayerHeldItem heldItem;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        heldItem = GetComponent<PlayerHeldItem>();

        if (attackPoint == null)
        {
            attackPoint = new GameObject("AttackPoint").transform;
            attackPoint.SetParent(transform);
            attackPoint.localPosition = Vector3.right;
        }
    }

    private void Update()
    {
        if (playerMovement != null)
        {
            attackPoint.localPosition = playerMovement.SRFlipX
                ? (Vector3)leftAttackOffset
                : (Vector3)rightAttackOffset;
        }

        // Attack on left mouse click
        if (Time.time >= nextAttackTime && Input.GetMouseButtonDown(0))
        {
            //  Check if equipped item is a weapon
            var item = heldItem != null ? heldItem.GetEquippedItem() : null;
            if (item == null || !item.isWeapon)
            {
                Debug.Log("No weapon equipped!");
                return;
            }

            Attack();
            Debug.Log("Attack triggered with: " + item.itemName);
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hit " + enemy.name);
            //enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
