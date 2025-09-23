using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
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
    [SerializeField] private ObjectPool arrowPool; // pool of Arrow prefab

    [Header("Attack Offsets")]
    [SerializeField] private Vector2 rightAttackOffset = new Vector2(1f, 0f);
    [SerializeField] private Vector2 leftAttackOffset = new Vector2(-1f, 0f);
    [SerializeField] private Vector2 upAttackOffset = new Vector2(0f, 1f);
    [SerializeField] private Vector2 downAttackOffset = new Vector2(0f, -1f);

    private NewPlayerMovement playerMovement;
    private PlayerHeldItem heldItem;
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction attackAction;

    private void Awake()
    {
        playerMovement = GetComponent<NewPlayerMovement>();
        heldItem = GetComponent<PlayerHeldItem>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null && playerInput.actions != null)
        {
            attackAction = playerInput.actions["Attack"]; // Make sure you have "Attack" in your Input Actions!
        }

        if (attackPoint == null)
        {
            attackPoint = new GameObject("AttackPoint").transform;
            attackPoint.SetParent(transform);
            attackPoint.localPosition = Vector3.right;
        }
    }

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.performed += OnAttackPerformed;
            attackAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.performed -= OnAttackPerformed;
            attackAction.Disable();
        }
    }

    private void Update()
    {
        UpdateAttackPoint();
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (Time.time < nextAttackTime) return;

        var item = heldItem != null ? heldItem.GetEquippedItem() : null;
        if (item == null || !item.isWeapon)
        {
            Debug.Log("No weapon equipped!");
            return;
        }

        if (item.isBow)
            FireArrow();
        else
            AttackMelee();

        nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, attackRate);
    }

    private void FireArrow()
    {
        if (arrowPool == null)
        {
            Debug.LogWarning("Arrow pool not assigned!");
            return;
        }

        GameObject arrowObj = arrowPool.Get();
        arrowObj.transform.position = attackPoint.position;

        // Determine direction based on attackPoint local position
        Vector2 dir = attackPoint.localPosition.normalized;

        // Default fallback
        if (dir == Vector2.zero)
            dir = Vector2.right;

        // Fire arrow
        Arrow arrow = arrowObj.GetComponent<Arrow>();
        if (arrow != null)
            arrow.Fire(dir);

        // Rotate arrow to face direction
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        arrowObj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Debug log which direction
        if (dir == Vector2.up)
            Debug.Log("Arrow fired UP!");
        else if (dir == Vector2.down)
            Debug.Log("Arrow fired DOWN!");
        else if (dir == Vector2.left)
            Debug.Log("Arrow fired LEFT!");
        else if (dir == Vector2.right)
            Debug.Log("Arrow fired RIGHT!");
        else
            Debug.Log("Arrow fired diagonally: " + dir);
    }


    private void AttackMelee()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (var enemy in hitEnemies)
        {
            Debug.Log($"Melee hit {enemy.name}, dealt {attackDamage}");
            // TODO: Apply damage here
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
