using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class BlockFocusAnchor : MonoBehaviour
{
    [Tooltip("If empty, will search up the hierarchy.")]
    public Collider2D sourceCollider;
    public Renderer sourceRenderer;

    private void Reset()
    {
        if (!sourceCollider) sourceCollider = GetComponentInParent<Collider2D>();
        if (!sourceRenderer) sourceRenderer = GetComponentInParent<Renderer>();
    }

    private void LateUpdate()
    {
        Vector3 center = transform.position;
        if (sourceCollider) center = sourceCollider.bounds.center;
        else if (sourceRenderer) center = sourceRenderer.bounds.center; // world-space bounds
        transform.position = center;
    }
}
