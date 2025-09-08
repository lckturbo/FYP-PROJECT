using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private float speed = 5.0f;
    private InputAction moveAction; // 左右移動

    private bool canMove = true;   // 移動可能かどうか

    //[SerializeField] private int cash = 10000;   // 所持金

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        moveAction.Enable();
    }

    void Update()
    {
        if (canMove)
        {
            var moveValue = moveAction.ReadValue<Vector2>();
            var move = new Vector3(moveValue.x, 0f, 0f) * speed * Time.deltaTime;
            transform.Translate(move);
        }
        var MoveValue = moveAction.ReadValue<Vector2>();
    }

    /// <summary>
    /// 外部からプレイヤーの移動可否を設定する
    /// </summary>
    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}
