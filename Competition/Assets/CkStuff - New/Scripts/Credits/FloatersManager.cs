using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FloaterManager : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private RectTransform spawnRect;
    [SerializeField] private List<GameObject> prefabs;

    [Header("Counts")]
    [SerializeField] private int initialSpawn = 4;
    [SerializeField] private int maxAlive = 8;

    [Header("Spawn Timing (seconds)")]
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(0.6f, 1.4f);

    [Header("Random Ranges")]
    [SerializeField] private Vector2 speedRange = new Vector2(60f, 160f);
    [SerializeField] private Vector2 sizeRange = new Vector2(0.7f, 1.4f);
    [SerializeField] private Vector2 rotSpeedRange = new Vector2(-60f, 60f);
    [SerializeField] private Vector2 lifeRange = new Vector2(4f, 9f);
    [SerializeField] private float fadeOutSeconds = 0.8f;
    [SerializeField] private float minSpawnSeparation = 40f;

    [Header("Collision")]
    [SerializeField] private float bounciness = 1.0f;
    [SerializeField] private float edgePadding = 24f;

    private readonly List<Floater> _particles = new List<Floater>();
    private RectTransform _rt;
    private float _spawnTimer;

    private void Awake()
    {
        _rt = (RectTransform)transform;
        if (!spawnRect) spawnRect = _rt;
        if (prefabs == null) prefabs = new List<GameObject>();
        _spawnTimer = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
    }

    private void Start()
    {
        // initial batch
        for (int i = 0; i < initialSpawn && _particles.Count < maxAlive; i++)
            TrySpawnOne();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // step particles
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            if (p == null) { _particles.RemoveAt(i); continue; }

            p.Step(dt);
            KeepInsideBounds(p);
        }

        // collide/bounce
        CollideAll();

        // cull faded
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            if (_particles[i].IsFullyFaded())
            {
                Destroy(_particles[i].gameObject);
                _particles.RemoveAt(i);
            }
        }

        // spawn new over time
        if (_particles.Count < maxAlive && prefabs.Count > 0)
        {
            _spawnTimer -= dt;
            if (_spawnTimer <= 0f)
            {
                if (TrySpawnOne())
                    _spawnTimer = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
                else
                    _spawnTimer = 0.2f; // retry soon if overlap blocked us
            }
        }
    }

    private bool TrySpawnOne()
    {
        if (prefabs.Count == 0) return false;

        var prefab = prefabs[Random.Range(0, prefabs.Count)];
        var go = Instantiate(prefab, spawnRect);
        var p = go.GetComponent<Floater>();
        if (!p) p = go.AddComponent<Floater>();
        if (!p.rt) p.rt = (RectTransform)go.transform;

        // random size
        float scale = Random.Range(sizeRange.x, sizeRange.y);
        p.rt.localScale = Vector3.one * scale;

        // rough radius from width/height (use half of min dimension * 0.45)
        Vector2 sz = p.rt.sizeDelta * scale;
        float radius = Mathf.Max(8f, Mathf.Min(sz.x, sz.y) * 0.45f);

        // found a non-overlapping start
        Vector2 start;
        if (!FindSpawnPosition(radius, out start))
        {
            Destroy(go);
            return false;
        }

        // random direction + speed
        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir.sqrMagnitude < 0.1f) dir = Vector2.right;
        float speed = Random.Range(speedRange.x, speedRange.y);
        Vector2 vel = dir * speed;

        // rotation speed + lifetime
        float rot = Random.Range(rotSpeedRange.x, rotSpeedRange.y);
        float life = Random.Range(lifeRange.x, lifeRange.y);

        p.Init(start, vel, rot, radius, life, fadeOutSeconds);

        // optional random tint if Image present
        var img = go.GetComponent<Image>();
        if (img)
        {
            img.raycastTarget = false;
            // subtle brightness variety
            float v = Random.Range(0.9f, 1.1f);
            img.color = new Color(img.color.r * v, img.color.g * v, img.color.b * v, img.color.a);
        }

        _particles.Add(p);
        return true;
    }

    private bool FindSpawnPosition(float radius, out Vector2 pos)
    {
        // spawn inside rect with padding
        Rect r = spawnRect.rect;
        float left = r.xMin + edgePadding + radius;
        float right = r.xMax - edgePadding - radius;
        float bottom = r.yMin + edgePadding + radius;
        float top = r.yMax - edgePadding - radius;

        const int attempts = 25;
        for (int i = 0; i < attempts; i++)
        {
            float x = Random.Range(left, right);
            float y = Random.Range(bottom, top);
            Vector2 candidate = new Vector2(x, y);

            bool overlaps = false;
            for (int j = 0; j < _particles.Count; j++)
            {
                var pj = _particles[j];
                float minDist = pj.radius + radius + minSpawnSeparation * 0.5f;
                if ((pj.rt.anchoredPosition - candidate).sqrMagnitude < (minDist * minDist))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                pos = candidate;
                return true;
            }
        }

        pos = Vector2.zero;
        return false;
    }

    private void KeepInsideBounds(Floater p)
    {
        Rect r = spawnRect.rect;

        // local pos inside panel rect space
        Vector2 pos = p.rt.anchoredPosition;

        float left = r.xMin + edgePadding + p.radius;
        float right = r.xMax - edgePadding - p.radius;
        float bottom = r.yMin + edgePadding + p.radius;
        float top = r.yMax - edgePadding - p.radius;

        // reflect off edges
        if (pos.x < left)
        {
            pos.x = left;
            p.velocity.x = Mathf.Abs(p.velocity.x) * bounciness;
        }
        else if (pos.x > right)
        {
            pos.x = right;
            p.velocity.x = -Mathf.Abs(p.velocity.x) * bounciness;
        }

        if (pos.y < bottom)
        {
            pos.y = bottom;
            p.velocity.y = Mathf.Abs(p.velocity.y) * bounciness;
        }
        else if (pos.y > top)
        {
            pos.y = top;
            p.velocity.y = -Mathf.Abs(p.velocity.y) * bounciness;
        }

        p.rt.anchoredPosition = pos;
    }

    private void CollideAll()
    {
        int n = _particles.Count;
        for (int i = 0; i < n; i++)
        {
            var a = _particles[i];
            if (!a) continue;

            for (int j = i + 1; j < n; j++)
            {
                var b = _particles[j];
                if (!b) continue;

                Vector2 pa = a.rt.anchoredPosition;
                Vector2 pb = b.rt.anchoredPosition;
                Vector2 d = pb - pa;
                float dist = d.magnitude;
                float minDist = a.radius + b.radius;

                if (dist < minDist && dist > 0.0001f)
                {
                    // push them apart equally
                    float overlap = (minDist - dist);
                    Vector2 nrm = d / dist;
                    pa -= nrm * (overlap * 0.5f);
                    pb += nrm * (overlap * 0.5f);
                    a.rt.anchoredPosition = pa;
                    b.rt.anchoredPosition = pb;

                    // elastic collision (equal masses): swap normal components
                    Vector2 va = a.velocity;
                    Vector2 vb = b.velocity;

                    float vaN = Vector2.Dot(va, nrm);
                    float vbN = Vector2.Dot(vb, nrm);

                    Vector2 vaT = va - nrm * vaN; // tangential components unchanged
                    Vector2 vbT = vb - nrm * vbN;

                    float newVaN = vbN;
                    float newVbN = vaN;

                    a.velocity = (vaT + nrm * newVaN) * bounciness;
                    b.velocity = (vbT + nrm * newVbN) * bounciness;
                }
            }
        }
    }
}
