using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private MovementConfig config;

    [Header("Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("HUD")]
    [SerializeField] private bool showHud = true;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // Input state
    private float inputX;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool dashPressed;

    // Movement state
    private bool isGrounded;
    private bool isTouchingWallLeft, isTouchingWallRight;
    private bool wallSliding;

    private int airJumpsLeft;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private int dashDir = 1;
    public bool SRFlipX => sr != null && sr.flipX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        airJumpsLeft = (config != null) ? config.maxAirJumps : 0;
    }

    public void ApplyMovementConfig(MovementConfig cfg)
    {
        config = cfg;
        airJumpsLeft = config.maxAirJumps;
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 value = ctx.ReadValue<Vector2>();
        inputX = Mathf.Clamp(value.x, -1f, 1f);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            jumpPressed = true;
            jumpHeld = true;
        }
        if (ctx.canceled)
        {
            jumpHeld = false;
        }
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            dashPressed = true;
        }
    }

    void Update()
    {
        if (config == null) return;

        // Grounding + coyote
        if (isGrounded)
        {
            coyoteTimer = config.coyoteTime;
            airJumpsLeft = config.maxAirJumps;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // Jump buffer
        if (jumpPressed) jumpBufferTimer = config.jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;

        // Dash cooldown
        dashCooldownTimer -= Time.deltaTime;

        // Start dash
        if (config.enableDash && dashPressed && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = config.dashTime;
            dashCooldownTimer = config.dashCooldown;

            int facing = (Mathf.Abs(inputX) > 0.01f) ? (inputX > 0 ? 1 : -1) : (sr && config.flipSprite && sr.flipX ? -1 : 1);
            dashDir = facing;
            rb.velocity = new Vector2(dashDir * config.dashSpeed, 0f);
        }

        // Variable jump height
        if (!isDashing)
        {
            if (rb.velocity.y < 0f)
                rb.velocity += Vector2.up * Physics2D.gravity.y * (config.fallGravityMultiplier - 1f) * Time.deltaTime;
            else if (rb.velocity.y > 0f && !jumpHeld)
                rb.velocity += Vector2.up * Physics2D.gravity.y * (config.lowJumpMultiplier - 1f) * Time.deltaTime;
        }

        // Flip sprite
        if (config.flipSprite && sr && Mathf.Abs(inputX) > 0.01f)
            sr.flipX = inputX < 0f;

        // Clear one-frame latches
        jumpPressed = false;
        dashPressed = false;
    }

    void FixedUpdate()
    {
        if (config == null) return;

        // Collision checks
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        isTouchingWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, checkRadius, wallLayer);
        isTouchingWallRight = Physics2D.OverlapCircle(wallCheckRight.position, checkRadius, wallLayer);
        bool touchingWall = isTouchingWallLeft || isTouchingWallRight;

        // Dashing
        if (isDashing)
        {
            rb.velocity = new Vector2(dashDir * config.dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) isDashing = false;
            return;
        }

        // Horizontal move
        float targetX = inputX * config.moveSpeed;
        float lerp = isGrounded ? 1f : config.airControlMultiplier;
        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, targetX, lerp), rb.velocity.y);

        // Wall slide
        wallSliding = false;
        if (config.enableWallMoves && touchingWall && !isGrounded && rb.velocity.y < 0f)
        {
            wallSliding = true;
            if (rb.velocity.y < -config.wallSlideSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -config.wallSlideSpeed);

            rb.velocity = new Vector2(rb.velocity.x * 0.2f, rb.velocity.y); // optional slow horizontal
        }

        // Prevent glue
        bool intoLeft = isTouchingWallLeft && rb.velocity.x < 0f;
        bool intoRight = isTouchingWallRight && rb.velocity.x > 0f;
        if (intoLeft || intoRight)
            rb.velocity = new Vector2(0f, rb.velocity.y);

        // Jumps
        if (jumpBufferTimer > 0f)
        {
            if (config.enableWallMoves && touchingWall && !isGrounded)
            {
                // Wall jump (don’t reset airJumpsLeft!)
                jumpBufferTimer = 0f;

                int wallDir = isTouchingWallRight ? 1 : -1;
                Vector2 jv = new Vector2(-wallDir * config.wallJumpPower.x, config.wallJumpPower.y);
                rb.velocity = new Vector2(jv.x, 0f);
                rb.AddForce(new Vector2(0f, jv.y), ForceMode2D.Impulse);

                if (sr && config.flipSprite) sr.flipX = wallDir > 0;
            }
            else if (coyoteTimer > 0f)
            {
                // Ground jump
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * config.jumpForce, ForceMode2D.Impulse);
            }
            else if (airJumpsLeft > 0)
            {
                // Air jump
                jumpBufferTimer = 0f;
                airJumpsLeft--;
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * config.jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (wallCheckLeft)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheckLeft.position, checkRadius);
        }
        if (wallCheckRight)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheckRight.position, checkRadius);
        }
    }

    void OnGUI()
    {
        if (!showHud) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 12;

        string text =
            "Vel: (" + rb.velocity.x.ToString("0.00") + ", " + rb.velocity.y.ToString("0.00") + ")\n" +
            "InputX: " + inputX.ToString("0.00") + "\n" +
            "Grounded: " + isGrounded + "  WallL: " + isTouchingWallLeft + "  WallR: " + isTouchingWallRight + "\n" +
            "WallSlide: " + wallSliding + "  Dashing: " + isDashing + "  DashCD: " + Mathf.Max(0, dashCooldownTimer).ToString("0.00") + "\n" +
            "AirJumpsLeft: " + airJumpsLeft + "\n" +
            "Coyote: " + Mathf.Max(0, coyoteTimer).ToString("0.00") + "  Buffer: " + Mathf.Max(0, jumpBufferTimer).ToString("0.00");

        GUI.Box(new Rect(10, 10, 320, 90), text, style);
    }
}
