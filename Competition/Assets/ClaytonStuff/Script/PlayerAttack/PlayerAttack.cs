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

    [Header("References")]
    [SerializeField] private ObjectPool arrowPool; // assign pool with Arrow prefab

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

        if (Time.time >= nextAttackTime && Mouse.current.leftButton.wasPressedThisFrame)
        {
            var item = heldItem != null ? heldItem.GetEquippedItem() : null;
            if (item == null || !item.isWeapon) return;

            if (item.isBow)
            {
                FireArrow();
                Debug.Log("arrow fired");
            }
            else
                AttackMelee();

            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    private void FireArrow()
    {
        if (arrowPool == null)
        {
            Debug.LogWarning("No arrow pool assigned!");
            return;
        }

        GameObject arrowObj = arrowPool.Get();
        arrowObj.transform.position = attackPoint.position;

        // Get facing direction
        Vector2 dir = (attackPoint.position - transform.position).normalized;
        arrowObj.GetComponent<Arrow>().Fire(dir);
    }

    private void AttackMelee()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position, attackRange, enemyLayer
        );

        foreach (var enemy in hitEnemies)
        {
            Debug.Log($"Hit {enemy.name}, dealt {attackDamage}");
            // TODO: Apply melee damage
        }
    }

    private void UpdateAttackPoint()
    {
        if (animator == null) return;

        float moveX = animator.GetFloat("moveX");
        float moveY = animator.GetFloat("moveY");

        Vector2 offset = Vector2.zero;

        if (Mathf.Abs(moveX) > Mathf.Abs(moveY))
            offset = moveX > 0 ? rightAttackOffset : leftAttackOffset;
        else if (Mathf.Abs(moveY) > 0.01f)
            offset = moveY > 0 ? upAttackOffset : downAttackOffset;
        else
            offset = rightAttackOffset;

        attackPoint.localPosition = offset;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
