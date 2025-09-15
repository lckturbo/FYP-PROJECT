using UnityEngine;

public class NewCameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    [Range(0.02f, 0.5f)] public float smoothTime = 0.15f;
    private Vector3 _vel;

    void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);
    }
}
