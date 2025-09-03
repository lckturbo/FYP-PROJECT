using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Config")]
    public MovementConfig config;

    [Header("Checks")]
    public Transform groundCheck;   // child at feet
    public Transform wallCheck;     // child at player side
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Input")]
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";
    public KeyCode dashKey = KeyCode.LeftShift;

    Rigidbody2D rb;
    SpriteRenderer sr;

    float inputX;
    bool jumpPressed, jumpHeld, dashPressed;

    bool isGrounded, isTouchingWall;
    int airJumpsLeft;

    float coyoteTimer;
    float jumpBufferTimer;

    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    int dashDir = 1;

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

        // Start Dash
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
        }

        // Variable jump height (don’t alter while dashing)
        if (!isDashing)
        {
            if (rb.velocity.y < 0f)
                rb.velocity += Vector2.up * Physics2D.gravity.y * (config.fallGravityMultiplier - 1f) * Time.deltaTime;
            else if (rb.velocity.y > 0f && !jumpHeld)
                rb.velocity += Vector2.up * Physics2D.gravity.y * (config.lowJumpMultiplier - 1f) * Time.deltaTime;
        }

        // Flip
        if (config.flipSprite && sr && Mathf.Abs(inputX) > 0.01f)
            sr.flipX = inputX < 0f;
    }

    void FixedUpdate()
    {
        if (config == null) return;

        // Checks
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);

        if (isDashing)
        {
            rb.velocity = new Vector2(dashDir * config.dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) isDashing = false;
            return;
        }

        // Horizontal move (reduced air control)
        float targetX = inputX * config.moveSpeed;
        float lerp = isGrounded ? 1f : config.airControlMultiplier;
        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, targetX, lerp), rb.velocity.y);

        // Wall slide
        bool wallSliding = false;
        if (config.enableWallMoves && isTouchingWall && !isGrounded && Mathf.Abs(inputX) > 0.01f)
        {
            wallSliding = true;
            if (rb.velocity.y < -config.wallSlideSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -config.wallSlideSpeed);
        }

        // Jumping (buffer + coyote + air + wall)
        if (jumpBufferTimer > 0f)
        {
            if (config.enableWallMoves && isTouchingWall && !isGrounded)
            {
                jumpBufferTimer = 0f;
                int wallDir = (wallCheck.position.x > transform.position.x) ? 1 : -1; // wall on right = +1
                Vector2 jv = new Vector2(-wallDir * config.wallJumpPower.x, config.wallJumpPower.y);
                rb.velocity = new Vector2(jv.x, 0f);
                rb.AddForce(new Vector2(0f, jv.y), ForceMode2D.Impulse);
                // face away from wall
                if (sr && config.flipSprite) sr.flipX = wallDir > 0;
                airJumpsLeft = config.maxAirJumps; // restore air jumps after wall jump (optional)
            }
            else if (coyoteTimer > 0f)
            {
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector2.up * config.jumpForce, ForceMode2D.Impulse);
            }
            else if (airJumpsLeft > 0)
            {
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
        if (wallCheck)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
        }
    }
}
