using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FogOfWar : MonoBehaviour
{
    public static FogOfWar Instance;

    [Header("Fog Settings")]
    [SerializeField] private int textureSize = 256;
    [SerializeField] private float revealRadius = 5f;
    [SerializeField] private Color fogColor = Color.black;

    private Texture2D fogTexture;
    private SpriteRenderer fogRenderer;
    private Color[] fogColors;
    private bool[,] revealed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        fogRenderer = GetComponent<SpriteRenderer>();
        InitializeFog();
    }

    private void InitializeFog()
    {
        fogTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Point;

        fogColors = new Color[textureSize * textureSize];
        revealed = new bool[textureSize, textureSize];

        for (int i = 0; i < fogColors.Length; i++)
            fogColors[i] = fogColor;

        fogTexture.SetPixels(fogColors);
        fogTexture.Apply();

        fogRenderer.sprite = Sprite.Create(fogTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
    }

    public void RevealArea(Vector3 worldPos)
    {
        Vector2 fogPos = WorldToFogPosition(worldPos);

        int centerX = Mathf.RoundToInt(fogPos.x);
        int centerY = Mathf.RoundToInt(fogPos.y);
        int radius = Mathf.RoundToInt(revealRadius * (textureSize / fogRenderer.bounds.size.x));

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int pixelX = centerX + x;
                int pixelY = centerY + y;

                if (pixelX < 0 || pixelX >= textureSize || pixelY < 0 || pixelY >= textureSize) continue;

                if (x * x + y * y <= radius * radius)
                {
                    if (!revealed[pixelX, pixelY])
                    {
                        revealed[pixelX, pixelY] = true;
                        fogColors[pixelY * textureSize + pixelX] = new Color(0, 0, 0, 0); // transparent
                    }
                }
            }
        }

        fogTexture.SetPixels(fogColors);
        fogTexture.Apply();
    }

    private Vector2 WorldToFogPosition(Vector3 worldPos)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos);
        float halfSize = textureSize / 2f;
        return new Vector2(
            (local.x / fogRenderer.bounds.size.x) * textureSize + halfSize,
            (local.y / fogRenderer.bounds.size.y) * textureSize + halfSize
        );
    }
}
