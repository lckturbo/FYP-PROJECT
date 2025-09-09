using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private Vector2 upAttackOffset = new Vector2(0f, 1f);
    [SerializeField] private Vector2 downAttackOffset = new Vector2(0f, -1f);

    private NewPlayerMovement playerMovement;
    private PlayerHeldItem heldItem;
    private Animator animator;

    private void Awake()
    {
        playerMovement = GetComponent<NewPlayerMovement>();
        heldItem = GetComponent<PlayerHeldItem>();
        animator = GetComponent<Animator>();

        if (attackPoint == null)
        {
            attackPoint = new GameObject("AttackPoint").transform;
            attackPoint.SetParent(transform);
            attackPoint.localPosition = Vector3.right;
        }
    }

    private void Update()
    {
        UpdateAttackPoint();

        // Attack on left mouse click
        if (Time.time >= nextAttackTime && Mouse.current.leftButton.wasPressedThisFrame)
        {
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

    private void UpdateAttackPoint()
    {
        if (animator == null) return;

        float moveX = animator.GetFloat("moveX");
        float moveY = animator.GetFloat("moveY");

        Vector2 offset = Vector2.zero;

        // Pick dominant axis for facing direction
        if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
        {
            offset = moveX > 0 ? rightAttackOffset : leftAttackOffset;
        }
        else if (Mathf.Abs(moveY) > 0.01f)
        {
            offset = moveY > 0 ? upAttackOffset : downAttackOffset;
        }
        else
        {
            // Default facing right if idle
            offset = rightAttackOffset;
        }

        attackPoint.localPosition = offset;
    }

    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position, attackRange, enemyLayer
        );

        Debug.Log($"Attack at {attackPoint.position}, radius {attackRange}, hits {hitEnemies.Length}");

        foreach (var enemy in hitEnemies)
        {
            Debug.Log($"Hit {enemy.name}, Layer: {LayerMask.LayerToName(enemy.gameObject.layer)}");
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
