using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Floater : MonoBehaviour
{
    [HideInInspector] public RectTransform rt;
    [HideInInspector] public CanvasGroup cg;

    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public float rotationSpeedDeg;
    [HideInInspector] public float radius;
    [HideInInspector] public float lifetime;
    [HideInInspector] public float fadeOutSeconds;

    private float _age;

    public void Init(Vector2 startPos, Vector2 initialVel, float rotDegPerSec, float rad, float life, float fade)
    {
        if (!rt) rt = (RectTransform)transform;
        if (!cg) cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();

        rt.anchoredPosition = startPos;
        velocity = initialVel;
        rotationSpeedDeg = rotDegPerSec;
        radius = Mathf.Max(2f, rad);
        lifetime = Mathf.Max(0.1f, life);
        fadeOutSeconds = Mathf.Max(0.05f, fade);
        _age = 0f;
        cg.alpha = 1f;
    }

    // Called by manager each frame
    public void Step(float dt)
    {
        // integrate position + rotation
        rt.anchoredPosition += velocity * dt;
        rt.Rotate(0f, 0f, rotationSpeedDeg * dt);

        // lifetime handling
        _age += dt;
        if (_age > lifetime)
        {
            float t = (_age - lifetime) / fadeOutSeconds;
            cg.alpha = 1f - Mathf.Clamp01(t);
        }
    }

    public bool IsFullyFaded()
    {
        return _age >= (lifetime + fadeOutSeconds);
    }
}
