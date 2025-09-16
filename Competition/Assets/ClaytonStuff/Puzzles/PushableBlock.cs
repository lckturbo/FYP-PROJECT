using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PushableBlock : MonoBehaviour
{
    [Header("Movement")]
    public float gridSize = 1f;
    public float moveSpeed = 6f;
    public LayerMask obstacleMask;

    [Header("Safety")]
    public float checkPadding = 0.05f;

    private bool isMoving = false;
    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.drag = 1000f; // prevent sliding
    }

    public bool TryPush(Vector2 direction)
    {
        if (isMoving) return false;

        Vector2 dir = GetCardinal(direction);
        if (dir == Vector2.zero) return false;

        if (!CanPushChain(this, dir)) return false;

        DoPushChain(this, dir);
        return true;
    }

    private bool CanPushChain(PushableBlock block, Vector2 dir)
    {
        Bounds b = block.col.bounds;
        Vector2 size = new Vector2(b.size.x - checkPadding, b.size.y - checkPadding);

        RaycastHit2D hit = Physics2D.BoxCast(block.rb.position, size, 0f, dir, gridSize, obstacleMask);
        if (!hit.collider) return true;

        PushableBlock other = hit.collider.GetComponent<PushableBlock>();
        return other != null && CanPushChain(other, dir);
    }

    private void DoPushChain(PushableBlock block, Vector2 dir)
    {
        Bounds b = block.col.bounds;
        Vector2 size = new Vector2(b.size.x - checkPadding, b.size.y - checkPadding);

        RaycastHit2D hit = Physics2D.BoxCast(block.rb.position, size, 0f, dir, gridSize, obstacleMask);
        if (hit.collider)
        {
            PushableBlock other = hit.collider.GetComponent<PushableBlock>();
            if (other != null)
                DoPushChain(other, dir);
        }

        Vector2 target = block.rb.position + dir * gridSize;
        block.StartCoroutine(block.MoveTo(target));
    }

    private IEnumerator MoveTo(Vector2 target)
    {
        isMoving = true;

        rb.isKinematic = true;

        // Lock axis movement to cardinal direction only
        Vector2 start = transform.position;
        bool moveX = Mathf.Abs(target.x - start.x) > 0.01f;
        bool moveY = Mathf.Abs(target.y - start.y) > 0.01f;

        while ((Vector2)transform.position != target)
        {
            Vector2 pos = transform.position;

            if (moveX)
                pos.x = Mathf.MoveTowards(pos.x, target.x, moveSpeed * Time.deltaTime);
            if (moveY)
                pos.y = Mathf.MoveTowards(pos.y, target.y, moveSpeed * Time.deltaTime);

            transform.position = pos;

            yield return null;
        }

        // Snap to grid to avoid drift
        rb.position = new Vector2(
            Mathf.Round(transform.position.x / gridSize) * gridSize,
            Mathf.Round(transform.position.y / gridSize) * gridSize
        );

        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMoving)
        {
            StopAllCoroutines();
            rb.velocity = Vector2.zero;

            // Snap to grid immediately on collision
            rb.position = new Vector2(
                Mathf.Round(rb.position.x / gridSize) * gridSize,
                Mathf.Round(rb.position.y / gridSize) * gridSize
            );

            isMoving = false;
        }
    }
}
