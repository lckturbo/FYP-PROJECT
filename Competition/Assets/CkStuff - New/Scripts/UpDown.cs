using UnityEngine;

public class UpDown : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveAmount;
    public float speed = 4f;

    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.localPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * moveAmount;
        transform.localPosition = _startPos + new Vector3(0f, offset, 0f);
    }
}
