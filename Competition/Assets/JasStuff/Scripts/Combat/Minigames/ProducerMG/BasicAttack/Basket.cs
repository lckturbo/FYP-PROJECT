using UnityEngine;

public class Basket : MonoBehaviour
{
    private RectTransform rect;
    [SerializeField] private float moveSpeed = 800f;
    [SerializeField] private RectTransform movementBounds; 

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); 
        rect.anchoredPosition += new Vector2(moveX * moveSpeed * Time.deltaTime, 0);

        if (movementBounds)
        {
            float halfWidth = rect.rect.width / 2f;
            float maxX = movementBounds.rect.width / 2f - halfWidth;
            rect.anchoredPosition = new Vector2(
                Mathf.Clamp(rect.anchoredPosition.x, -maxX, maxX),
                rect.anchoredPosition.y
            );
        }
    }
}

