using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class BeamSource : MonoBehaviour
{
    [Header("Beam Settings")]
    public Vector2 initialDirection = Vector2.right;
    [Min(0.1f)] public float maxDistance = 6.5f;
    [Min(0)] public int maxBounces = 5;
    public LayerMask beamMask; // Layer(s): Beam

    [Header("Visuals")]
    [Min(0f)] public float beamWidth = 0.06f;
    public bool animateTexture = true;
    public float scrollSpeed = 2f;

    private LineRenderer lr;
    private readonly List<Vector3> points = new List<Vector3>();
    private Vector2 dir;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.startWidth = lr.endWidth = beamWidth;
        dir = initialDirection.normalized;
    }

    private void Update()
    {
        CastAndRenderBeam(transform.position, dir);

        if (animateTexture && lr.material && lr.material.mainTexture != null)
        {
            Vector2 offset = lr.material.mainTextureOffset;
            offset.x -= scrollSpeed * Time.deltaTime;
            lr.material.mainTextureOffset = offset;

            float totalLen = ComputeTotalLength(points);
            Vector2 tiling = lr.material.mainTextureScale;
            tiling.x = Mathf.Max(1f, totalLen * 1.0f);
            lr.material.mainTextureScale = tiling;
        }
    }

    private void CastAndRenderBeam(Vector2 startPos, Vector2 startDir)
    {
        points.Clear();
        points.Add(startPos);

        Vector2 curPos = startPos;
        Vector2 curDir = startDir.normalized;

        for (int i = 0; i <= maxBounces; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(curPos, curDir, maxDistance, beamMask);

            if (!hit.collider)
            {
                points.Add(curPos + curDir * maxDistance);
                break;
            }

            points.Add(hit.point);

            BeamCornerRedirector corner = hit.collider.GetComponent<BeamCornerRedirector>();
            if (corner && corner.TryGetOutput(curDir, out Vector2 newDir))
            {
                Vector2 pass = corner.GetPassPoint();
                points.Add(pass);
                curDir = newDir.normalized;
                curPos = pass + curDir * 0.02f;
                continue;
            }

            BeamReceiver receiver = hit.collider.GetComponent<BeamReceiver>();
            if (receiver)
            {
                receiver.OnBeamStay();
                break;
            }

            break;
        }

        lr.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            lr.SetPosition(i, points[i]);
        }
    }

    private float ComputeTotalLength(List<Vector3> pts)
    {
        float total = 0f;
        for (int i = 1; i < pts.Count; i++)
            total += Vector3.Distance(pts[i - 1], pts[i]);
        return total;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector2 startPos = transform.position;
        Vector2 gizDir = (Application.isPlaying ? dir : initialDirection).normalized;
        float max = Mathf.Max(maxDistance, 0.01f);

        Gizmos.color = Color.yellow;
        RaycastHit2D hit = Physics2D.Raycast(startPos, gizDir, max, beamMask);
        if (hit)
        {
            Gizmos.DrawLine(startPos, hit.point);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.05f);
        }
        else
        {
            Gizmos.DrawLine(startPos, startPos + gizDir * max);
        }
    }
#endif
}
