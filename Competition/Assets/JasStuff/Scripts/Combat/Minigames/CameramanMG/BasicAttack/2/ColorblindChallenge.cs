using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorblindChallenge : BaseMinigame
{
    [Header("UI")]
    [SerializeField] private RawImage plateImage;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button verifyButton;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text resultText;

    [Header("Gameplay")]
    [SerializeField, Min(1)] private int seconds = 15;
    [SerializeField] private Vector2Int numberRange = new Vector2Int(10, 99);
    [SerializeField] private bool alwaysTwoDigits = true;

    [Header("Plate")]
    [SerializeField] private int plateSize = 768;
    [SerializeField] private int dotCount = 1900;
    [SerializeField] private Vector2 dotRadiusPx = new Vector2(8, 16);
    [SerializeField, Range(0f, 1f)] private float digitDotFraction = 0.90f;
    [SerializeField] private float edgePadding = 12f;

    [Header("Animator")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject animationPanel;

    [Header("Colors")]
    [SerializeField]
    private Color[] backgroundPalette = new Color[]
    {
        new Color(0.90f, 0.55f, 0.35f),
        new Color(0.85f, 0.70f, 0.40f),
        new Color(0.75f, 0.55f, 0.45f),
        new Color(0.65f, 0.65f, 0.65f),
        new Color(0.80f, 0.50f, 0.60f),
        new Color(0.92f, 0.60f, 0.45f),
        new Color(0.88f, 0.65f, 0.55f),
        new Color(0.78f, 0.62f, 0.40f),
        new Color(0.82f, 0.58f, 0.44f),
        new Color(0.75f, 0.70f, 0.45f),
    };

    [SerializeField]
    private Color[] digitPalette = new Color[]
    {
        new Color(0.15f, 0.80f, 0.60f),
        new Color(0.10f, 0.70f, 0.90f),
        new Color(0.25f, 0.85f, 0.45f),
        new Color(0.35f, 0.75f, 0.25f),
        new Color(0.40f, 0.65f, 0.90f),
        new Color(0.25f, 0.75f, 0.75f),
        new Color(0.50f, 0.70f, 0.20f),
        new Color(0.15f, 0.75f, 0.50f),
        new Color(0.45f, 0.80f, 0.70f),
        new Color(0.30f, 0.60f, 0.85f),
    };

    [Header("Clarity Tweaks")]
    [SerializeField, Range(0f, 1f)] private float minLumaDiff = 0.30f;
    [SerializeField, Range(0.5f, 2f)] private float digitRadiusScale = 1.25f;
    [SerializeField, Range(0f, 1f)] private float bgAlpha = 0.80f;
    [SerializeField, Range(0f, 1f)] private float digitAlpha = 1.00f;
    [SerializeField, Range(0.50f, 1f)] private float digitFractionOverride = 0.97f;

    [Header("Sharpness (Segments)")]
    [SerializeField, Range(0f, 0.08f)] private float segmentInflate = 0.02f;
    [SerializeField, Min(0)] private int minDotsPerLitSegment = 28;
    [SerializeField, Min(0)] private int edgeAnchorDotsPerSegment = 6;
    [SerializeField, Range(1f, 2f)] private float edgeAnchorRadiusScale = 1.35f;

    [Header("Density")]
    [SerializeField] private bool useDigitGrid = true;
    [SerializeField, Min(2)] private int digitGridCols = 18;
    [SerializeField, Min(2)] private int digitGridRows = 26;

    [SerializeField] private bool useBackgroundGrid = false;
    [SerializeField, Min(2)] private int bgGridCols = 14;
    [SerializeField, Min(2)] private int bgGridRows = 20;
    [SerializeField, Range(0f, 1f)] private float bgGridFillChance = 0.35f;

    private Texture2D plateTex;
    private bool awaitingSubmit;
    private string correctAnswer;
    private float timeLeft;

    private void Awake()
    {
        Result = MinigameManager.ResultType.Fail;
        if (verifyButton) { verifyButton.onClick.RemoveAllListeners(); verifyButton.onClick.AddListener(OnVerify); }
        if (answerInput) { answerInput.text = ""; }

        if (promptText) promptText.text = "Camera calibration test:\nEnter the number hidden in the dots.";
        if (resultText) resultText.text = "";

        plateTex = new Texture2D(plateSize, plateSize, TextureFormat.RGBA32, false);
        plateTex.wrapMode = TextureWrapMode.Clamp;
        if (plateImage) plateImage.texture = plateTex;
    }

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);

        animationPanel.SetActive(true);

        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(1.5f);
        }

        animationPanel.SetActive(false);

        int num = Random.Range(numberRange.x, numberRange.y + 1);
        if (alwaysTwoDigits) num = Mathf.Clamp(num, 10, 99);
        correctAnswer = num.ToString();

        GeneratePlate(num);

        if (answerInput) { answerInput.text = ""; answerInput.ActivateInputField(); }
        if (resultText) resultText.text = "";
        awaitingSubmit = true;
        timeLeft = Mathf.Max(1, seconds);

        while (awaitingSubmit && timeLeft > 0f)
        {
            timeLeft -= Time.unscaledDeltaTime;
            if (timerText) timerText.text = timeLeft.ToString("0.0");
            yield return null;
        }

        if (awaitingSubmit) OnVerify();

        yield return new WaitForSecondsRealtime(0.6f);

        BattleManager.instance?.SetBattlePaused(false);
    }

    private void OnVerify()
    {
        awaitingSubmit = false;
        string input = (answerInput ? answerInput.text.Trim() : "");
        bool ok = input == correctAnswer;

        Result = ok ? MinigameManager.ResultType.Success : MinigameManager.ResultType.Fail;
        if (resultText) resultText.text = ok ? "Success!" : "Failed!";
    }

    private void GeneratePlate(int number)
    {
        var cols = plateTex.GetPixels32();
        for (int i = 0; i < cols.Length; i++) cols[i] = new Color32(0, 0, 0, 0);
        plateTex.SetPixels32(cols);

        float cx = plateSize * 0.5f, cy = plateSize * 0.5f;
        float radius = (plateSize * 0.5f) - edgePadding;

        Color bgA, bgB, dgA, dgB;
        PickContrastingPair(backgroundPalette, backgroundPalette, minLumaDiff * 0.5f, out bgA, out bgB);
        PickContrastingPair(digitPalette, backgroundPalette, minLumaDiff, out dgA, out dgB);

        SevenSegMask mask = new SevenSegMask();

        int leftDigit = Mathf.Clamp((number / 10) % 10, 0, 9);
        int rightDigit = Mathf.Clamp(number % 10, 0, 9);
        Rect leftRect = new Rect(0.05f, 0.18f, 0.42f, 0.64f);
        Rect rightRect = new Rect(0.53f, 0.18f, 0.42f, 0.64f);

        int prePlaced = 0;
        if (useBackgroundGrid)
        {
            prePlaced += GridFillBackground(leftRect, bgA, bgB);
            prePlaced += GridFillBackground(rightRect, bgA, bgB);
        }

        PlaceAnchorsForDigit(leftRect, leftDigit, mask, dgA, dgB);
        PlaceAnchorsForDigit(rightRect, rightDigit, mask, dgA, dgB);

        if (useDigitGrid)
        {
            prePlaced += GridFillDigit(leftRect, leftDigit, mask, dgA, dgB);
            prePlaced += GridFillDigit(rightRect, rightDigit, mask, dgA, dgB);
        }

        int remaining = Mathf.Max(0, dotCount - prePlaced);
        int[] segCounts = new int[14];
        float frac = Mathf.Max(digitDotFraction, digitFractionOverride);

        for (int i = 0; i < remaining; i++)
        {
            float ang = Random.value * Mathf.PI * 2f;
            float rr = Mathf.Sqrt(Random.value) * radius;
            float px = cx + Mathf.Cos(ang) * rr;
            float py = cy + Mathf.Sin(ang) * rr;

            float nx = (px - (cx - radius)) / (radius * 2f);
            float ny = (py - (cy - radius)) / (radius * 2f);

            bool inDigit = false;
            int segIdxGlobal = -1;

            if (Random.value <= frac)
            {
                if (leftRect.Contains(new Vector2(nx, ny)))
                {
                    float lx = (nx - leftRect.x) / leftRect.width;
                    float ly = (ny - leftRect.y) / leftRect.height;
                    inDigit = mask.PointInDigit(lx, ly, leftDigit, segmentInflate);
                    if (inDigit)
                    {
                        int local = mask.WhichSegment(lx, ly, leftDigit, segmentInflate);
                        if (local >= 0) segIdxGlobal = local;
                    }
                }
                else if (rightRect.Contains(new Vector2(nx, ny)))
                {
                    float rx = (nx - rightRect.x) / rightRect.width;
                    float ry = (ny - rightRect.y) / rightRect.height;
                    inDigit = mask.PointInDigit(rx, ry, rightDigit, segmentInflate);
                    if (inDigit)
                    {
                        int local = mask.WhichSegment(rx, ry, rightDigit, segmentInflate);
                        if (local >= 0) segIdxGlobal = 7 + local;
                    }
                }
            }

            Color baseCol = inDigit ? Color.Lerp(dgA, dgB, Random.value)
                                    : Color.Lerp(bgA, bgB, Random.value);

            int baseRad = Random.Range((int)dotRadiusPx.x, (int)dotRadiusPx.y + 1);
            int rad = inDigit ? Mathf.CeilToInt(baseRad * digitRadiusScale) : baseRad;
            float alpha = inDigit ? digitAlpha : bgAlpha;

            DrawDot(plateTex, (int)px, (int)py, rad, baseCol, alpha);

            if (segIdxGlobal >= 0) segCounts[segIdxGlobal]++;
        }

        EnforceMinPerSegment(leftRect, leftDigit, 0, segCounts, mask, dgA, dgB);
        EnforceMinPerSegment(rightRect, rightDigit, 7, segCounts, mask, dgA, dgB);

        plateTex.Apply();
    }

    private static float RelativeLuma(Color c)
    {
        float rl = Mathf.LinearToGammaSpace(c.r);
        float gl = Mathf.LinearToGammaSpace(c.g);
        float bl = Mathf.LinearToGammaSpace(c.b);
        return 0.2126f * rl + 0.7152f * gl + 0.0722f * bl;
    }

    private void PickContrastingPair(Color[] aPal, Color[] bPal, float minDiff, out Color a, out Color b)
    {
        a = aPal[Random.Range(0, aPal.Length)];
        b = bPal[Random.Range(0, bPal.Length)];
        float best = Mathf.Abs(RelativeLuma(a) - RelativeLuma(b));
        const int ATTEMPTS = 16;

        for (int i = 0; i < ATTEMPTS && best < minDiff; i++)
        {
            Color aa = aPal[Random.Range(0, aPal.Length)];
            Color bb = bPal[Random.Range(0, bPal.Length)];
            float gap = Mathf.Abs(RelativeLuma(aa) - RelativeLuma(bb));
            if (gap > best) { a = aa; b = bb; best = gap; }
        }
    }

    private void DrawDot(Texture2D tex, int cx, int cy, int r, Color col, float alpha)
    {
        int x0 = Mathf.Max(0, cx - r), x1 = Mathf.Min(tex.width - 1, cx + r);
        int y0 = Mathf.Max(0, cy - r), y1 = Mathf.Min(tex.height - 1, cy + r);
        float r2 = r * r;

        for (int y = y0; y <= y1; y++)
        {
            int dy = y - cy; float dy2 = dy * dy;
            for (int x = x0; x <= x1; x++)
            {
                int dx = x - cx; if (dx * dx + dy2 > r2) continue;

                Color under = tex.GetPixel(x, y);
                Color blended = Color.Lerp(under, col, alpha);
                blended.a = 1f;
                tex.SetPixel(x, y, blended);
            }
        }
    }

    private void PlaceAnchorsForDigit(Rect digitRect, int digit, SevenSegMask mask, Color dgA, Color dgB)
    {
        var lit = mask.GetLitSegments(digit, segmentInflate);
        for (int s = 0; s < lit.Count; s++)
        {
            var (segRect, horizontal, segIdx) = lit[s];
            for (int i = 0; i < edgeAnchorDotsPerSegment; i++)
            {
                float t = (i + 1f) / (edgeAnchorDotsPerSegment + 1f);
                float u = horizontal ? Mathf.Lerp(segRect.x + 0.06f, segRect.xMax - 0.06f, t)
                                     : segRect.x + (Random.value * (segRect.width * 0.5f) + segRect.width * 0.25f);
                float v = horizontal ? segRect.y + (Random.value * (segRect.height * 0.5f) + segRect.height * 0.25f)
                                     : Mathf.Lerp(segRect.y + 0.06f, segRect.yMax - 0.06f, t);

                Vector2 pix = DigitUVToPixel(digitRect, u, v);
                int baseRad = Random.Range((int)dotRadiusPx.x, (int)dotRadiusPx.y + 1);
                int rad = Mathf.CeilToInt(baseRad * edgeAnchorRadiusScale);
                Color c = Color.Lerp(dgA, dgB, Random.value);
                DrawDot(plateTex, (int)pix.x, (int)pix.y, rad, c, 1.0f);
            }
        }
    }

    private void EnforceMinPerSegment(Rect digitRect, int digit, int globalOffset, int[] segCounts, SevenSegMask mask, Color dgA, Color dgB)
    {
        var lit = mask.GetLitSegments(digit, segmentInflate);
        for (int s = 0; s < lit.Count; s++)
        {
            var (segRect, horizontal, segIdx) = lit[s];
            int global = globalOffset + segIdx;
            int need = Mathf.Max(0, minDotsPerLitSegment - segCounts[global]);

            for (int n = 0; n < need; n++)
            {
                float u = segRect.x + Random.value * segRect.width;
                float v = segRect.y + Random.value * segRect.height;

                Vector2 pix = DigitUVToPixel(digitRect, u, v);
                int baseRad = Random.Range((int)dotRadiusPx.x, (int)dotRadiusPx.y + 1);
                int rad = Mathf.CeilToInt(baseRad * digitRadiusScale);
                Color c = Color.Lerp(dgA, dgB, Random.value);
                DrawDot(plateTex, (int)pix.x, (int)pix.y, rad, c, 1.0f);
            }
        }
    }

    private Vector2 DigitUVToPixel(Rect digitRect, float u, float v)
    {
        float cx = plateSize * 0.5f, cy = plateSize * 0.5f;
        float radius = (plateSize * 0.5f) - edgePadding;

        float left = (cx - radius);
        float bottom = (cy - radius);

        float nx = digitRect.x + u * digitRect.width;
        float ny = digitRect.y + v * digitRect.height;

        float px = left + nx * (radius * 2f);
        float py = bottom + ny * (radius * 2f);
        return new Vector2(px, py);
    }

    private class SevenSegMask
    {
        private readonly Rect[] segsBase =
        {
            new Rect(0.18f, 0.86f, 0.64f, 0.10f),
            new Rect(0.78f, 0.56f, 0.10f, 0.30f),
            new Rect(0.78f, 0.14f, 0.10f, 0.34f),
            new Rect(0.18f, 0.04f, 0.64f, 0.10f),
            new Rect(0.12f, 0.14f, 0.10f, 0.34f),
            new Rect(0.12f, 0.56f, 0.10f, 0.30f),
            new Rect(0.20f, 0.45f, 0.60f, 0.10f),
        };

        private readonly bool[][] map =
        {
            new[]{ true,  true,  true,  true,  true,  true,  false },
            new[]{ false, true,  true,  false, false, false, false },
            new[]{ true,  true,  false, true,  true,  false, true  },
            new[]{ true,  true,  true,  true,  false, false, true  },
            new[]{ false, true,  true,  false, false, true,  true  },
            new[]{ true,  false, true,  true,  false, true,  true  },
            new[]{ true,  false, true,  true,  true,  true,  true  },
            new[]{ true,  true,  true,  false, false, false, false },
            new[]{ true,  true,  true,  true,  true,  true,  true  },
            new[]{ true,  true,  true,  true,  false, true,  true  },
        };

        private readonly bool[] isHorizontal = { true, false, false, true, false, false, true };

        private Rect Inflate(Rect r, float pad) => new Rect(r.x - pad, r.y - pad, r.width + pad * 2f, r.height + pad * 2f);

        private bool RoundRectContains(Rect r, float x, float y, float corner = 0.06f)
        {
            Rect core = new Rect(r.x + corner, r.y + corner, r.width - 2 * corner, r.height - 2 * corner);
            if (core.Contains(new Vector2(x, y))) return true;

            Vector2[] centers =
            {
                new Vector2(r.x + corner,     r.y + corner),
                new Vector2(r.xMax - corner,  r.y + corner),
                new Vector2(r.x + corner,     r.yMax - corner),
                new Vector2(r.xMax - corner,  r.yMax - corner),
            };
            float rad2 = corner * corner;
            foreach (var c in centers)
            {
                float dx = x - c.x, dy = y - c.y;
                if (dx * dx + dy * dy <= rad2) return true;
            }
            return false;
        }

        public List<(Rect rect, bool horizontal, int segIndex)> GetLitSegments(int digit, float inflatePad)
        {
            digit = Mathf.Clamp(digit, 0, 9);
            var lights = map[digit];
            var list = new List<(Rect, bool, int)>(7);
            for (int i = 0; i < segsBase.Length; i++)
            {
                if (!lights[i]) continue;
                list.Add((Inflate(segsBase[i], inflatePad), isHorizontal[i], i));
            }
            return list;
        }

        public bool PointInDigit(float x, float y, int digit, float inflatePad = 0f)
        {
            var lit = GetLitSegments(digit, inflatePad);
            for (int i = 0; i < lit.Count; i++)
                if (RoundRectContains(lit[i].rect, x, y, 0.06f))
                    return true;
            return false;
        }

        public int WhichSegment(float x, float y, int digit, float inflatePad = 0f)
        {
            var lit = GetLitSegments(digit, inflatePad);
            for (int i = 0; i < lit.Count; i++)
                if (RoundRectContains(lit[i].rect, x, y, 0.06f))
                    return lit[i].segIndex;
            return -1;
        }
    }

    private int GridFillDigit(Rect digitRect, int digit, SevenSegMask mask, Color dgA, Color dgB)
    {
        int placed = 0;
        for (int r = 0; r < digitGridRows; r++)
        {
            for (int c = 0; c < digitGridCols; c++)
            {
                float u = (c + 0.5f) / digitGridCols;
                float v = (r + 0.5f) / digitGridRows;
                if (!mask.PointInDigit(u, v, digit, segmentInflate)) continue;

                float jitterU = (Random.value - 0.5f) / digitGridCols;
                float jitterV = (Random.value - 0.5f) / digitGridRows;
                float uu = Mathf.Clamp01(u + jitterU);
                float vv = Mathf.Clamp01(v + jitterV);

                Vector2 pix = DigitUVToPixel(digitRect, uu, vv);
                int baseRad = Random.Range((int)dotRadiusPx.x, (int)dotRadiusPx.y + 1);
                int rad = Mathf.CeilToInt(baseRad * digitRadiusScale);
                Color ccol = Color.Lerp(dgA, dgB, Random.value);

                DrawDot(plateTex, (int)pix.x, (int)pix.y, rad, ccol, 1.0f);
                placed++;
            }
        }
        return placed;
    }

    private int GridFillBackground(Rect rect, Color bgA, Color bgB)
    {
        int placed = 0;
        for (int r = 0; r < bgGridRows; r++)
        {
            for (int c = 0; c < bgGridCols; c++)
            {
                if (Random.value > bgGridFillChance) continue;

                float u = (c + 0.5f) / bgGridCols;
                float v = (r + 0.5f) / bgGridRows;

                // jitter
                float jitterU = (Random.value - 0.5f) / bgGridCols;
                float jitterV = (Random.value - 0.5f) / bgGridRows;
                float uu = Mathf.Clamp01(u + jitterU);
                float vv = Mathf.Clamp01(v + jitterV);

                Vector2 pix = DigitUVToPixel(rect, uu, vv);
                int baseRad = Random.Range((int)dotRadiusPx.x, (int)dotRadiusPx.y + 1);
                int rad = baseRad;
                Color ccol = Color.Lerp(bgA, bgB, Random.value);

                DrawDot(plateTex, (int)pix.x, (int)pix.y, rad, ccol, bgAlpha);
                placed++;
            }
        }
        return placed;
    }

}
