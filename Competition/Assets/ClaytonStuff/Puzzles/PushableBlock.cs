using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PushableBlock : MonoBehaviour
{
    [Header("Movement")]
    public float gridSize = 1f;            // size of one step / cell
    public float moveSpeed = 6f;           // higher = faster (units/sec)
    public LayerMask obstacleMask;         // layers that block movement (walls, other blocks)

    [Header("Safety")]
    public float checkPadding = 0.05f;     // small padding for overlap checks

    private bool isMoving = false;
    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // block should use Kinematic so we control it
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    /// <summary>
    /// Attempt to push the block in cardinal direction (Vector2.up/right/left/down).
    /// Returns true if push started.
    /// </summary>
    public bool TryPush(Vector2 direction)
    {
        if (isMoving) return false;

        Vector2 dir = GetCardinal(direction);
        if (dir == Vector2.zero) return false;

        // bounds of the block
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x - checkPadding, b.size.y - checkPadding);

        // cast forward 1 grid cell
        RaycastHit2D hit = Physics2D.BoxCast(rb.position, size, 0f, dir, gridSize, obstacleMask);

        Vector2 target;
        if (hit.collider != null)
        {
            // check if it's another pushable block
            PushableBlock otherBlock = hit.collider.GetComponent<PushableBlock>();
            if (otherBlock != null)
            {
                // try to push that block forward
                bool pushed = otherBlock.TryPush(dir);
                if (!pushed) return false; // other block couldn't move, so we can't either

                // if the other block can move, we also move forward 1 cell
                target = rb.position + dir * gridSize;
            }
            else
            {
                // obstacle that cannot move, stop just before it
                float distance = hit.distance - checkPadding;
                if (distance <= 0f) return false;
                target = rb.position + dir * distance;
            }
        }
        else
        {
            // free space, move exactly 1 grid cell
            target = rb.position + dir * gridSize;
        }

        StartCoroutine(MoveTo(target));
        return true;
    }

    private IEnumerator MoveTo(Vector2 target)
    {
        isMoving = true;
        while ((Vector2)transform.position != target)
        {
            Vector2 newPos = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            rb.MovePosition(newPos);
            yield return null;
        }

        rb.MovePosition(target); // snap to exact
        isMoving = false;
    }

    private Vector2 GetCardinal(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? Vector2.right : Vector2.left;
        else if (Mathf.Abs(dir.y) > 0.01f)
            return dir.y > 0 ? Vector2.up : Vector2.down;
        return Vector2.zero;
    }

    // Debug draw
    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, col.bounds.size);
    }
}
