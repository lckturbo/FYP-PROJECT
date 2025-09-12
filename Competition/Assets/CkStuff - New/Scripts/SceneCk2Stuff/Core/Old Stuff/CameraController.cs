using UnityEngine;

[RequireComponent(typeof(Camera))]
public class HKCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Tooltip("Offset from target (usually z = -10 for 2D).")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Dead-Zone (camera won't move while player stays inside)")]
    public Vector2 deadZoneSize = new Vector2(3.0f, 1.5f);

    [Header("Look-Ahead")]
    public float lookAheadDistance = 2.0f;
    public float lookAheadMoveThreshold = 0.1f;
    public float lookAheadReturnSpeed = 2.0f;

    [Header("Smoothing")]
    public float smoothTimeX = 0.15f;
    public float smoothTimeY = 0.2f;

    [Header("Level Bounds (world space)")]
    public Vector2 minBounds = new Vector2(-50f, -10f);
    public Vector2 maxBounds = new Vector2(50f, 10f);

    private Camera cam;
    private Vector3 currentVelocity;
    private float currentLookAheadX;
    private float targetLookAheadX;

    private Vector3 lastTargetPos;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        lastTargetPos = target ? target.position : Vector3.zero;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float vx = (target.position.x - lastTargetPos.x) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastTargetPos = target.position;

        if (Mathf.Abs(vx) > lookAheadMoveThreshold)
            targetLookAheadX = Mathf.Sign(vx) * lookAheadDistance;
        else
            targetLookAheadX = 0f;

        currentLookAheadX = Mathf.MoveTowards(currentLookAheadX, targetLookAheadX, lookAheadReturnSpeed * Time.deltaTime);

        Vector3 desired = target.position + offset;
        desired.x += currentLookAheadX;

        Vector3 camPos = transform.position;
        Rect dz = GetDeadZoneRect(camPos);

        if (desired.x < dz.xMin) camPos.x += desired.x - dz.xMin;
        else if (desired.x > dz.xMax) camPos.x += desired.x - dz.xMax;

        if (desired.y < dz.yMin) camPos.y += desired.y - dz.yMin;
        else if (desired.y > dz.yMax) camPos.y += desired.y - dz.yMax;

        float newX = Mathf.SmoothDamp(transform.position.x, camPos.x, ref currentVelocity.x, smoothTimeX);
        float newY = Mathf.SmoothDamp(transform.position.y, camPos.y, ref currentVelocity.y, smoothTimeY);

        Vector3 finalPos = new Vector3(newX, newY, offset.z);

        float minX = minBounds.x + halfWidth;
        float maxX = maxBounds.x - halfWidth;
        float minY = minBounds.y + halfHeight;
        float maxY = maxBounds.y - halfHeight;

        if (minX > maxX) { float mid = (minBounds.x + maxBounds.x) * 0.5f; minX = maxX = mid; }
        if (minY > maxY) { float mid = (minBounds.y + maxBounds.y) * 0.5f; minY = maxY = mid; }

        finalPos.x = Mathf.Clamp(finalPos.x, minX, maxX);
        finalPos.y = Mathf.Clamp(finalPos.y, minY, maxY);

        transform.position = finalPos;
    }

    private Rect GetDeadZoneRect(Vector3 cameraCenter)
    {
        float w = Mathf.Max(0.01f, deadZoneSize.x);
        float h = Mathf.Max(0.01f, deadZoneSize.y);
        return new Rect(cameraCenter.x - w * 0.5f, cameraCenter.y - h * 0.5f, w, h);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Gizmos.color = Color.yellow;
        Vector3 bCenter = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0f);
        Vector3 bSize = new Vector3(Mathf.Abs(maxBounds.x - minBounds.x), Mathf.Abs(maxBounds.y - minBounds.y), 0f);
        Gizmos.DrawWireCube(bCenter, bSize);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(halfWidth * 2f, halfHeight * 2f, 0f));

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadZoneSize.x, deadZoneSize.y, 0f));
    }
#endif
}
