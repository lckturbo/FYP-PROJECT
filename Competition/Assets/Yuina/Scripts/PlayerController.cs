using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private float speed = 5.0f;
    private bool canMove = true;   // �� �ړ��\���ǂ���

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (!canMove) return; // ��b���͓��͂𖳎�����

        // �ړ��ʂ��v�Z
        float moveX = 0.0f;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            // ���ֈړ�
            moveX = -1.0f;
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            // �E�ֈړ�
            moveX = 1.0f;
        }

        // �ړ�������
        transform.Translate(new Vector3(moveX, 0f, 0f) * speed * Time.deltaTime);
    }

    /// <summary>
    /// �O������v���C���[�̈ړ��ۂ�ݒ肷��
    /// </summary>
    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}
