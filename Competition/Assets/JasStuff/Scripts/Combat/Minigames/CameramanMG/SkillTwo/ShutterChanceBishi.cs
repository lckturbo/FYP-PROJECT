using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShutterChanceBishi : BaseMinigame
{
    [Header("UI (assign from prefab)")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text goldenHint;
    [SerializeField] private Image flashOverlay;

    [Header("Bonus Visuals")]
    [SerializeField] private Image bonusTintOverlay;
    [SerializeField] private ParticleSystem bonusParticles;
    [SerializeField, Range(0f, 1f)] private float bonusTintAlpha = 0.35f;
    [SerializeField] private float bonusTintPulseSpeed = 2.0f;

    [Header("OKAY! Centered")]
    [SerializeField] private TMP_Text okayCenterText;
    [SerializeField] private float okayDisplayTime = 0.45f;

    [Header("Lane Images")]
    [SerializeField] private Image leftImage;
    [SerializeField] private Image midImage;
    [SerializeField] private Image rightImage;

    [Header("Golden Rope (Bonus cue)")]
    [SerializeField] private Image goldenRope;

    [Header("GOLDEN HINT Pulse + Alarm Shake")]
    [SerializeField, Range(0.5f, 3f)] private float goldenHintPulseHz = 2.0f;
    [SerializeField, Range(0f, 0.6f)] private float goldenHintScaleAmp = 0.12f;
    [SerializeField, Range(0f, 0.6f)] private float goldenHintAlphaAmp = 0.20f;
    [SerializeField, Range(0f, 50f)] private float goldenHintShakePx = 10f;
    [SerializeField, Range(0.5f, 30f)] private float goldenHintShakeHz = 14f;
    [SerializeField, Range(0f, 40f)] private float goldenHintTiltDeg = 10f;
    [SerializeField] private Color goldenHintColorA = Color.white;
    [SerializeField] private Color goldenHintColorB = new Color(1f, 0.2f, 0.2f);

    private Coroutine goldenHintPulseCo;
    private Vector2 goldenHintBasePos;
    private Quaternion goldenHintBaseRot;
    private Color goldenHintBaseColor;

    [Header("Sprites (randomized, no duplicates per wave)")]
    [SerializeField] private Sprite[] modelSprites = new Sprite[0];
    [SerializeField] private Sprite[] trashSprites = new Sprite[0];

    [Header("Sprite Variation (Flip)")]
    [SerializeField] private bool flipModels = true;
    [SerializeField, Range(0f, 1f)] private float flipModelsChance = 0.5f;
    [SerializeField] private bool flipTrash = false;
    [SerializeField, Range(0f, 1f)] private float flipTrashChance = 0.25f;

    [Header("Backgrounds (instant swap, no fade)")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite bgRuleIntro1;
    [SerializeField] private Sprite bgRuleIntro2;
    [SerializeField] private Sprite bgRuleIntro3;
    [SerializeField] private Sprite bgPhoto;
    [SerializeField] private Sprite bgTransition;
    [SerializeField] private Sprite bgResult;

    [Header("Cameraman Sprite")]
    [SerializeField] private RectTransform cameraman;
    [SerializeField] private float camWiggleDegrees = 12f;
    [SerializeField] private float camWiggleSeconds = 0.22f;
    private bool camFlipped = false;
    private Coroutine camWiggleCo;

    private Vector2 cameramanBasePos;

    [Header("Cameraman Sprite Swap")]
    [SerializeField] private Image cameramanImage;
    [SerializeField] private Sprite cameramanDefaultSprite;
    [SerializeField] private Sprite cameramanThumbsSprite;

    [Header("Cameraman Hop")]
    [SerializeField] private float camHopHeight = 32f;
    [SerializeField] private float camHopSeconds = 0.24f;
    private Coroutine camHopCo;

    [Header("Screen Shake")]
    [SerializeField] private RectTransform shakeRoot;
    [SerializeField] private float shakeDuration = 0.20f;
    [SerializeField] private float shakeIntensity = 14f;
    [SerializeField] private float shakeFrequency = 28f;
    [SerializeField] private float shakeDecay = 1.6f;
    private Coroutine shakeCo;
    private Vector2 shakeRootBasePos;

    [Header("Phases & Durations")]
    [SerializeField] private float instructionSeconds = 8.0f;
    [SerializeField] private float resultHoldSeconds = 14f;

    [Header("Instruction UI")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private TMP_Text instructionsText;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    [Header("READY / GO!")]
    [SerializeField] private TMP_Text readyGoText;
    [SerializeField] private float readySeconds = 0.7f;
    [SerializeField] private float goSeconds = 0.5f;
    [SerializeField] private float readyGoPopScale = 1.2f;

    private bool gameplayActive = false; 

    [Header("NORMAL: Waves")]
    [SerializeField] private int normalWaves = 11;
    [SerializeField] private float normalIntroDelay = 1.0f;
    [SerializeField] private float normalInterWaveDelay = 0.60f;

    [SerializeField] private Vector2Int normalHitsNeeded = new Vector2Int(8, 14);

    [Header("NORMAL: Scoring")]
    [SerializeField] private int pointsPerHit = 10;
    [SerializeField] private int wrongLanePenalty = -20;

    [Header("NORMAL: Speed Bonus")]
    [SerializeField] private int normalSpeedMaxBonus = 150;
    [SerializeField] private float normalSpeedParSeconds = 1.20f;
    [SerializeField] private int normalSpeedMinBonus = 20;

    [Header("BONUS: Timing")]
    [SerializeField] private float bonusPhaseSeconds = 6.0f;
    [SerializeField] private float bonusIntroDelay = 0.75f;
    [SerializeField] private float goldenRopeDelay = 4.75f;

    [Header("BONUS: Reaction Scoring")]
    [SerializeField] private int goldenReactionMaxPoints = 250;
    [SerializeField] private float goldenReactionDecayPerSecond = 300f;
    [SerializeField] private int goldenReactionMinPoints = 20;

    [Header("FX")]
    [SerializeField] private float flashHold = 0.10f;
    [SerializeField] private float flashFade = 0.18f;

    [Header("Finish Splash")]
    [SerializeField] private TMP_Text finishText;
    [SerializeField] private float finishHoldSeconds = 2.5f;

    private enum Lane { Left = 0, Mid = 1, Right = 2 }

    private struct Slot
    {
        public bool hasSprite;
        public bool isModel;
        public bool isTrash;
        public int hitsToClear;
        public bool cleared;
        public Sprite currentSprite;
    }

    private readonly Dictionary<Lane, Slot> slots = new();

    private int score;
    private bool bonusPhase;

    private bool waveActive = false;
    private float waveStartTime = 0f;
    private int waveHitsRemaining = 0;

    private Coroutine tintPulseCo;
    private bool isShowingOkay = false;

    private Sprite _currentBG;

    private bool goldenActive = false;
    private bool goldenWindowActive = false;
    private float goldenStartTime = 0f;

    private bool resultPhase = false;

    private Vector3 leftBaseScale, midBaseScale, rightBaseScale;

    private void Awake()
    {
        if (leftImage) leftBaseScale = leftImage.rectTransform.localScale;
        if (midImage) midBaseScale = midImage.rectTransform.localScale;
        if (rightImage) rightBaseScale = rightImage.rectTransform.localScale;

        Result = MinigameManager.ResultType.Fail;
        slots[Lane.Left] = new Slot();
        slots[Lane.Mid] = new Slot();
        slots[Lane.Right] = new Slot();
        ResetHUD();

        if (cameraman) cameramanBasePos = cameraman.anchoredPosition;
    }

    private void ResetHUD()
    {
        if (flashOverlay) flashOverlay.gameObject.SetActive(false);

        StopGoldenHintPulse(true);
        if (goldenHint)
        {
            goldenHint.gameObject.SetActive(false);
            goldenHint.rectTransform.localScale = Vector3.one;
            var c = goldenHint.color; c.a = 1f; goldenHint.color = c;
        }

        if (goldenRope) goldenRope.gameObject.SetActive(false);

        if (bonusTintOverlay)
        {
            var c = bonusTintOverlay.color; c.a = 0f;
            bonusTintOverlay.color = c;
            bonusTintOverlay.gameObject.SetActive(true);
        }
        if (bonusParticles) bonusParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (okayCenterText) okayCenterText.gameObject.SetActive(false);
        if (instructionsPanel) instructionsPanel.SetActive(false);
        if (resultPanel) resultPanel.SetActive(false);
        if (finishText) finishText.gameObject.SetActive(false);

        gameplayActive = false;
        ShowCameraman(false);


        HideLane(Lane.Left);
        HideLane(Lane.Mid);
        HideLane(Lane.Right);

        UpdateScoreHUD();

        if (scoreText) scoreText.gameObject.SetActive(true);

        SetBackgroundInstant(bgTransition);
        if (backgroundImage)
        {
            backgroundImage.enabled = true;
            var bc = backgroundImage.color; bc.a = 1f; backgroundImage.color = bc;
        }

        if (!shakeRoot && backgroundImage) shakeRoot = backgroundImage.rectTransform;
        if (shakeRoot)
        {
            shakeRootBasePos = shakeRoot.anchoredPosition;
            shakeRoot.anchoredPosition = shakeRootBasePos;
        }

        if (leftImage) leftBaseScale = leftImage.rectTransform.localScale;
        if (midImage) midBaseScale = midImage.rectTransform.localScale;
        if (rightImage) rightBaseScale = rightImage.rectTransform.localScale;

        waveActive = false;
        goldenWindowActive = false;
        waveHitsRemaining = 0;
        bonusPhase = false;
    }

    private void HideLane(Lane lane)
    {
        Image img = ImageFor(lane);
        if (img) img.gameObject.SetActive(false);
        var s = slots[lane];
        s.hasSprite = s.isModel = s.isTrash = false;
        s.hitsToClear = 0;
        s.cleared = false;
        s.currentSprite = null;
        slots[lane] = s;
    }

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);

        score = 0;
        isShowingOkay = false;

        if (instructionsPanel) instructionsPanel.SetActive(true);
        if (scoreText) scoreText.gameObject.SetActive(false);

        EnsureBackground(bgRuleIntro1);
        if (instructionsText)
        {
            instructionsText.text =
                "Aim! Don't miss out!\nShutter Chance!!";
        }
        yield return new WaitForSecondsRealtime(instructionSeconds * 0.25f);

        EnsureBackground(bgRuleIntro2);
        if (instructionsText)
        {
            instructionsText.text =
                "HOW TO PLAY\n";
        }
        yield return new WaitForSecondsRealtime(instructionSeconds * 0.25f);

        if (instructionsText)
        {
            instructionsText.text =
                "HOW TO PLAY\n" +
                "• Z / X / C to snap.\n" +
                "• MODELS need multiple taps. TRASH is a mistake (-points).\n" +
                "• Clear ALL required taps to finish a wave (OKAY!). Faster = bonus.";
        }
        yield return new WaitForSecondsRealtime(instructionSeconds * 0.25f);

        EnsureBackground(bgRuleIntro3);
        if (instructionsText)
        {
            instructionsText.text =
                 "HOW TO PLAY\n" +
                "• BONUS: spam models;\n" +
                "  when GOLDEN ROPE appears,\n" +
                "  press SPACE fast!";
        }
        yield return new WaitForSecondsRealtime(instructionSeconds * 0.25f);

        EnsureBackground(bgRuleIntro3);
        if (instructionsText)
        {
            instructionsText.text =
                "RATING      SCORE\n" +
                "SSS         2240+\n" +
                "SS          2100\n" +
                "S           2000\n" +
                "A           1900\n" +
                "B           1600\n" +
                "C           1300\n" +
                "D           1100\n" +
                "E           900\n" +
                "F           600\n" +
                "Out of Rank  0";
        }
        yield return new WaitForSecondsRealtime(instructionSeconds * 0.25f);

        if (instructionsPanel) instructionsPanel.SetActive(false);
        if (scoreText) scoreText.gameObject.SetActive(true);
        ShowCameraman(true);
        EnsureBackground(bgTransition);


        yield return ShowReadyGo();
        if (normalIntroDelay > 0f)
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, normalIntroDelay));

        for (int wave = 1; wave <= Mathf.Max(1, normalWaves); wave++)
        {
            int desiredModels;
            if (wave <= 3)
            {
                desiredModels = 1;
            }
            else if (wave <= 6)
            {
                desiredModels = Random.Range(1, 3);
            }
            else
            {
                desiredModels = Random.Range(1, 4);
            }

            SpawnNormalStage(desiredModels);
            EnsureBackground(bgPhoto);

            waveActive = true;
            waveStartTime = Time.unscaledTime;

            while (waveHitsRemaining > 0)
            {
                HandleInput(normalPhase: true);
                yield return null;
            }

            yield return WaveClearedSequence();

            EnsureBackground(bgTransition);
        }

        ClearAllLanes();
        EnsureBackground(bgTransition);

        bonusPhase = true;
        EnterBonusVisuals();
        goldenActive = false;
        goldenWindowActive = false;

        if (goldenHint) goldenHint.gameObject.SetActive(false);

        float bonusTimer = 0f;
        bool feverActivated = false;

        while (bonusTimer < bonusPhaseSeconds)
        {
            float dt = Time.unscaledDeltaTime;
            bonusTimer += dt;

            EnsureBackground(feverActivated ? bgPhoto : bgTransition);

            if (!feverActivated && bonusTimer >= bonusIntroDelay)
            {
                ActivateBonusFeverAllLanes();
                feverActivated = true;
                EnsureBackground(bgPhoto);
            }

            if (!goldenActive && bonusTimer >= goldenRopeDelay)
            {
                goldenActive = true;
                goldenWindowActive = true;
                goldenStartTime = Time.unscaledTime;
                if (goldenRope) goldenRope.gameObject.SetActive(true);

                if (goldenHint)
                {
                    goldenHint.text = "PRESS!";
                    goldenHint.gameObject.SetActive(true);
                    StartGoldenHintPulse();
                }
            }

            if (HandleGoldenPressDuringBonus())
                break;

            HandleInput(normalPhase: false);
            UpdateScoreHUD();
            yield return null;
        }

        ExitBonusVisuals();
        StopGoldenHintPulse(true);
        if (goldenHint) goldenHint.gameObject.SetActive(false);
        if (goldenRope) goldenRope.gameObject.SetActive(false);
        ClearAllLanes();

        EnsureBackground(bgResult);

        if (finishText)
        {
            finishText.rectTransform.SetAsLastSibling();
            finishText.text = "FINISH!!";
            var c = finishText.color; finishText.color = new Color(c.r, c.g, c.b, 0f);
            finishText.gameObject.SetActive(true);

            yield return FadeTMPAlpha(finishText, 0f, 1f, 0.25f);

            yield return new WaitForSecondsRealtime(finishHoldSeconds);

            yield return FadeTMPAlpha(finishText, 1f, 0f, 0.25f);
            finishText.gameObject.SetActive(false);
        }

        var rank = JudgeRank(score);
        Result = (rank == Rank.SSS) ? MinigameManager.ResultType.Perfect
             : (score > 0 ? MinigameManager.ResultType.Success
                          : MinigameManager.ResultType.Fail);

        yield return Flash();

        ShowCameraman(false);
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (resultPanel) resultPanel.SetActive(true);
        if (resultText) resultText.text = $"RESULT\nRank: {rank}\nScore: {score}";

        float rt = 0f;
        while (rt < resultHoldSeconds)
        {
            rt += Time.unscaledDeltaTime;
            yield return null;
        }
        if (resultPanel) resultPanel.SetActive(false);

        BattleManager.instance?.SetBattlePaused(false);
    }

    private IEnumerator ShowReadyGo()
{
    gameplayActive = false;

    if (!readyGoText)
    {
        yield return new WaitForSecondsRealtime(0.8f);
        gameplayActive = true;
        yield break;
    }

    readyGoText.gameObject.SetActive(true);

    yield return PopWord(readyGoText, "READY", readySeconds);

    yield return new WaitForSecondsRealtime(0.1f);

    yield return PopWord(readyGoText, "GO!", goSeconds);

    readyGoText.gameObject.SetActive(false);

    gameplayActive = true;
}

private IEnumerator PopWord(TMP_Text txt, string word, float seconds)
{
    txt.text = word;
    var rt = txt.rectTransform;

    Vector3 startScale = Vector3.one * Mathf.Max(1f, readyGoPopScale);
    Vector3 endScale   = Vector3.one;

    float inTime  = seconds * 0.25f;
    float hold    = seconds * 0.50f;
    float outTime = seconds * 0.25f;

    rt.localScale = startScale;
    Color baseCol = txt.color;
    txt.color = new Color(baseCol.r, baseCol.g, baseCol.b, 0f);

    float t = 0f;
    while (t < inTime)
    {
        t += Time.unscaledDeltaTime;
        float u = Mathf.Clamp01(t / inTime);
        rt.localScale = Vector3.Lerp(startScale, endScale, u);
        txt.color = new Color(baseCol.r, baseCol.g, baseCol.b, u);
        yield return null;
    }
    rt.localScale = endScale;
    txt.color = new Color(baseCol.r, baseCol.g, baseCol.b, 1f);

    if (hold > 0f) yield return new WaitForSecondsRealtime(hold);

    t = 0f;
    while (t < outTime)
    {
        t += Time.unscaledDeltaTime;
        float u = Mathf.Clamp01(t / outTime);
        txt.color = new Color(baseCol.r, baseCol.g, baseCol.b, 1f - u);
        yield return null;
    }
    txt.color = new Color(baseCol.r, baseCol.g, baseCol.b, 0f);
}

    private IEnumerator FadeTMPAlpha(TMP_Text txt, float aFrom, float aTo, float seconds)
    {
        if (!txt) yield break;
        var baseCol = txt.color;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(aFrom, aTo, Mathf.Clamp01(t / seconds));
            txt.color = new Color(baseCol.r, baseCol.g, baseCol.b, a);
            yield return null;
        }
        txt.color = new Color(baseCol.r, baseCol.g, baseCol.b, aTo);
    }

    private IEnumerator WaveClearedSequence()
    {
        if (waveActive)
        {
            float elapsed = Mathf.Max(0f, Time.unscaledTime - waveStartTime);
            float t = (normalSpeedParSeconds <= 0f) ? 1f : (elapsed / normalSpeedParSeconds);
            int spd = Mathf.RoundToInt(Mathf.Lerp(normalSpeedMaxBonus, normalSpeedMinBonus, t));
            spd = Mathf.Clamp(spd, normalSpeedMinBonus, normalSpeedMaxBonus);
            score += spd;
            waveActive = false;
            UpdateScoreHUD();
        }

        foreach (Lane L in System.Enum.GetValues(typeof(Lane)))
        {
            var ss = slots[L];
            ss.hasSprite = false;
            ss.isModel = false;
            ss.isTrash = false;
            ss.cleared = false;
            ss.hitsToClear = 0;
            ss.currentSprite = null;
            slots[L] = ss;
            SetLaneSprite(L, null);
        }

        // OKAY! pop
        yield return ShowOkayCenteredRoutine(Mathf.Max(0f, normalInterWaveDelay));
    }

    private void ActivateBonusFeverAllLanes()
    {
        foreach (Lane lane in System.Enum.GetValues(typeof(Lane)))
        {
            var s = slots[lane];
            s.isTrash = false;
            s.isModel = true;
            s.hasSprite = true;
            s.cleared = false;
            s.hitsToClear = 999999;

            Sprite chosen = PickUniqueSprite(modelSprites, lane);
            s.currentSprite = chosen;
            slots[lane] = s;

            SetLaneSprite(lane, chosen);
        }
    }

    private void StartGoldenHintPulse()
    {
        StopGoldenHintPulse(true);
        if (!goldenHint) return;

        goldenHintBasePos = goldenHint.rectTransform.anchoredPosition;
        goldenHintBaseRot = goldenHint.rectTransform.localRotation;
        goldenHintBaseColor = goldenHint.color;

        goldenHintPulseCo = StartCoroutine(GoldenHintPulseCo());
    }

    private void StopGoldenHintPulse(bool resetTransform = false)
    {
        if (goldenHintPulseCo != null)
        {
            StopCoroutine(goldenHintPulseCo);
            goldenHintPulseCo = null;
        }
        if (goldenHint && resetTransform)
        {
            goldenHint.rectTransform.localScale = Vector3.one;
            goldenHint.rectTransform.anchoredPosition = goldenHintBasePos;
            goldenHint.rectTransform.localRotation = goldenHintBaseRot;
            var c = goldenHintBaseColor; c.a = 1f;
            goldenHint.color = c;
        }
    }

    private IEnumerator GoldenHintPulseCo()
    {
        var rt = goldenHint.rectTransform;

        while (true)
        {
            float tPulse = Time.unscaledTime * (goldenHintPulseHz * Mathf.PI * 2f);
            float tShake = Time.unscaledTime * (goldenHintShakeHz * Mathf.PI * 2f);

            float s = 1f + goldenHintScaleAmp * Mathf.Sin(tPulse);
            rt.localScale = new Vector3(s, s, 1f);

            float alpha = 1f - goldenHintAlphaAmp + goldenHintAlphaAmp * (0.5f + 0.5f * Mathf.Sin(tPulse));
            float colorLerp = 0.5f + 0.5f * Mathf.Sin(tPulse);
            Color col = Color.Lerp(goldenHintColorA, goldenHintColorB, colorLerp);
            col.a = alpha;
            goldenHint.color = col;

            float jx = Mathf.Sin(tShake) * goldenHintShakePx;
            float jy = Mathf.Cos(tShake * 0.9f) * goldenHintShakePx * 0.6f;
            rt.anchoredPosition = goldenHintBasePos + new Vector2(jx, jy);

            float tilt = Mathf.Sin(tShake * 0.75f) * goldenHintTiltDeg;
            rt.localRotation = Quaternion.Euler(0f, 0f, tilt);

            yield return null;
        }
    }

    private void EnsureBackground(Sprite target)
    {
        if (!backgroundImage || target == null) return;

        if (_currentBG == target)
        {
            if (!backgroundImage.enabled) backgroundImage.enabled = true;
            if (backgroundImage.color.a < 0.99f)
            {
                var c = backgroundImage.color; c.a = 1f; backgroundImage.color = c;
            }
            return;
        }
        SetBackgroundInstant(target);
    }

    private void SetBackgroundInstant(Sprite target)
    {
        if (!backgroundImage || target == null) return;
        _currentBG = target;
        backgroundImage.enabled = true;
        backgroundImage.sprite = target;
        var c = backgroundImage.color; c.a = 1f; backgroundImage.color = c;
    }

    private IEnumerator CameramanThumbsUpFor(float seconds)
    {
        if (!cameramanImage || !cameramanThumbsSprite) yield break;

        var original = cameramanImage.sprite;

        cameramanImage.sprite = cameramanThumbsSprite;

        if (cameraman)
        {
            var start = cameraman.localScale;
            var peak = start * 1.07f;
            float t = 0f, up = 0.08f, down = 0.10f;

            while (t < up) { t += Time.unscaledDeltaTime; cameraman.localScale = Vector3.Lerp(start, peak, t / up); yield return null; }
            float hold = Mathf.Max(0f, seconds - up - down);
            if (hold > 0f) yield return new WaitForSecondsRealtime(hold);
            t = 0f;
            while (t < down) { t += Time.unscaledDeltaTime; cameraman.localScale = Vector3.Lerp(peak, start, t / down); yield return null; }
            cameraman.localScale = start;
        }
        else
        {
            yield return new WaitForSecondsRealtime(seconds);
        }

        cameramanImage.sprite = cameramanDefaultSprite ? cameramanDefaultSprite : original;
    }

    private void EnterBonusVisuals()
    {
        if (bonusTintOverlay)
        {
            var c = bonusTintOverlay.color; c.a = 0f;
            bonusTintOverlay.color = c;
            if (tintPulseCo != null) StopCoroutine(tintPulseCo);
            tintPulseCo = StartCoroutine(PulseTint());
        }
        if (bonusParticles) bonusParticles.Play(true);
    }

    private void ExitBonusVisuals()
    {
        if (tintPulseCo != null) { StopCoroutine(tintPulseCo); tintPulseCo = null; }
        if (bonusTintOverlay)
        {
            var c = bonusTintOverlay.color; c.a = 0f;
            bonusTintOverlay.color = c;
        }
        if (bonusParticles) bonusParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (goldenHint) goldenHint.gameObject.SetActive(false);
        if (goldenRope) goldenRope.gameObject.SetActive(false);
    }

    private IEnumerator PulseTint()
    {
        if (!bonusTintOverlay) yield break;
        while (true)
        {
            float a = (Mathf.Sin(Time.unscaledTime * bonusTintPulseSpeed) * 0.5f + 0.5f) * bonusTintAlpha;
            var c = bonusTintOverlay.color; c.a = a;
            bonusTintOverlay.color = c;
            yield return null;
        }
    }

    private void ShowCameraman(bool on)
    {
        if (!cameraman) return;
        cameraman.gameObject.SetActive(on);
        if (on)
        {
            cameraman.localRotation = Quaternion.identity;
            cameraman.anchoredPosition = cameramanBasePos;
        }
    }

    private void TriggerCameramanGag()
    {
        if (!cameraman) return;
        camFlipped = !camFlipped;
        var s = cameraman.localScale;
        s.x = Mathf.Abs(s.x) * (camFlipped ? -1f : 1f);
        cameraman.localScale = s;
        if (camWiggleCo != null) StopCoroutine(camWiggleCo);
        camWiggleCo = StartCoroutine(CamWiggle());
    }

    private IEnumerator CamWiggle()
    {
        float half = Mathf.Max(0.01f, camWiggleSeconds) * 0.5f;
        float target = Random.Range(-camWiggleDegrees, camWiggleDegrees);
        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(0f, target, t / half);
            cameraman.localRotation = Quaternion.Euler(0f, 0f, a);
            yield return null;
        }
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(target, 0f, t / half);
            cameraman.localRotation = Quaternion.Euler(0f, 0f, a);
            yield return null;
        }
        cameraman.localRotation = Quaternion.identity;
        camWiggleCo = null;
    }

    private void TriggerCameramanHop()
    {
        if (!cameraman) return;

        if (camHopCo != null) StopCoroutine(camHopCo);
        cameraman.anchoredPosition = cameramanBasePos;
        camHopCo = StartCoroutine(CamHop());
    }

    private IEnumerator CamHop()
    {
        float dur = Mathf.Max(0.01f, camHopSeconds);
        float t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / dur);

            float y = Mathf.Sin(u * Mathf.PI) * camHopHeight;
            cameraman.anchoredPosition = cameramanBasePos + new Vector2(0f, y);
            yield return null;
        }

        cameraman.anchoredPosition = cameramanBasePos;
        camHopCo = null;
    }

    private void TriggerScreenShake(float scale = 1f)
    {
        if (!shakeRoot) return;
        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ScreenShakeCo(scale));
    }

    private IEnumerator ScreenShakeCo(float scale)
    {
        shakeRoot.anchoredPosition = shakeRootBasePos;

        float time = 0f;
        float dur = Mathf.Max(0.01f, shakeDuration) * Mathf.Clamp(scale, 0.1f, 3f);
        float freq = Mathf.Max(0.01f, shakeFrequency);
        float decay = Mathf.Max(1f, shakeDecay);
        float seed = Random.value * 1000f;

        while (time < dur)
        {
            time += Time.unscaledDeltaTime;
            float t = time / dur;
            float envelope = Mathf.Pow(1f - t, decay);
            float angle = (seed + time) * freq;
            float dx = Mathf.Sin(angle) * (shakeIntensity * scale) * envelope;
            float dy = Mathf.Cos(angle * 0.9f) * (shakeIntensity * 0.8f * scale) * envelope;
            shakeRoot.anchoredPosition = shakeRootBasePos + new Vector2(dx, dy);
            yield return null;
        }

        shakeRoot.anchoredPosition = shakeRootBasePos;
        shakeCo = null;
    }

    private Sprite PickUniqueSprite(Sprite[] pool, Lane currentLane)
    {
        if (pool == null || pool.Length == 0) return null;

        HashSet<Sprite> used = new();
        foreach (var kv in slots)
        {
            if (kv.Key == currentLane) continue;
            if (kv.Value.hasSprite && kv.Value.currentSprite) used.Add(kv.Value.currentSprite);
        }

        List<Sprite> candidates = new(pool);
        candidates.RemoveAll(s => used.Contains(s));
        if (candidates.Count == 0) candidates.AddRange(pool);

        return candidates[Random.Range(0, candidates.Count)];
    }

    private void SpawnNormalStage(int desiredModels)
    {
        waveHitsRemaining = 0;

        List<Lane> lanes = new List<Lane> { Lane.Left, Lane.Mid, Lane.Right };
        Shuffle(lanes);

        int modelsToPlace = Mathf.Clamp(desiredModels, 1, 3);

        for (int i = 0; i < modelsToPlace; i++)
        {
            Lane lane = lanes[i];
            int hits = Random.Range(normalHitsNeeded.x, normalHitsNeeded.y + 1);

            var s = slots[lane];
            s.isModel = true;
            s.isTrash = false;
            s.hasSprite = true;
            s.cleared = false;
            s.hitsToClear = Mathf.Max(1, hits);
            s.currentSprite = PickUniqueSprite(modelSprites, lane);
            slots[lane] = s;

            SetLaneSprite(lane, s.currentSprite);
            waveHitsRemaining += s.hitsToClear;
        }

        for (int i = modelsToPlace; i < 3; i++)
        {
            Lane lane = lanes[i];
            var s = slots[lane];
            s.isModel = false;
            s.isTrash = true;
            s.hasSprite = true;
            s.cleared = false;
            s.hitsToClear = 0;
            s.currentSprite = PickUniqueSprite(trashSprites, lane);
            slots[lane] = s;

            SetLaneSprite(lane, s.currentSprite);
        }
    }

    private void ClearAllLanes()
    {
        foreach (Lane lane in System.Enum.GetValues(typeof(Lane)))
        {
            var s = slots[lane];
            s.hasSprite = s.isModel = s.isTrash = false;
            s.hitsToClear = 0;
            s.cleared = false;
            s.currentSprite = null;
            slots[lane] = s;

            SetLaneSprite(lane, null);
        }
        waveHitsRemaining = 0;
    }

    private void SetLaneSprite(Lane lane, Sprite spriteOrNull)
    {
        var img = ImageFor(lane);
        if (!img) return;

        if (spriteOrNull == null)
        {
            img.gameObject.SetActive(false);
            if (lane == Lane.Left && leftImage)
                leftImage.rectTransform.localScale = leftBaseScale;
            else if (lane == Lane.Mid && midImage)
                midImage.rectTransform.localScale = midBaseScale;
            else if (rightImage)
                rightImage.rectTransform.localScale = rightBaseScale;
        }
        else
        {
            img.sprite = spriteOrNull;
            img.gameObject.SetActive(true);

            ApplyRandomFlip(img, lane);
        }
    }

    private void ApplyRandomFlip(Image img, Lane lane)
    {
        if (!img) return;

        Vector3 baseScale =
            (lane == Lane.Left) ? leftBaseScale :
            (lane == Lane.Mid) ? midBaseScale :
                                   rightBaseScale;

        var rt = img.rectTransform;
        rt.localScale = baseScale;

        var s = slots[lane];
        bool doFlip = false;

        if (s.isModel && flipModels)
            doFlip = Random.value < flipModelsChance;
        else if (s.isTrash && flipTrash)
            doFlip = Random.value < flipTrashChance;

        if (doFlip)
            rt.localScale = new Vector3(-Mathf.Abs(baseScale.x), baseScale.y, baseScale.z);
    }

    private Image ImageFor(Lane lane) => lane switch
    {
        Lane.Left => leftImage,
        Lane.Mid => midImage,
        _ => rightImage
    };

    // ---------- Input & Scoring ----------
    private void HandleInput(bool normalPhase)
    {
        if (!gameplayActive) return;
        if (resultPhase) return;
        if (isShowingOkay) return;

        bool blockLaneSnaps = bonusPhase && goldenWindowActive;

        if (!blockLaneSnaps && Input.GetKeyDown(KeyCode.Z)) { StartCoroutine(Flash()); TriggerCameramanGag(); TriggerCameramanHop(); TriggerScreenShake(); HandleSnap(Lane.Left, normalPhase); }
        if (!blockLaneSnaps && Input.GetKeyDown(KeyCode.X)) { StartCoroutine(Flash()); TriggerCameramanGag(); TriggerCameramanHop(); TriggerScreenShake(); HandleSnap(Lane.Mid, normalPhase); }
        if (!blockLaneSnaps && Input.GetKeyDown(KeyCode.C)) { StartCoroutine(Flash()); TriggerCameramanGag(); TriggerCameramanHop(); TriggerScreenShake(); HandleSnap(Lane.Right, normalPhase); }
    }

    private bool HandleGoldenPressDuringBonus()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return false;

        if (goldenActive && goldenWindowActive)
        {
            float rt = Mathf.Max(0f, Time.unscaledTime - goldenStartTime);
            float raw = goldenReactionMaxPoints - (rt * goldenReactionDecayPerSecond);
            int bonus = Mathf.Max(goldenReactionMinPoints, Mathf.RoundToInt(raw));
            score += bonus;
            UpdateScoreHUD();

            goldenWindowActive = false;
            bonusPhase = false;
            resultPhase = true;

            StopGoldenHintPulse(true);
            if (goldenHint) goldenHint.gameObject.SetActive(false);
            if (goldenRope) goldenRope.gameObject.SetActive(false);

            ClearAllLanes();
            if (leftImage) leftImage.gameObject.SetActive(false);
            if (midImage) midImage.gameObject.SetActive(false);
            if (rightImage) rightImage.gameObject.SetActive(false);

            ExitBonusVisuals();

            resultPhase = true;

            EnsureBackground(bgResult);

            return true;
        }

        return false;
    }

    private void HandleSnap(Lane lane, bool normalPhase)
    {
        var s = slots[lane];

        if (normalPhase && s.isTrash)
        {
            StartCoroutine(FlashColor(Color.red, flashHold * 0.6f, flashFade * 0.6f));
            TriggerScreenShake(0.65f);
            score += wrongLanePenalty;
            UpdateScoreHUD();
            return;
        }

        if (s.isModel)
        {
            if (bonusPhase)
            {
                score += Mathf.Max(0, pointsPerHit);
                UpdateScoreHUD();
                return;
            }

            if (s.cleared) return;

            s.hitsToClear = Mathf.Max(0, s.hitsToClear - 1);
            waveHitsRemaining = Mathf.Max(0, waveHitsRemaining - 1);
            score += Mathf.Max(0, pointsPerHit);

            if (s.hitsToClear <= 0) s.cleared = true;

            slots[lane] = s;
            UpdateScoreHUD();
        }
    }

    private void UpdateScoreHUD()
    {
        if (scoreText) scoreText.text = score.ToString();
    }

    private enum Rank { SSS, SS, S, A, B, C, D, E, F }
    private Rank JudgeRank(int finalScore)
    {
        if (finalScore >= 2240) return Rank.SSS;
        if (finalScore >= 2100) return Rank.SS;
        if (finalScore >= 2000) return Rank.S;    
        if (finalScore >= 1900) return Rank.A;
        if (finalScore >= 1600) return Rank.B;
        if (finalScore >= 1300) return Rank.C;
        if (finalScore >= 1100) return Rank.D;
        if (finalScore >= 900) return Rank.E;
        if (finalScore >= 600) return Rank.F;
        return Rank.F;
    }

    private IEnumerator Flash()
    {
        yield return FlashColor(Color.white, flashHold, flashFade);
    }

    private IEnumerator FlashColor(Color color, float hold, float fade)
    {
        if (!flashOverlay) yield break;

        flashOverlay.gameObject.SetActive(true);
        flashOverlay.color = color;
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, hold));

        float t = 0f;
        float f = Mathf.Max(0.01f, fade);
        Color c0 = color;
        while (t < f)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(1f, 0f, t / f);
            flashOverlay.color = new Color(c0.r, c0.g, c0.b, a);
            yield return null;
        }
        flashOverlay.gameObject.SetActive(false);
    }

    private IEnumerator ShowOkayCenteredRoutine(float delayAfter)
    {
        if (!okayCenterText)
        {
            yield return new WaitForSecondsRealtime(delayAfter);
            yield break;
        }

        isShowingOkay = true;

        bool scoreWasActive = scoreText && scoreText.gameObject.activeSelf;
        bool hintWasActive = goldenHint && goldenHint.gameObject.activeSelf;
        bool ropeWasActive = goldenRope && goldenRope.gameObject.activeSelf;

        if (scoreText) scoreText.gameObject.SetActive(false);
        if (goldenHint) goldenHint.gameObject.SetActive(false);
        if (goldenRope) goldenRope.gameObject.SetActive(false);

        okayCenterText.text = "OKAY!";
        okayCenterText.gameObject.SetActive(true);

        StartCoroutine(CameramanThumbsUpFor(okayDisplayTime));

        yield return new WaitForSecondsRealtime(okayDisplayTime);

        okayCenterText.gameObject.SetActive(false);

        if (scoreText) scoreText.gameObject.SetActive(scoreWasActive);
        if (goldenHint) goldenHint.gameObject.SetActive(hintWasActive);
        if (goldenRope) goldenRope.gameObject.SetActive(ropeWasActive);

        isShowingOkay = false;

        // small inter-wave delay
        if (delayAfter > 0f)
            yield return new WaitForSecondsRealtime(delayAfter);
    }

    private void OnDisable()
    {
        if (shakeRoot)
            shakeRoot.anchoredPosition = shakeRootBasePos;

        if (cameraman)
        {
            cameraman.localRotation = Quaternion.identity;
            cameraman.anchoredPosition = cameramanBasePos;
        }

        StopGoldenHintPulse(true);

        if (camHopCo != null) StopCoroutine(camHopCo);
        if (camWiggleCo != null) StopCoroutine(camWiggleCo);
        if (shakeCo != null) StopCoroutine(shakeCo);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
} 