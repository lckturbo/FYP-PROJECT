using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralNoiseTilemapGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundMap;
    [SerializeField] private TileBase solidTile;

    [Header("Map Size (tiles)")]
    [SerializeField] private int mapWidth = 120;
    [SerializeField] private int mapHeight = 70;
    [SerializeField] private bool centerAtOrigin = true;

    [Header("Seed & Noise (Fractal Perlin)")]
    [SerializeField] private string worldSeed = "";
    [SerializeField] private int noiseOctaves = 3;
    [SerializeField] private float noiseScale = 5f;
    [SerializeField] private float persistence = 0.7f;
    [SerializeField] private float lacunarity = 2.0f;
    [SerializeField] private float noiseThreshold = 0.5f;
    [SerializeField] private Vector2 noiseOffset = Vector2.zero;

    [Header("Post Process")]
    [SerializeField] private bool smoothOnce = true;        // cellular automata style smoothing
    [SerializeField] private int minRegionArea = 30;        // remove or connect tiny pockets
    [SerializeField] private int corridorHalfWidth = 1;     // 1 => ~2–3 tiles wide corridors
    [SerializeField] private int corridorHalfHeight = 1;    // 1 => ~2–3 tiles high
    [SerializeField] private bool connectAllRegions = true; // carve tunnels to main region

    [Header("Runtime")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool clearBeforeGenerate = true;
    [SerializeField] private bool randomizeSeedOnPlay = false;

    System.Random seededRnd;
    int seedInt;

    bool[,] solid; // true = solid ground, false = air/walkable

    void OnValidate()
    {
        mapWidth = Mathf.Max(4, mapWidth);
        mapHeight = Mathf.Max(4, mapHeight);
        noiseOctaves = Mathf.Max(1, noiseOctaves);
        noiseScale = Mathf.Max(0.0001f, noiseScale);
        persistence = Mathf.Clamp01(persistence);
        lacunarity = Mathf.Max(1.0f, lacunarity);
        corridorHalfWidth = Mathf.Max(0, corridorHalfWidth);
        corridorHalfHeight = Mathf.Max(0, corridorHalfHeight);
        minRegionArea = Mathf.Max(0, minRegionArea);
    }

    void Start()
    {
        seedInt = HashToInt(worldSeed);
        if (randomizeSeedOnPlay)
        {
            seedInt = UnityEngine.Random.Range(int.MinValue / 2, int.MaxValue / 2);
            worldSeed = seedInt.ToString();
        }
        seededRnd = new System.Random(seedInt);

        if (generateOnStart) Generate();
    }

    // ---------- Inspector buttons ----------
    [ContextMenu("Generate (Current Seed)")]
    public void Generate()
    {
        if (groundMap == null || solidTile == null)
        {
            Debug.LogWarning("Assign groundMap and solidTile.");
            return;
        }

        // ensure PRNG is ready (useful when called in edit mode)
        if (seededRnd == null)
        {
            seedInt = HashToInt(worldSeed);
            seededRnd = new System.Random(seedInt);
        }

        if (clearBeforeGenerate) groundMap.ClearAllTiles();

        // 1) Build boolean grid from noise
        BuildNoiseGrid();

        // 2) Optional smoothing to remove pinholes/spikes
        if (smoothOnce) SmoothStep();

        // 3) Connectivity: keep main region, connect or remove others
        EnsureConnectivity();

        // 4) Paint
        PaintGrid();
        groundMap.RefreshAllTiles();
        groundMap.CompressBounds();
    }

    [ContextMenu("Randomize Seed + Generate")]
    public void RandomizeAndGenerate()
    {
        worldSeed = UnityEngine.Random.Range(int.MinValue / 2, int.MaxValue / 2).ToString();
        seedInt = HashToInt(worldSeed);
        seededRnd = new System.Random(seedInt);
        Generate();
    }

    // -------------------- GRID GEN --------------------
    void BuildNoiseGrid()
    {
        solid = new bool[mapWidth, mapHeight];

        Vector2[] octaveOffsets = new Vector2[Mathf.Max(1, noiseOctaves)];
        for (int i = 0; i < octaveOffsets.Length; i++)
            octaveOffsets[i] = new Vector2(
                (float)seededRnd.NextDouble() * 10000f,
                (float)seededRnd.NextDouble() * 10000f
            );

        int sx = centerAtOrigin ? -mapWidth / 2 : 0;
        int sy = centerAtOrigin ? -mapHeight / 2 : 0;

        for (int ix = 0; ix < mapWidth; ix++)
        {
            int x = sx + ix;
            for (int iy = 0; iy < mapHeight; iy++)
            {
                int y = sy + iy;
                float n = FractalPerlin(x, y, octaveOffsets, noiseScale, persistence, lacunarity, noiseOffset);
                bool placeSolid = n < noiseThreshold;
                solid[ix, iy] = placeSolid;
            }
        }
    }

    void SmoothStep()
    {
        bool[,] next = (bool[,])solid.Clone();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int solidNeighbors = CountSolidNeighbors(x, y);
                if (solid[x, y])
                    next[x, y] = (solidNeighbors >= 4); // keep solid if supported
                else
                    next[x, y] = (solidNeighbors >= 5); // fill sparse air pockets
            }
        }
        solid = next;
    }

    int CountSolidNeighbors(int cx, int cy)
    {
        int count = 0;
        for (int x = cx - 1; x <= cx + 1; x++)
        {
            for (int y = cy - 1; y <= cy + 1; y++)
            {
                if (x == cx && y == cy) continue;
                if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) { count++; continue; } // treat out of bounds as solid
                if (solid[x, y]) count++;
            }
        }
        return count;
    }

    // -------------------- CONNECTIVITY --------------------
    void EnsureConnectivity()
    {
        var regions = FindAirRegions();
        if (regions.Count == 0) return;

        // Sort by size (desc)
        regions.Sort((a, b) => b.tiles.Count.CompareTo(a.tiles.Count));

        // Remove tiny regions outright
        for (int i = regions.Count - 1; i >= 0; i--)
        {
            if (regions[i].tiles.Count < minRegionArea)
            {
                foreach (var t in regions[i].tiles)
                    solid[t.x, t.y] = true; // fill with solid
                regions.RemoveAt(i);
            }
        }

        if (!connectAllRegions) return;

        // Recompute after removals
        regions = FindAirRegions();
        if (regions.Count == 0) return;
        regions.Sort((a, b) => b.tiles.Count.CompareTo(a.tiles.Count));
        var main = regions[0];

        // Connect every other region to main by a corridor between centroids
        for (int i = 1; i < regions.Count; i++)
        {
            var a = regions[i].centroid;
            var b = main.centroid;
            CarveCorridor(a, b, corridorHalfWidth, corridorHalfHeight);
        }
    }

    class Region
    {
        public List<Vector2Int> tiles = new List<Vector2Int>();
        public Vector2Int centroid;
    }

    List<Region> FindAirRegions()
    {
        var regions = new List<Region>();
        bool[,] vis = new bool[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (vis[x, y] || solid[x, y]) continue;

                var reg = new Region();
                var q = new Queue<Vector2Int>();
                q.Enqueue(new Vector2Int(x, y));
                vis[x, y] = true;

                long sx = 0, sy = 0;
                while (q.Count > 0)
                {
                    var p = q.Dequeue();
                    reg.tiles.Add(p);
                    sx += p.x; sy += p.y;

                    // 4-neighbors only (platformer)
                    TryVisit(p.x + 1, p.y, vis, q);
                    TryVisit(p.x - 1, p.y, vis, q);
                    TryVisit(p.x, p.y + 1, vis, q);
                    TryVisit(p.x, p.y - 1, vis, q);
                }

                if (reg.tiles.Count > 0)
                    reg.centroid = new Vector2Int((int)(sx / reg.tiles.Count), (int)(sy / reg.tiles.Count));

                regions.Add(reg);
            }
        }
        return regions;
    }

    void TryVisit(int x, int y, bool[,] vis, Queue<Vector2Int> q)
    {
        if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) return;
        if (vis[x, y]) return;
        if (solid[x, y]) return;
        vis[x, y] = true;
        q.Enqueue(new Vector2Int(x, y));
    }

    void CarveCorridor(Vector2Int a, Vector2Int b, int halfW, int halfH)
    {
        // Randomized pivot so we don't always pass through b.x
        // Use seededRnd for determinism with worldSeed
        float tX = 0.3f + (float)seededRnd.NextDouble() * 0.4f; // 0.3..0.7
        float tY = 0.3f + (float)seededRnd.NextDouble() * 0.4f; // 0.3..0.7

        int pivotX = Mathf.RoundToInt(Mathf.Lerp(a.x, b.x, tX));
        int pivotY = Mathf.RoundToInt(Mathf.Lerp(a.y, b.y, tY));

        // Randomize whether we go H-first or V-first
        bool horizontalFirst = seededRnd.Next(2) == 0;

        if (horizontalFirst)
        {
            CarveBoxLine(a, new Vector2Int(pivotX, a.y), halfW, halfH);
            CarveBoxLine(new Vector2Int(pivotX, a.y), new Vector2Int(pivotX, pivotY), halfW, halfH);
            CarveBoxLine(new Vector2Int(pivotX, pivotY), new Vector2Int(b.x, pivotY), halfW, halfH);
            CarveBoxLine(new Vector2Int(b.x, pivotY), b, halfW, halfH);
        }
        else
        {
            CarveBoxLine(a, new Vector2Int(a.x, pivotY), halfW, halfH);
            CarveBoxLine(new Vector2Int(a.x, pivotY), new Vector2Int(pivotX, pivotY), halfW, halfH);
            CarveBoxLine(new Vector2Int(pivotX, pivotY), new Vector2Int(pivotX, b.y), halfW, halfH);
            CarveBoxLine(new Vector2Int(pivotX, b.y), b, halfW, halfH);
        }
    }

    void CarveBoxLine(Vector2Int from, Vector2Int to, int halfW, int halfH)
    {
        int dx = Math.Sign(to.x - from.x);
        int dy = Math.Sign(to.y - from.y);

        int x = from.x;
        int y = from.y;

        int steps = Mathf.Max(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));
        for (int i = 0; i <= steps; i++)
        {
            CarveBox(x, y, halfW, halfH);
            x += dx;
            y += dy;
        }
    }

    void CarveBox(int cx, int cy, int halfW, int halfH)
    {
        int minX = Mathf.Clamp(cx - halfW, 0, mapWidth - 1);
        int maxX = Mathf.Clamp(cx + halfW, 0, mapWidth - 1);
        int minY = Mathf.Clamp(cy - halfH, 0, mapHeight - 1);
        int maxY = Mathf.Clamp(cy + halfH, 0, mapHeight - 1);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                solid[x, y] = false; // air
    }

    // -------------------- PAINT --------------------
    void PaintGrid()
    {
        int sx = centerAtOrigin ? -mapWidth / 2 : 0;
        int sy = centerAtOrigin ? -mapHeight / 2 : 0;

        for (int ix = 0; ix < mapWidth; ix++)
        {
            for (int iy = 0; iy < mapHeight; iy++)
            {
                Vector3Int pos = new Vector3Int(sx + ix, sy + iy, 0);
                if (solid[ix, iy]) groundMap.SetTile(pos, solidTile);
                else groundMap.SetTile(pos, null);
            }
        }
    }

    // -------------------- NOISE --------------------
    float FractalPerlin(int x, int y, Vector2[] octaveOffsets, float scale, float pers, float lac, Vector2 globalOffset)
    {
        if (scale <= 0f) scale = 0.0001f;
        float amplitude = 1f;
        float frequency = 1f;
        float total = 0f;
        float maxPossible = 0f;

        for (int i = 0; i < Mathf.Max(1, noiseOctaves); i++)
        {
            float sampleX = (x + globalOffset.x + octaveOffsets[i].x) / scale * frequency;
            float sampleY = (y + globalOffset.y + octaveOffsets[i].y) / scale * frequency;
            float perlin = Mathf.PerlinNoise(sampleX, sampleY);
            total += perlin * amplitude;
            maxPossible += amplitude;
            amplitude *= pers;
            frequency *= lac;
        }
        return (maxPossible > 0f) ? total / maxPossible : total;
    }

    int HashToInt(string s)
    {
        using (var md5 = MD5.Create())
        {
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(s ?? ""));
            return BitConverter.ToInt32(data, 0);
        }
    }
}
