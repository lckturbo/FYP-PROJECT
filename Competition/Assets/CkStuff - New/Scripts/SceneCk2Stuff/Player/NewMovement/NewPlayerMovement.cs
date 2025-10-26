//Doesn't allow diagonal movement
using UnityEngine;
using UnityEngine.InputSystem;

public class NewPlayerMovement : MonoBehaviour, IDataPersistence
{
    [SerializeField] private NewCharacterStats stats;
    public NewCharacterStats GetStats() => stats;
    [SerializeField] private bool useStatsDirectly = true;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction moveAction;

    private Vector2 inputRaw;
    public Vector2 moveDir;
    private float cachedWalkSpeed;

    public bool IsFacingRight { get; private set; } = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        if (playerInput && playerInput.actions != null)
            moveAction = playerInput.actions["Move"];

            if (stats != null)
            ApplyStats(stats);

        if (SaveLoadSystem.instance)
            SaveLoadSystem.instance.RegisterDataPersistenceObjects(this);
    }

    public void ApplyStats(NewCharacterStats newStats)
    {
        stats = newStats;
        if (!useStatsDirectly && stats != null)
            cachedWalkSpeed = stats.Speed;

        PlayerAttack atk = GetComponent<PlayerAttack>();
        if (atk != null && newStats is NewCharacterStats charStats)
        {
            atk.ApplyStats(charStats);
        }
    }
    void Update()
    {
        if (DialogueManager.IsDialogueActiveGlobal)
        {
            moveDir = Vector2.zero;
            animator.SetBool("moving", false);
            rb.velocity = Vector2.zero;
            return;
        }

        if (GetComponent<PlayerAttack>().IsAttacking)
        {
            moveDir = Vector2.zero;
            animator.SetBool("moving", false);
            return;
        }
        if (GetComponent<PlayerAttack>().IsAttacking)
        {
            moveDir = Vector2.zero;
            animator.SetBool("moving", false);
            Debug.Log("[NewPlayerMovement] moving; " + animator.GetBool("moving"));
            return;
        }

        inputRaw = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        if (inputRaw.x != 0f)
            moveDir = new Vector2(Mathf.Sign(inputRaw.x), 0f);
        else if (inputRaw.y != 0f)
            moveDir = new Vector2(0f, Mathf.Sign(inputRaw.y));
        else
            moveDir = Vector2.zero;

        if (moveDir.x > 0)
            IsFacingRight = true;
        else if (moveDir.x < 0)
            IsFacingRight = false;

        // animator
        if (moveDir != Vector2.zero)
        {
            animator.SetFloat("moveX", moveDir.x);
            animator.SetFloat("moveY", moveDir.y);
            animator.SetBool("moving", true);
        }
        else
        {
            //animator.SetFloat("moveX", 0f); // jas add
            //animator.SetFloat("moveY", -1f); // jas add
            animator.SetBool("moving", false);
        }

        //Debug.Log("[NewPlayerMovement] moving2; " + animator.GetBool("moving"));

        // ?? Check for B key to print stats
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            PrintStats();
        }
    }

    private void PrintStats()
    {
        if (stats == null)
        {
            Debug.LogWarning("[NewPlayerMovement] No stats assigned!");
            return;
        }

        Debug.Log($"[Player Stats]\n" +
                  $"Speed: {stats.Speed}\n" +
                  $"Max HP: {stats.maxHealth}\n" +
                  $"Attack Damage: {stats.atkDmg}\n" +
                  $"Defense: {stats.attackreduction}\n" +
                  $"Crit Rate: {stats.critRate * 100f}%\n" +
                  $"Crit Damage: {stats.critDamage}\n" +
                  $"Element: {stats.attackElement}\n");
    }


    void FixedUpdate()
    {
        float speed = GetWalkSpeed();
        if (speed <= 0f || moveDir == Vector2.zero) return;

        Vector2 next = rb.position + moveDir * (speed * Time.fixedDeltaTime);
        rb.MovePosition(next);
    }

    private float GetWalkSpeed()
    {
        if (stats == null) return cachedWalkSpeed;
        return useStatsDirectly ? stats.Speed : cachedWalkSpeed;
    }

    public void LoadData(GameData data) { }

    public void SaveData(ref GameData data)
    {
        Debug.Log("[NewPlayerMovement] saving position");
        data.playerPosition = (Vector2)transform.position;
        data.hasSavedPosition = true;
    }
}