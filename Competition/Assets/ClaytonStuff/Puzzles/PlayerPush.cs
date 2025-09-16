using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PlayerPush : MonoBehaviour
{
    [Header("Push")]
    public float interactRange = 0.6f;         // distance to check for blocks
    public LayerMask pushableLayer;            // layer for pushable blocks
    public Key pushKey = Key.E;                // manual push key (InputSystem key)

    private Animator animator;
    private PlayerInput playerInput;
    private InputAction pushAction;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
            pushAction = playerInput.actions["Interact"]; // optional if you have an action named Interact
    }

    private void Update()
    {
        // Manual push using keyboard key E if InputSystem action not configured
        if (Keyboard.current != null && Keyboard.current[pushKey].wasPressedThisFrame)
        {
            TryManualPush();
        }

        // auto-push while moving into block: optional behavior (uncomment to enable)
        //AutoPushOnCollision();
    }

    private void TryManualPush()
    {
        Vector2 facing = GetFacingFromAnimator();
        if (facing == Vector2.zero) return;

        Vector2 origin = (Vector2)transform.position + facing * interactRange;
        Collider2D hit = Physics2D.OverlapCircle(origin, 0.3f, pushableLayer);
        if (hit != null)
        {
            var pushable = hit.GetComponent<PushableBlock>();
            if (pushable != null)
            {
                bool pushed = pushable.TryPush(facing);
                // optionally play push animation/sound when pushed == true
            }
        }
    }

    /// <summary>
    /// Determines player's facing direction via animator parameters moveX/moveY.
    /// </summary>
    private Vector2 GetFacingFromAnimator()
    {
        if (animator == null) return Vector2.right;
        float mx = animator.GetFloat("moveX");
        float my = animator.GetFloat("moveY");

        if (Mathf.Abs(mx) > Mathf.Abs(my))
            return mx > 0 ? Vector2.right : Vector2.left;
        if (Mathf.Abs(my) > 0.01f)
            return my > 0 ? Vector2.up : Vector2.down;
        // fallback: use last known direction stored in animator (you can add a lastFacing parameter)
        return Vector2.zero;
    }

    // Optional: auto-push when walking into a block (useful for sokoban-like feel)
    // To enable: call this from FixedUpdate when movement happens OR use collision callbacks.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var pushable = collision.collider.GetComponent<PushableBlock>();
        if (pushable == null) return;

        // Get the first contact point
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;
        // normal points *from the block towards the player*

        // Flip it so it becomes the push direction (player pushes block away)
        Vector2 dir = -normal;

        // Snap to cardinal directions
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            dir = dir.x > 0 ? Vector2.right : Vector2.left;
        else
            dir = dir.y > 0 ? Vector2.up : Vector2.down;

        Debug.Log($"Pushing block {pushable.name} in {dir}");
        pushable.TryPush(dir);

    }


}
