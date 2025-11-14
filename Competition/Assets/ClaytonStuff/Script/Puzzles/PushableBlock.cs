using System.Collections;
using System.Xml;
using UnityEngine;
using static GameData;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PushableBlock : MonoBehaviour, IDataPersistence
{
    [SerializeField] private int id;

    [Header("Movement")]
    public float gridSize = 1f;
    public float moveSpeed = 6f;
    public LayerMask obstacleMask;

    [Header("Safety")]
    public float checkPadding = 0.05f;

    [Header("Grid Settings")]
    public Vector2 gridOffset = Vector2.zero;

    private bool isMoving = false;
    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 originalPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.drag = 1000f;
    }

    private void Start()
    {
        originalPosition = rb.position;

        // Handle possible overlap with player on scene load
        CheckInitialOverlap();
    }

    private void CheckInitialOverlap()
    {
        Collider2D hit = Physics2D.OverlapBox(rb.position, col.bounds.size, 0f, LayerMask.GetMask("Player"));
        if (hit)
        {
            Debug.Log($"[{name}] Player overlapping on load, resolving...");
            PlayerPush player = hit.GetComponent<PlayerPush>();
            if (player != null)
            {
                Vector2 dir = (rb.position - (Vector2)player.transform.position).normalized;
                TryPush(dir);
            }
        }
    }

    public void ResetToOriginal()
    {
        StopAllCoroutines();
        isMoving = false;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        rb.position = originalPosition;
        SnapToGrid();
    }

    public bool TryPush(Vector2 direction)
    {
        if (isMoving) return false;

        AudioManager.instance.PlaySFXAtPoint("cinderblockmove2-107628", transform.position);

        Vector2 dir = GetCardinal(direction);
        if (dir == Vector2.zero) return false;

        SnapToGrid();
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
        block.SnapToGrid();

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

        while ((rb.position - target).sqrMagnitude > 0.001f)
        {
            rb.position = Vector2.MoveTowards(rb.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        rb.position = target;
        rb.isKinematic = false;
        rb.velocity = Vector2.zero;
        isMoving = false;
    }

    private Vector2 GetCardinal(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x > 0 ? Vector2.right : Vector2.left;
        else if (Mathf.Abs(dir.y) > 0)
            return dir.y > 0 ? Vector2.up : Vector2.down;

        return Vector2.zero;
    }

    private void SnapToGrid()
    {
        rb.position = new Vector2(
            Mathf.Round((rb.position.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x,
            Mathf.Round((rb.position.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y
        );
    }

    public void LoadData(GameData data)
    {
        var blockData = data.pushableBlocks.Find(b => b.id == id);
        if (blockData != null)
        {
            rb.position = blockData.position;
            SnapToGrid();
        }
    }

    public void SaveData(ref GameData data)
    {
        var existing = data.pushableBlocks.Find(b => b.id == id);
        if (existing != null)
            existing.position = rb.position;
        else
            data.pushableBlocks.Add(new BlockSaveData(id, rb.position));
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    // Only react if colliding with the player
    //    if (collision.collider.GetComponent<PlayerPush>() == null)
    //        return;

    //    if (collision.contactCount == 0) return;
    //    ContactPoint2D contact = collision.GetContact(0);
    //    Vector2 dir = -contact.normal;

    //    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
    //        dir = dir.x > 0 ? Vector2.right : Vector2.left;
    //    else
    //        dir = dir.y > 0 ? Vector2.up : Vector2.down;

    //    TryPush(dir);
    //}
}
