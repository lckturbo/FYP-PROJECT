using UnityEngine;

public class Basket : MonoBehaviour
{
    private RectTransform rect;
    [SerializeField] private float moveSpeed = 800f;

    [Header("Dash Settings")]
    [SerializeField] private float dashMultiplier = 2f;  
   // [SerializeField] private float dashDuration = 0.15f;
    //[SerializeField] private float dashCooldown = 0.5f;

    [SerializeField] private RectTransform movementBounds;

    private bool isDashing = false;
    //private float dashTimer = 0f;
    //private float cooldownTimer = 0f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        MoveBasket();
        ClampPosition();
    }

    private void MoveBasket()
    {
        float currentSpeed = isDashing ? moveSpeed * dashMultiplier : moveSpeed;
        float moveX = Input.GetAxisRaw("Horizontal");
        rect.anchoredPosition += new Vector2(moveX * currentSpeed * Time.unscaledDeltaTime, 0);
    }

    private void ClampPosition()
    {
        float halfWidth = rect.rect.width / 2f;       
        float maxX = (950f / 2f) - halfWidth;          

        rect.anchoredPosition = new Vector2(
            Mathf.Clamp(rect.anchoredPosition.x, -maxX, maxX),
            rect.anchoredPosition.y
        );
    }
    
}
