using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralNoiseTilemapGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundMap;          // the map we paint into
    [SerializeField] private Tilemap backgroundMap;      // optional (decor/backdrop)

    [Header("Tiles")]
    [SerializeField] private TileBase solidTile;         // RuleTile recommended (autotile)
    [SerializeField] private TileBase backgroundTile;    // optional backdrop tile

    [Header("Map Size (tiles)")]
    [SerializeField] private int mapWidth = 80;
    [SerializeField] private int mapHeight = 50;
    [SerializeField] private bool centerAtOrigin = true; // if true, we generate around (0,0)

    [Header("Seed & Noise (Fractal Perlin)")]
    [SerializeField] private string worldSeed = "Hello Unity!";
    [SerializeField] private int noiseOctaves = 3;
    [SerializeField] private float noiseScale = 3f;      // like "period" (smaller = smoother)
    [SerializeField] private float persistence = 0.7f;   // amplitude falloff per octave
    [SerializeField] private float lacunarity = 2.0f;    // frequency increase per octave
    [SerializeField] private float noiseThreshold = 0.5f;// place tile when noise < threshold
    [SerializeField] private Vector2 noiseOffset = Vector2.zero; // shifts pattern

    [Header("Caves (optional carve)")]
    [SerializeField] private bool carveCaves = false;
    [SerializeField] private float caveScale = 6f;
    [SerializeField] private float caveThreshold = 0.6f;

    [Header("Runtime")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool clearBeforeGenerate = true;
    [SerializeField] private bool randomizeSeedOnPlay = false;

    System.Random seededRnd;
    int seedInt;

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

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (groundMap == null || solidTile == null)
        {
            Debug.LogWarning("Assign groundMap and solidTile.");
            return;
        }

        if (clearBeforeGenerate) Clear();

        // Pre-randomized offsets per octave so seed actually changes the pattern
        Vector2[] octaveOffsets = new Vector2[Mathf.Max(1, noiseOctaves)];
        for (int i = 0; i < octaveOffsets.Length; i++)
            octaveOffsets[i] = new Vector2(
                (float)seededRnd.NextDouble() * 10000f,
                (float)seededRnd.NextDouble() * 10000f
            );

        // Bounds
        int startX = centerAtOrigin ? -mapWidth / 2 : 0;
        int startY = centerAtOrigin ? -mapHeight / 2 : 0;
        int endX = startX + mapWidth;
        int endY = startY + mapHeight;

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                float n = FractalPerlin(x, y, octaveOffsets, noiseScale, persistence, lacunarity, noiseOffset);
                bool placeSolid = n < noiseThreshold;

                // optional cave carve: clear some solids back out using a second noise
                if (carveCaves && placeSolid)
                {
                    float c = Perlin01(x / caveScale + seedInt * 0.00123f, y / caveScale - seedInt * 0.00234f);
                    if (c > caveThreshold) placeSolid = false;
                }

                Vector3Int pos = new Vector3Int(x, y, 0);
                if (placeSolid)
                {
                    groundMap.SetTile(pos, solidTile);
                    if (backgroundMap && backgroundTile) backgroundMap.SetTile(pos, backgroundTile);
                }
                else
                {
                    // Air in foreground; you can still put background in caves if you want
                    if (backgroundMap && backgroundTile) backgroundMap.SetTile(pos, backgroundTile);
                }
            }
        }

        // Let RuleTile recompute neighbors
        groundMap.RefreshAllTiles();
        if (backgroundMap) backgroundMap.RefreshAllTiles();

        // Optional: compress bounds to tighten the Tilemap rect
        groundMap.CompressBounds();
        if (backgroundMap) backgroundMap.CompressBounds();
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        if (groundMap) groundMap.ClearAllTiles();
        if (backgroundMap) backgroundMap.ClearAllTiles();
    }

    // ----------------------
    // Noise helpers
    // ----------------------
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

            float perlin = Perlin01(sampleX, sampleY); // 0..1
            total += perlin * amplitude;

            maxPossible += amplitude;
            amplitude *= pers;
            frequency *= lac;
        }

        // Normalize to 0..1
        return (maxPossible > 0f) ? total / maxPossible : total;
    }

    // Unity's Perlin returns 0..1 already
    float Perlin01(float x, float y) => Mathf.PerlinNoise(x, y);

    // Deterministic hash from string to int
    int HashToInt(string s)
    {
        using (var md5 = MD5.Create())
        {
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(s ?? ""));
            return BitConverter.ToInt32(data, 0);
        }
    }
}
