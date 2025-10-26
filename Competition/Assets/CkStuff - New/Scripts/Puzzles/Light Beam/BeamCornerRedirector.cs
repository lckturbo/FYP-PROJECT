using UnityEngine;

[DisallowMultipleComponent]
public class BeamCornerRedirector : MonoBehaviour
{
    [Header("Port pairs in LOCAL space")]
    public Vector2 inputA_Local = Vector2.left;
    public Vector2 outputA_Local = Vector2.up;

    public Vector2 inputB_Local = Vector2.down;
    public Vector2 outputB_Local = Vector2.right;

    [Range(0.5f, 0.999f)]
    public float matchDot = 0.85f;

    [Header("Pass point")]
    public Transform passPoint;

    public bool TryGetOutput(Vector2 incomingWorldDir, out Vector2 outWorldDir)
    {
        Vector2 incLocal = transform.InverseTransformDirection(incomingWorldDir).normalized;

        if (Vector2.Dot(incLocal, -inputA_Local.normalized) >= matchDot)
        {
            outWorldDir = transform.TransformDirection(outputA_Local.normalized).normalized;
            return true;
        }
        if (Vector2.Dot(incLocal, -inputB_Local.normalized) >= matchDot)
        {
            outWorldDir = transform.TransformDirection(outputB_Local.normalized).normalized;
            return true;
        }

        outWorldDir = default;
        return false;
    }

    public Vector2 GetPassPoint()
    {
        if (passPoint) return passPoint.position;

        Collider2D col = GetComponent<Collider2D>();
        if (col) return col.bounds.center;

        return transform.position;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 p = transform.position;

        // Inputs (short arrows)
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 1f); // input A
        Gizmos.DrawLine(p, p + (Vector3)transform.TransformDirection(-inputA_Local.normalized) * 0.7f);
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 1f); // input B
        Gizmos.DrawLine(p, p + (Vector3)transform.TransformDirection(-inputB_Local.normalized) * 0.7f);

        // Outputs (long arrows)
        Gizmos.color = new Color(0.2f, 1f, 0.6f, 1f); // output A
        Gizmos.DrawLine(p, p + (Vector3)transform.TransformDirection(outputA_Local.normalized) * 0.9f);
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 1f); // output B
        Gizmos.DrawLine(p, p + (Vector3)transform.TransformDirection(outputB_Local.normalized) * 0.9f);

        // Pass point indicator
        Vector3 pass = GetPassPoint();
        Gizmos.color = Color.white;
        float r = 0.07f;
        Gizmos.DrawWireSphere(pass, r);
    }
#endif
}
