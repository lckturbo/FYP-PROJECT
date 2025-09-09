using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class NewPlayerMovement : MonoBehaviour
{
    [Header("Config")]
    public NewMovementConfig config;   // assign a NewMovementConfig asset in Inspector

    private Rigidbody2D rb;
    private Animator animator;

    private PlayerInput playerInput;
    private InputAction moveAction;

    // working vector from input
    private Vector3 change;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        playerInput = GetComponent<PlayerInput>();
        // Expect an action named "Move" (Value, Vector2) in your Input Actions asset
        moveAction = playerInput.actions["Move"];
    }

    void Update()
    {
        // Read input from Input System
        Vector2 input = moveAction.ReadValue<Vector2>();
        change.x = input.x;
        change.y = input.y;

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // Preserve original behavior: move at config.walkSpeed; allow diagonal speed-up (unchanged)
        if (change != Vector3.zero && config != null)
        {
            Vector3 target = transform.position + change * config.walkSpeed * Time.fixedDeltaTime;
            rb.MovePosition(target);
        }
    }

    private void UpdateAnimation()
    {
        if (!animator) return;

        if (change != Vector3.zero)
        {
            animator.SetFloat("moveX", change.x);
            animator.SetFloat("moveY", change.y);
            animator.SetBool("moving", true);
        }
        else
        {
            animator.SetBool("moving", false);
        }
    }

    // Allow switcher to apply new movement config
    public void ApplyMovementConfig(NewMovementConfig newCfg)
    {
        config = newCfg;
        // If you cache derived values from config, recompute them here.
    }
}
