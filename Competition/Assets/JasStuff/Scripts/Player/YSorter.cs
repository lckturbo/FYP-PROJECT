using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSorter : MonoBehaviour
{
    private SpriteRenderer sr;
    private Vector2 lastMoveDir;
    private bool isLeader;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        int baseOrder = -(int)(transform.position.y * 100);
        sr.sortingOrder = baseOrder;

        if (isLeader)
        {
            if (lastMoveDir.y > 0.1f)
            {
                sr.sortingOrder -= 10;
            }
            else
            {
                sr.sortingOrder += 10;
            }
        }
    }

    public void SetMoveDirection(Vector2 dir)
    {
        lastMoveDir = dir;
    }

    public void SetLeader(bool leader)
    {
        isLeader = leader;
    }
}
