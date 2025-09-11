using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
public class NewPlayerMovement : MonoBehaviour
{
    [Header("Stats (provides walkSpeed)")]
    [SerializeField] private BaseStats stats;
    [SerializeField] private bool useStatsDirectly = true;

    [Header("Input")]
    [SerializeField] private string moveActionName = "Move";

    [Header("Movement Options")]
    private bool normalizeDiagonal = true;
    private float deadzone = 0.1f;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction moveAction;

    private Vector2 inputRaw;
    private Vector2 moveDir;
    private float cachedWalkSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (playerInput && playerInput.actions != null)
        {
            moveAction = playerInput.actions[moveActionName];
            if (moveAction == null)
                Debug.LogWarning($"[NewPlayerMovement] No action named '{moveActionName}' in PlayerInput actions.");
        }

        if (stats != null)
            ApplyStats(stats);
    }

    public void ApplyStats(BaseStats newStats)
    {
        stats = newStats;

        if (!useStatsDirectly && stats != null)
            cachedWalkSpeed = stats.Speed;
    }

    void Update()
    {
        if (moveAction != null)
            inputRaw = moveAction.ReadValue<Vector2>();

        if (inputRaw.sqrMagnitude < deadzone * deadzone)
            inputRaw = Vector2.zero;

        moveDir = inputRaw;
        if (normalizeDiagonal && moveDir.sqrMagnitude > 1e-4f)
            moveDir = moveDir.normalized;

        if (moveDir != Vector2.zero)
        {
            animator.SetFloat("moveX", moveDir.x);
            animator.SetFloat("moveY", moveDir.y);
            animator.SetBool("moving", true);
        }
        else
        {
            animator.SetBool("moving", false);
        }
    }

    void FixedUpdate()
    {
        float speed = GetWalkSpeed();
        if (speed <= 0f) return;

        if (moveDir != Vector2.zero)
        {
            Vector2 next = rb.position + moveDir * (speed * Time.fixedDeltaTime);
            rb.MovePosition(next);
        }
    }

    private float GetWalkSpeed()
    {
        if (stats == null)
            return cachedWalkSpeed;

        return useStatsDirectly ? stats.Speed : cachedWalkSpeed;
    }
}
