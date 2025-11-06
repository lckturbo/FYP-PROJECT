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
    [SerializeField, Range(0f, 1f)] private float digitDotFraction = 0.9f;
    [SerializeField] private float edgePadding = 12f;

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

    private Texture2D plateTex;
    private bool awaitingSubmit;
    private string correctAnswer;
    private float timeLeft;

    private void Awake()
    {
        Result = MinigameManager.ResultType.Fail;
        if (verifyButton) { verifyButton.onClick.RemoveAllListeners(); verifyButton.onClick.AddListener(OnVerify); }
        if (answerInput) { answerInput.text = ""; }

        if (promptText) promptText.text = "Camera calibration test: Enter the number hidden in the dots.";
        if (resultText) resultText.text = "";

        plateTex = new Texture2D(plateSize, plateSize, TextureFormat.RGBA32, false);
        plateTex.wrapMode = TextureWrapMode.Clamp;
        if (plateImage) plateImage.texture = plateTex;
    }

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);

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

        float cx = plateSize * 0.5f;
        float cy = plateSize * 0.5f;
        float radius = (plateSize * 0.5f) - edgePadding;

        Color bgA = backgroundPalette[Random.Range(0, backgroundPalette.Length)];
        Color bgB = backgroundPalette[Random.Range(0, backgroundPalette.Length)];
        Color dgA = digitPalette[Random.Range(0, digitPalette.Length)];
        Color dgB = digitPalette[Random.Range(0, digitPalette.Length)];

        SevenSegMask mask = new SevenSegMask();

        for (int i = 0; i < dotCount; i++)
        {
            float ang = Random.value * Mathf.PI * 2f;
            float r = Mathf.Sqrt(Random.value) * radius;
            float px = cx + Mathf.Cos(ang) * r;
            float py = cy + Mathf.Sin(ang) * r;

            float nx = (px - (cx - radius)) / (radius * 2f);
            float ny = (py - (cy - radius)) / (radius * 2f);

            bool inDigit = IsInTwoDigitMask(nx, ny, number, mask);

            Color baseCol = inDigit
                ? Color.Lerp(dgA, dgB, Random.value)
                : Color.Lerp(bgA, bgB, Random.value);

            DrawFilledCircle(plateTex, (int)px, (int)py, Random.Range((int)dotRadiusPx.x, (int)dotRadiusPx.y + 1), baseCol);
        }

        plateTex.Apply();
    }

    private bool IsInTwoDigitMask(float nx, float ny, int number, SevenSegMask mask)
    {
        if (Random.value > digitDotFraction) return false;

        int leftDigit = (number / 10) % 10;
        int rightDigit = number % 10;

        Rect leftRect = new Rect(0.05f, 0.18f, 0.42f, 0.64f);
        Rect rightRect = new Rect(0.53f, 0.18f, 0.42f, 0.64f);

        if (leftRect.Contains(new Vector2(nx, ny)))
        {
            float lx = (nx - leftRect.x) / leftRect.width;
            float ly = (ny - leftRect.y) / leftRect.height;
            return mask.PointInDigit(lx, ly, leftDigit);
        }
        if (rightRect.Contains(new Vector2(nx, ny)))
        {
            float rx = (nx - rightRect.x) / rightRect.width;
            float ry = (ny - rightRect.y) / rightRect.height;
            return mask.PointInDigit(rx, ry, rightDigit);
        }
        return false;
    }

    private void DrawFilledCircle(Texture2D tex, int cx, int cy, int r, Color col)
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

                Color blended = Color.Lerp(under, col, 0.85f);
                blended.a = 1f;
                tex.SetPixel(x, y, blended);
            }
        }
    }

    private class SevenSegMask
    {
        private readonly Rect[] segs =
        {
            new Rect(0.18f, 0.86f, 0.64f, 0.10f),
            new Rect(0.78f, 0.56f, 0.10f, 0.30f),
            new Rect(0.78f, 0.14f, 0.10f, 0.34f),
            new Rect(0.18f, 0.04f, 0.64f, 0.10f),
            new Rect(0.12f, 0.14f, 0.10f, 0.34f),
            new Rect(0.12f, 0.56f, 0.10f, 0.30f),
            new Rect(0.20f, 0.45f, 0.60f, 0.10f),
        };

        // which segments are lit per digit
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

        private bool RoundRectContains(Rect r, float x, float y, float radius = 0.08f)
        {
            Rect core = new Rect(r.x + radius, r.y + radius, r.width - 2 * radius, r.height - 2 * radius);
            if (core.Contains(new Vector2(x, y))) return true;

            Vector2[] centers =
            {
                new Vector2(r.x + radius, r.y + radius),
                new Vector2(r.xMax - radius, r.y + radius),
                new Vector2(r.x + radius, r.yMax - radius),
                new Vector2(r.xMax - radius, r.yMax - radius),
            };
            float rad2 = radius * radius;
            foreach (var c in centers)
            {
                float dx = x - c.x, dy = y - c.y;
                if (dx * dx + dy * dy <= rad2) return true;
            }
            return false;
        }

        public bool PointInDigit(float x, float y, int digit)
        {
            digit = Mathf.Clamp(digit, 0, 9);
            var lights = map[digit];
            for (int i = 0; i < segs.Length; i++)
            {
                if (!lights[i]) continue;
                if (RoundRectContains(segs[i], x, y, 0.08f)) return true;
            }
            return false;
        }
    }
}
