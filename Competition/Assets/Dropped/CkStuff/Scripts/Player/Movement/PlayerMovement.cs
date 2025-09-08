using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private MovementConfig config;

    [Header("Checks (raycasts vs Terrain)")]
    [SerializeField] private Transform groundCheck;      // bottom-center of player
    [SerializeField] private Transform wallCheckLeft;    // chest/hip height
    [SerializeField] private Transform wallCheckRight;   // chest/hip height

    [SerializeField] private LayerMask terrainMask;      // your Tilemap layer
    [SerializeField] private float groundProbe = 0.15f;  // ray length down
    [SerializeField] private float wallProbe = 0.15f;  // ray length sideways
    [SerializeField, Range(0f, 1f)] private float groundNormalMin = 0.6f;
    [SerializeField, Range(0f, 1f)] private float wallNormalMin = 0.9f;

    [Header("Debug/HUD")]
    [SerializeField] private bool drawDebugRays = true;
    [SerializeField] private float debugRayTime = 0.05f; // seconds visible
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

    // ---------- Input System callbacks ----------
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
        if (ctx.performed) dashPressed = true;
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
        else coyoteTimer -= Time.deltaTime;

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

        // ---------- Collision checks via raycasts ----------
        // Ground
        RaycastHit2D gHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundProbe, terrainMask);
        isGrounded = gHit && gHit.normal.y >= groundNormalMin;

        // Walls
        isTouchingWallLeft = false;
        isTouchingWallRight = false;

        RaycastHit2D wl = Physics2D.Raycast(wallCheckLeft.position, Vector2.left, wallProbe, terrainMask);
        if (wl && Mathf.Abs(wl.normal.x) >= wallNormalMin && Mathf.Abs(wl.normal.y) < 0.5f)
            isTouchingWallLeft = true;

        RaycastHit2D wr = Physics2D.Raycast(wallCheckRight.position, Vector2.right, wallProbe, terrainMask);
        if (wr && Mathf.Abs(wr.normal.x) >= wallNormalMin && Mathf.Abs(wr.normal.y) < 0.5f)
            isTouchingWallRight = true;

        // Optional: if firmly grounded, ignore wall contact (prevents corner glue)
        if (isGrounded && rb.velocity.y <= 0.05f)
        {
            isTouchingWallLeft = false;
            isTouchingWallRight = false;
        }

        // ---------- DEBUG DRAW ----------
        if (drawDebugRays)
        {
            // rays
            Debug.DrawRay(groundCheck.position, Vector2.down * groundProbe, Color.green, debugRayTime);
            Debug.DrawRay(wallCheckLeft.position, Vector2.left * wallProbe, Color.red, debugRayTime);
            Debug.DrawRay(wallCheckRight.position, Vector2.right * wallProbe, Color.blue, debugRayTime);

            // normals (yellow) when we hit something
            if (gHit) Debug.DrawRay(gHit.point, gHit.normal * 0.3f, Color.yellow, debugRayTime);
            if (wl) Debug.DrawRay(wl.point, wl.normal * 0.3f, Color.yellow, debugRayTime);
            if (wr) Debug.DrawRay(wr.point, wr.normal * 0.3f, Color.yellow, debugRayTime);
        }

        bool touchingWall = isTouchingWallLeft || isTouchingWallRight;

        // ---------- Dashing ----------
        if (isDashing)
        {
            rb.velocity = new Vector2(dashDir * config.dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) isDashing = false;
            return;
        }

        // ---------- Horizontal move ----------
        float targetX = inputX * config.moveSpeed;
        float lerp = isGrounded ? 1f : config.airControlMultiplier;
        rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x, targetX, lerp), rb.velocity.y);

        // ---------- Wall slide ----------
        wallSliding = false;
        if (config.enableWallMoves && touchingWall && !isGrounded && rb.velocity.y < 0f)
        {
            wallSliding = true;

            if (rb.velocity.y < -config.wallSlideSpeed)
                rb.velocity = new Vector2(rb.velocity.x, -config.wallSlideSpeed);

            bool intoLeft = isTouchingWallLeft && rb.velocity.x < 0f;
            bool intoRight = isTouchingWallRight && rb.velocity.x > 0f;
            if (intoLeft || intoRight)
                rb.velocity = new Vector2(rb.velocity.x * 0.2f, rb.velocity.y);
        }

        // ---------- Prevent glue when pushing into wall ----------
        bool pushLeft = isTouchingWallLeft && rb.velocity.x < 0f;
        bool pushRight = isTouchingWallRight && rb.velocity.x > 0f;
        if (pushLeft || pushRight)
            rb.velocity = new Vector2(0f, rb.velocity.y);

        // ---------- Jumps ----------
        if (jumpBufferTimer > 0f)
        {
            if (config.enableWallMoves && touchingWall && !isGrounded)
            {
                // Wall jump (do NOT refill air jumps here)
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

    // Non-play-mode gizmos: show probe lengths
    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundProbe);
        }
        if (wallCheckLeft)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(wallCheckLeft.position, wallCheckLeft.position + Vector3.left * wallProbe);
        }
        if (wallCheckRight)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheckRight.position, wallCheckRight.position + Vector3.right * wallProbe);
        }
    }

    // HUD
    void OnGUI()
    {
        if (!showHud) return;
        GUIStyle style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 12 };

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
