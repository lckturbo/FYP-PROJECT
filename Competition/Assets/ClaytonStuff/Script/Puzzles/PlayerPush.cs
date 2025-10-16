using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerPush : MonoBehaviour
{
    [Header("Push Settings")]
    public float interactRange = 0.6f;     // distance to check for blocks
    public LayerMask pushableLayer;        // layer for pushable blocks
    public Key pushKey = Key.E;            // manual push key

    private Animator animator;
    private PlayerInput playerInput;
    private InputAction pushAction;
    private Rigidbody2D rb;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (playerInput != null)
            pushAction = playerInput.actions["Interaction"];
    }

    private void Update()
    {
        // Manual push using keyboard (if InputSystem action not configured)
        if (Keyboard.current != null && Keyboard.current[pushKey].wasPressedThisFrame)
        {
            TryManualPush();
        }
    }

    private void TryManualPush()
    {
        Vector2 facing = GetFacingFromAnimator();
        if (facing == Vector2.zero) return;

        Vector2 origin = (Vector2)transform.position + facing * interactRange;
        Collider2D hit = Physics2D.OverlapCircle(origin, 0.3f, pushableLayer);
        if (hit)
        {
            var pushable = hit.GetComponent<PushableBlock>();
            if (pushable != null)
            {
                bool pushed = pushable.TryPush(facing);
                if (pushed)
                    Debug.Log($"Pushed {pushable.name} manually in {facing}");
            }
        }
    }

    private Vector2 GetFacingFromAnimator()
    {
        if (animator == null) return Vector2.right;
        float mx = animator.GetFloat("moveX");
        float my = animator.GetFloat("moveY");

        if (Mathf.Abs(mx) > Mathf.Abs(my))
            return mx > 0 ? Vector2.right : Vector2.left;
        if (Mathf.Abs(my) > 0.01f)
            return my > 0 ? Vector2.up : Vector2.down;

        return Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if collided with a pushable block
        var pushable = collision.collider.GetComponent<PushableBlock>();
        if (pushable == null) return;

        // Get the first contact point
        if (collision.contactCount == 0) return;
        ContactPoint2D contact = collision.GetContact(0);

        // Normal points from block ? player, so invert for push direction
        Vector2 dir = -contact.normal;

        // Snap to cardinal direction
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            dir = dir.x > 0 ? Vector2.right : Vector2.left;
        else
            dir = dir.y > 0 ? Vector2.up : Vector2.down;

        Debug.Log($"Collision push on {pushable.name} in {dir}");
        pushable.TryPush(dir);
    }
}
