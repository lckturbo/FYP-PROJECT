using UnityEngine;

public class Paper : MonoBehaviour
{
    private RectTransform rect;
    private RectTransform basket;
    private Deadline game;
    private float speed;
    private bool caught = false;

    public void Init(Deadline g, float s, RectTransform target)
    {
        game = g;
        speed = s;
        basket = target;
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (caught) return;

        rect.anchoredPosition -= new Vector2(0, speed * Time.unscaledDeltaTime);

        if (RectOverlaps(rect, basket))
        {
            caught = true;
            game.OnCatch();
            Destroy(gameObject);
            return;
        }

        if (rect.anchoredPosition.y < -Screen.height)
        {
            game.OnMiss();
            Destroy(gameObject);
        }
    }
    private bool RectOverlaps(RectTransform a, RectTransform b)
    {
        if (!a || !b) return false;

        Vector3[] aCorners = new Vector3[4];
        Vector3[] bCorners = new Vector3[4];
        a.GetWorldCorners(aCorners);
        b.GetWorldCorners(bCorners);

        Rect rectA = new Rect(aCorners[0].x, aCorners[0].y,
                              aCorners[2].x - aCorners[0].x,
                              aCorners[2].y - aCorners[0].y);

        Rect rectB = new Rect(bCorners[0].x, bCorners[0].y,
                              bCorners[2].x - bCorners[0].x,
                              bCorners[2].y - bCorners[0].y);

        return rectA.Overlaps(rectB);
    }

    public void Catch()
    {
        game.OnCatch();
        Destroy(gameObject);
    }
}
