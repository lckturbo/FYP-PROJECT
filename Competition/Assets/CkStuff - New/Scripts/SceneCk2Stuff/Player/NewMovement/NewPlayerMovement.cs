using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
public class NewPlayerMovement : MonoBehaviour, IDataPersistence
{
    [Header("Stats (provides walkSpeed)")]
    [SerializeField] private BaseStats stats;
    [SerializeField] private bool useStatsDirectly = true;

    [Header("Input Action Name")]
    [SerializeField] private string moveActionName = "Move"; // 2D Vector action (WASD/Stick)

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction moveAction;

    private Vector2 inputRaw;     // raw input
    private Vector2 moveDir;      // snapped to 4-way only
    private float cachedWalkSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (playerInput && playerInput.actions != null)
            moveAction = playerInput.actions[moveActionName];

        if (stats != null)
            ApplyStats(stats);

        if (SaveLoadSystem.instance)
            SaveLoadSystem.instance.RegisterDataPersistenceObjects(this);

    }

    public void ApplyStats(BaseStats newStats)
    {
        stats = newStats;
        if (!useStatsDirectly && stats != null)
            cachedWalkSpeed = stats.Speed;
    }

    void Update()
    {
        // read raw input from Input System
        inputRaw = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        // 4-way only: horizontal first, else vertical
        if (inputRaw.x != 0f)
            moveDir = new Vector2(Mathf.Sign(inputRaw.x), 0f);
        else if (inputRaw.y != 0f)
            moveDir = new Vector2(0f, Mathf.Sign(inputRaw.y));
        else
            moveDir = Vector2.zero;

        // animator
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
        data.playerPosition = (Vector2)transform.position;
        Debug.Log("transform.position: "+ transform.position);
        data.hasSavedPosition = true;
    }

    //private void OnDisable()
    //{
    //    if (SaveLoadSystem.instance && SceneManager.GetActiveScene().name != "jasBattle")
    //        SaveLoadSystem.instance.SaveGame();
    //}
}
