using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Config")]
    public MovementConfig config;

    [Header("Checks")]
    public Transform groundCheck;       // child at feet
    public Transform wallCheckLeft;     // child at left side
    public Transform wallCheckRight;    // child at right side
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;       // floors/platforms
    public LayerMask wallLayer;         // walls only

    [Header("Input")]
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("Debug")]
    public bool debug = true;
    public bool showHud = true;

    Rigidbody2D rb;
    SpriteRenderer sr;

    float inputX;
    bool jumpPressed, jumpHeld, dashPressed;

    bool isGrounded;
    bool isTouchingWallLeft, isTouchingWallRight;
    int airJumpsLeft;

    float coyoteTimer;
    float jumpBufferTimer;

    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    int dashDir = 1;

    // Debug state tracking
    bool wasGrounded, wasWallSliding;
    int lastFacing = 1;

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
        Log("Applied config");
    }

    void Update()
    {
        if (config == null) return;

        inputX = Input.GetAxisRaw(horizontalAxis);
        jumpPressed = Input.GetButtonDown(jumpButton);
        jumpHeld = Input.GetButton(jumpButton);
        dashPressed = Input.GetKeyDown(dashKey);

        if (isGrounded)
        {
            coyoteTimer = config.coyoteTime;
            airJumpsLeft = config.maxAirJumps;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpPressed) jumpBufferTimer = config.jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;

        dashCooldownTimer -= Time.deltaTime;

        // Dash start
        if (config.enableDash && dashPressed && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = config.dashTime;
            dashCooldownTimer = config.dashCooldown;

            int facing = 1;
            if (Mathf.Abs(inputX) > 0.01f) facing = inputX > 0 ? 1 : -1;
            else if (sr && config.flipSprite) facing = sr.flipX ? -1 : 1;

            dashDir = facing;
            rb.velocity = new Vector2(dashDir * config.dashSpeed, 0f);

            Log("Dash start dir=" + dashDir);
        }

        // Variable jump height
        if (!isDashing)
        {
            if (rb.velocity.y < 0f)
                rb.velocity += Vector2.up * Physics2D.gravity.y * (config.fallGravityMultiplier - 1f) * Time.deltaTime;
            else if (rb.velocity.y > 0f && !jumpHeld)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (config.lowJumpMultiplier - 1f) * Time.deltaTime;
                Log("Jump cut");
            }
        }

        // Flip sprite
        if (config.flipSprite && sr && Mathf.Abs(inputX) > 0.01f)
        {
            int facing = inputX < 0f ? -1 : 1;
            if (facing != lastFacing)
            {
                Log("Facing " + (facing > 0 ? "Right" : "Left"));
                lastFacing = facing;
            }
            sr.flipX = inputX < 0f;
        }
    }

    void FixedUpdate()
    {
        if (config == null) return;

        // Checks
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        isTouchingWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, checkRadius, wallLayer);
        isTouchingWallRight = Physics2D.OverlapCircle(wallCheckRight.position, checkRadius, wallLayer);
        bool isTouchingAnyWall = isTouchingWallLeft || isTouchingWallRight;

        if (isGrounded != wasGrounded)
        {
            Log(isGrounded ? "Landed" : "Left ground");
            wasGrounded = isGrounded;
        }

        if (isDashing)
        {
            rb.velocity = new Vector2(dashDir * config.dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                Log("Dash end");
            }
            return;
        }

        // Horizontal move
        float targetX = inputX * config.moveSpeed;
        float lerp = isGrounded ? 1f : config.airControlMultiplier;
        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, targetX, lerp), rb.velocity.y);

        // Wall slide
        bool wallSliding = false;
        if (config.enableWallMoves && isTouchingAnyWall && !isGrounded && Mathf.Abs(inputX) > 0.01f)
        {
            wallSliding = true;
            if (rb.velocity.y < -config.wallSlideSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -config.wallSlideSpeed);
            if (!wasWallSliding) Log("Wall slide start");
        }
        if (wasWallSliding && !wallSliding) Log("Wall slide end");
        wasWallSliding = wallSliding;

        // Jumps
        if (jumpBufferTimer > 0f)
        {
            if (config.enableWallMoves && isTouchingAnyWall && !isGrounded)
            {
                jumpBufferTimer = 0f;

                // pick which wall we hit
                int wallDir = 0;
                if (isTouchingWallRight) wallDir = 1;
                else if (isTouchingWallLeft) wallDir = -1;

                Vector2 jv = new Vector2(-wallDir * config.wallJumpPower.x, config.wallJumpPower.y);
                rb.velocity = new Vector2(jv.x, 0f);
                rb.AddForce(new Vector2(0f, jv.y), ForceMode2D.Impulse);

                if (sr && config.flipSprite) sr.flipX = wallDir > 0;
                airJumpsLeft = config.maxAirJumps;

                Log("Wall jump off " + (wallDir > 0 ? "Right" : "Left") + " wall");
            }
            else if (coyoteTimer > 0f)
            {
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * config.jumpForce, ForceMode2D.Impulse);
                Log("Ground jump");
            }
            else if (airJumpsLeft > 0)
            {
                jumpBufferTimer = 0f;
                airJumpsLeft--;
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * config.jumpForce, ForceMode2D.Impulse);
                Log("Air jump, left=" + airJumpsLeft);
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
            "Velocity: (" + rb.velocity.x.ToString("0.00") + ", " + rb.velocity.y.ToString("0.00") + ")\n" +
            "InputX: " + inputX.ToString("0.00") + "\n" +
            "Grounded: " + isGrounded + "\n" +
            "WallLeft: " + isTouchingWallLeft + "  WallRight: " + isTouchingWallRight + "\n" +
            "WallSlide: " + wasWallSliding + "  Dashing: " + isDashing + "\n" +
            "AirJumpsLeft: " + airJumpsLeft + "\n" +
            "Coyote: " + Mathf.Max(0, coyoteTimer).ToString("0.00") + "  Buffer: " + Mathf.Max(0, jumpBufferTimer).ToString("0.00");

        GUI.Box(new Rect(10, 10, 280, 120), text, style);
    }

    void Log(string msg)
    {
        //if (debug) Debug.Log("[Player] " + msg);
    }
}
