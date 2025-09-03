using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private float speed = 5.0f;
    private bool canMove = true;   // ← 移動可能かどうか

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!canMove) return; // 会話中は入力を無視する

        // 移動量を計算
        float moveX = 0.0f;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            // 左へ移動
            moveX = -1.0f;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            // 右へ移動
            moveX = 1.0f;
        }

        // 移動させる
        transform.Translate(new Vector3(moveX, 0f, 0f) * speed * Time.deltaTime);
    }

    /// <summary>
    /// 外部からプレイヤーの移動可否を設定する
    /// </summary>
    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}
