using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ShutterChanceBishi : BaseMinigame
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text goldenHint;
    [SerializeField] private Image flashOverlay;

    [Header("Bonus Visuals")]
    [SerializeField] private Image bonusTintOverlay;
    [SerializeField, Range(0f, 1f)] private float bonusTintAlpha = 0.35f;
    [SerializeField] private float bonusTintPulseSpeed = 2.0f;

    [Header("OKAY! Centered")]
    [SerializeField] private TMP_Text okayCenterText;

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

    [Header("Sprites")]
    [SerializeField] private Sprite[] modelSprites = new Sprite[0];
    [SerializeField] private Sprite[] trashSprites = new Sprite[0];

    [Header("Sprite Variation (Flip)")]
    [SerializeField] private bool flipModels = true;
    [SerializeField, Range(0f, 1f)] private float flipModelsChance = 0.5f;
    [SerializeField] private bool flipTrash = false;
    [SerializeField, Range(0f, 1f)] private float flipTrashChance = 0.25f;

    [Header("Backgrounds (instant swap, no fade)")]
    [SerializeField] private Image backgroundImage;
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
    [SerializeField] private Sprite cameramanFailSprite;

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

    [Header("Gameplay Time Budget (Normal + Bonus)")]
    [SerializeField] private float gameplayTotalSeconds = 18f;
    [SerializeField, Range(0f, 1f)] private float bonusPortion = 0.3235f;
    [SerializeField, Range(0.1f, 0.95f)] private float goldenRopeAtPortion = 0.5455f;

    [Header("Result & Hold Durations")]
    [SerializeField] private float resultHoldSeconds = 4.5f;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    [Header("READY / GO!")]
    [SerializeField] private TMP_Text readyGoText;
    [SerializeField] private float readySeconds = 0.7f;
    [SerializeField] private float goSeconds = 0.5f;
    [SerializeField] private float readyGoPopScale = 1.2f;

    //[Header("Skip")]
    //[SerializeField] private Button skipButton;
    //private bool skipRequested = false;

    private bool gameplayActive = false;

    [Header("NORMAL: Beat Settings (time-boxed)")]
    [SerializeField] private float normalBeatSeconds = 1.25f;
    [SerializeField] private float normalBeatGapSeconds = 0.75f;
    [SerializeField] private Vector2Int normalHitsNeeded = new Vector2Int(8, 12);
    [SerializeField] private int minModelsPerBeat = 1;
    [SerializeField] private int maxModelsPerBeat = 2;

    [Header("NORMAL: Scoring")]
    [SerializeField] private int pointsPerHit = 10;
    [SerializeField] private int wrongLanePenalty = -10;

    [Header("BONUS: Reaction Scoring")]
    [SerializeField] private int goldenReactionMaxPoints = 250;
    [SerializeField] private float goldenReactionDecayPerSecond = 300f;
    [SerializeField] private int goldenReactionMinPoints = 20;

    [Header("FX")]
    [SerializeField] private float flashHold = 0.10f;
    [SerializeField] private float flashFade = 0.18f;

    [Header("Finish Splash")]
    [SerializeField] private TMP_Text finishText;
    [SerializeField] private float finishHoldSeconds = 2.0f;

    [Header("End-of-Normal Alignment")]
    [SerializeField, Range(0.2f, 1f)] private float endAlignMinBeatFraction = 0.35f;
    [SerializeField] private float endOKAYExtraSeconds = 0.12f;
    //[SerializeField] private float bonusIntroHold = 0.45f;

    [Header("Animator")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject animationPanel;

    // ===================== Intro / Instructions Video =====================
    [Header("Intro / Instructions Video")]
    [SerializeField] private bool useIntroVideo = true;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoRoot;   // object that shows the video (screen / RawImage / etc.)

    // ========== Result thresholds (3-tier) ==========
    [Header("Result Thresholds (3-tier)")]
    [SerializeField] private int perfectScore = 1350;
    [SerializeField] private int successScore = 600;

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

    private int waveHitsRemaining = 0;

    private Coroutine tintPulseCo;
    private bool isShowingOkay = false;

    private Sprite _currentBG;

    private bool goldenActive = false;
    private bool goldenWindowActive = false;
    private float goldenStartTime = 0f;

    private bool resultPhase = false;

    private Vector3 leftBaseScale, midBaseScale, rightBaseScale;

    private enum BeatOutcome { Good, Miss, Afk }
    private int hitsThisBeat = 0;
    private int missesThisBeat = 0;
    private BeatOutcome lastBeatOutcome = BeatOutcome.Good;

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

        if (okayCenterText) okayCenterText.gameObject.SetActive(false);
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

        waveHitsRemaining = 0;
        bonusPhase = false;

        skipRequested = false;
        SetupSkipUI(false);

        // ensure video object starts hidden
        if (videoRoot) videoRoot.SetActive(false);
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

        // ===== Existing intro animation =====
        if (animationPanel) animationPanel.SetActive(true);

        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(3.3f);
        }

        if (animationPanel) animationPanel.SetActive(false);

        // ===== Reset / start state =====
        score = 0;
        isShowingOkay = false;

        if (scoreText) scoreText.gameObject.SetActive(false);

        // Use video instead of text instructions
        SetupSkipUI(true);                 // allow skipping the video
        yield return PlayIntroVideo();     // will stop early if skipRequested

        SetupSkipUI(false);                // no skip during gameplay
        skipRequested = false;             // reset skip flag

        if (scoreText) scoreText.gameObject.SetActive(true);
        ShowCameraman(true);
        EnsureBackground(bgTransition);

        // READY / GO! (only after video is done or skipped)
        yield return ShowReadyGo();

        float desiredBonusSeconds = Mathf.Clamp(
            gameplayTotalSeconds * bonusPortion,
            0.5f,
            Mathf.Max(0.5f, gameplayTotalSeconds - 0.5f)
        );
        float normalBudget = Mathf.Max(0f, gameplayTotalSeconds - desiredBonusSeconds);

        float normalClock = 0f;
        float beatClock = 0f;

        SpawnBeatLayout(0f);
        EnsureBackground(bgPhoto);

        bool endingNormal = false;

        while (!endingNormal)
        {
            float dt = Time.unscaledDeltaTime;
            normalClock += dt;
            beatClock += dt;

            HandleInput(normalPhase: true);

            if (normalClock >= normalBudget)
            {
                float timeToNextOKAY = Mathf.Max(0f, normalBeatSeconds - beatClock);
                while (timeToNextOKAY > 0f)
                {
                    float d = Time.unscaledDeltaTime;
                    normalClock += d;
                    timeToNextOKAY -= d;
                    HandleInput(normalPhase: true);
                    UpdateScoreHUD();
                    yield return null;
                }

                if (hitsThisBeat <= 0) lastBeatOutcome = BeatOutcome.Afk;
                else if (missesThisBeat > hitsThisBeat) lastBeatOutcome = BeatOutcome.Miss;
                else lastBeatOutcome = BeatOutcome.Good;

                ClearAllLanes();
                yield return OKAYBeatFlash(normalBeatGapSeconds + endOKAYExtraSeconds);

                endingNormal = true;
                break;
            }

            if (beatClock >= normalBeatSeconds)
            {
                beatClock -= normalBeatSeconds;

                if (hitsThisBeat <= 0) lastBeatOutcome = BeatOutcome.Afk;
                else if (missesThisBeat > hitsThisBeat) lastBeatOutcome = BeatOutcome.Miss;
                else lastBeatOutcome = BeatOutcome.Good;

                ClearAllLanes();
                yield return OKAYBeatFlash(normalBeatGapSeconds);

                float remaining = Mathf.Max(0f, normalBudget - normalClock);
                float minNeeded = (normalBeatSeconds * endAlignMinBeatFraction) +
                                  (normalBeatGapSeconds * 0.25f);

                if (remaining < minNeeded)
                {
                    endingNormal = true;
                }
                else
                {
                    float p = (normalBudget <= 0.001f)
                        ? 1f
                        : Mathf.Clamp01(normalClock / normalBudget);
                    SpawnBeatLayout(p);
                    EnsureBackground(bgPhoto);
                }
            }

            UpdateScoreHUD();
            yield return null;
        }

        ClearAllLanes();
        EnsureBackground(bgTransition);

        // ========== BONUS PHASE ==========
        bonusPhase = true;
        EnterBonusVisuals();
        goldenActive = false;
        goldenWindowActive = false;
        if (goldenHint) goldenHint.gameObject.SetActive(false);

        float bonusWindow = desiredBonusSeconds;
        float bonusTimer = 0f;
        float goldenAppearAt = Mathf.Clamp(
            bonusWindow * goldenRopeAtPortion,
            0.15f,
            Mathf.Max(0.2f, bonusWindow - 0.1f)
        );
        bool feverActivated = false;

        while (bonusTimer < bonusWindow)
        {
            float dt = Time.unscaledDeltaTime;
            bonusTimer += dt;

            EnsureBackground(feverActivated ? bgPhoto : bgTransition);

            if (!feverActivated)
            {
                ActivateBonusFeverAllLanes();
                feverActivated = true;
                EnsureBackground(bgPhoto);
            }

            if (!goldenActive && bonusTimer >= goldenAppearAt)
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
            var c = finishText.color;
            finishText.color = new Color(c.r, c.g, c.b, 0f);
            finishText.gameObject.SetActive(true);

            yield return FadeTMPAlpha(finishText, 0f, 1f, 0.25f);
            yield return new WaitForSecondsRealtime(finishHoldSeconds);
            yield return FadeTMPAlpha(finishText, 1f, 0f, 0.25f);
            finishText.gameObject.SetActive(false);
        }

        // ======= Final 3-tier result only =======
        Result = (score >= perfectScore)
            ? MinigameManager.ResultType.Perfect
            : (score >= successScore)
                ? MinigameManager.ResultType.Success
                : MinigameManager.ResultType.Fail;

        yield return Flash();

        ShowCameraman(false);
        if (scoreText) scoreText.gameObject.SetActive(false);
        if (resultPanel) resultPanel.SetActive(true);
        if (resultText) resultText.text = $"RESULT\n{Result}\nScore: {score}";

        float rt = 0f;
        while (rt < resultHoldSeconds)
        {
            rt += Time.unscaledDeltaTime;
            yield return null;
        }
        if (resultPanel) resultPanel.SetActive(false);

        BattleManager.instance?.SetBattlePaused(false);
    }

    // ===================== Intro Video Coroutine =====================
    private IEnumerator PlayIntroVideo()
    {
        if (!useIntroVideo || !videoPlayer)
            yield break;

        // Silent video
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        // Show the video screen / object
        if (videoRoot) videoRoot.SetActive(true);

        skipRequested = false;

        videoPlayer.Stop();
        videoPlayer.time = 0.0;
        videoPlayer.Prepare();

        // Wait until prepared or skipped
        while (!videoPlayer.isPrepared && !skipRequested)
            yield return null;

        if (skipRequested)
        {
            try { videoPlayer.Stop(); } catch { }
            if (videoRoot) videoRoot.SetActive(false);
            yield break;
        }

        // Play
        videoPlayer.Play();

        // Wait until finished or skipped
        while (videoPlayer.isPlaying && !skipRequested)
            yield return null;

        try { videoPlayer.Stop(); } catch { }

        // Hide video object so it doesn't sit in the background
        if (videoRoot) videoRoot.SetActive(false);
    }

    private void SpawnBeatLayout(float progress01)
    {
        hitsThisBeat = 0;
        missesThisBeat = 0;
        lastBeatOutcome = BeatOutcome.Good;

        int models = Mathf.RoundToInt(
            Mathf.Lerp(minModelsPerBeat, maxModelsPerBeat, Mathf.Clamp01(progress01))
        );
        models = Mathf.Clamp(models, 1, 3);

        ClearAllLanes();

        List<Lane> lanes = new List<Lane> { Lane.Left, Lane.Mid, Lane.Right };
        Shuffle(lanes);

        int placed = 0;
        for (int i = 0; i < lanes.Count; i++)
        {
            Lane lane = lanes[i];

            if (placed < models)
            {
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
                placed++;
            }
            else
            {
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

        waveHitsRemaining = 0;
        foreach (var kv in slots)
            if (kv.Value.isModel) waveHitsRemaining += Mathf.Max(0, kv.Value.hitsToClear);
    }

    private IEnumerator OKAYBeatFlash(float gapSeconds)
    {
        isShowingOkay = true;

        bool scoreWasActive = scoreText && scoreText.gameObject.activeSelf;
        bool hintWasActive = goldenHint && goldenHint.gameObject.activeSelf;
        bool ropeWasActive = goldenRope && goldenRope.gameObject.activeSelf;

        if (scoreText) scoreText.gameObject.SetActive(false);
        if (goldenHint) goldenHint.gameObject.SetActive(false);
        if (goldenRope) goldenRope.gameObject.SetActive(false);

        string label;
        Color labelColor;
        Sprite desiredCamSprite = cameramanDefaultSprite;

        if (lastBeatOutcome == BeatOutcome.Good)
        {
            label = "OKAY!";
            labelColor = new Color(0.3f, 1f, 0.45f, 1f);
            if (cameramanThumbsSprite) desiredCamSprite = cameramanThumbsSprite;
        }
        else
        {
            label = "MISS!";
            labelColor = new Color(1f, 0.3f, 0.3f, 1f);
            if (cameramanFailSprite) desiredCamSprite = cameramanFailSprite;
        }

        if (okayCenterText && gapSeconds > 0f)
        {
            okayCenterText.text = label;
            var baseCol = okayCenterText.color;
            okayCenterText.color = new Color(
                labelColor.r, labelColor.g, labelColor.b, baseCol.a
            );
            okayCenterText.gameObject.SetActive(true);
        }

        Sprite prevCamSprite = null;
        Vector3 prevCamScale = Vector3.one;

        if (cameramanImage) prevCamSprite = cameramanImage.sprite;
        if (cameraman) prevCamScale = cameraman.localScale;

        if (cameramanImage && desiredCamSprite)
            cameramanImage.sprite = desiredCamSprite;

        float show = Mathf.Clamp(
            gapSeconds * 0.8f, 0.06f, Mathf.Max(0.06f, gapSeconds)
        );
        float blank = Mathf.Max(0f, gapSeconds - show);

        float popUp = Mathf.Min(0.10f, show * 0.35f);
        float popDown = Mathf.Min(0.12f, show * 0.35f);
        float hold = Mathf.Max(0f, show - popUp - popDown);

        if (cameraman)
        {
            float t = 0f;
            Vector3 peak = prevCamScale * 1.07f;
            while (t < popUp)
            {
                t += Time.unscaledDeltaTime;
                cameraman.localScale = Vector3.Lerp(
                    prevCamScale, peak, t / Mathf.Max(0.01f, popUp)
                );
                yield return null;
            }
            if (hold > 0f) yield return new WaitForSecondsRealtime(hold);
            t = 0f;
            while (t < popDown)
            {
                t += Time.unscaledDeltaTime;
                cameraman.localScale = Vector3.Lerp(
                    peak, prevCamScale, t / Mathf.Max(0.01f, popDown)
                );
                yield return null;
            }
            cameraman.localScale = prevCamScale;
        }
        else
        {
            if (show > 0f) yield return new WaitForSecondsRealtime(show);
        }

        if (okayCenterText) okayCenterText.gameObject.SetActive(false);
        if (blank > 0f) yield return new WaitForSecondsRealtime(blank);

        if (cameramanImage)
            cameramanImage.sprite = cameramanDefaultSprite
                                    ? cameramanDefaultSprite
                                    : prevCamSprite;

        if (scoreText) scoreText.gameObject.SetActive(scoreWasActive);
        if (goldenHint) goldenHint.gameObject.SetActive(hintWasActive);
        if (goldenRope) goldenRope.gameObject.SetActive(ropeWasActive);

        isShowingOkay = false;
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
        Vector3 endScale = Vector3.one;

        float inTime = seconds * 0.25f;
        float hold = seconds * 0.50f;
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

            float alpha = 1f - goldenHintAlphaAmp +
                          goldenHintAlphaAmp *
                          (0.5f + 0.5f * Mathf.Sin(tPulse));
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

    private void EnterBonusVisuals()
    {
        if (bonusTintOverlay)
        {
            var c = bonusTintOverlay.color;
            c.a = 0f;
            bonusTintOverlay.color = c;

            if (tintPulseCo != null)
                StopCoroutine(tintPulseCo);

            tintPulseCo = StartCoroutine(PulseTint());
        }
    }

    private void ExitBonusVisuals()
    {
        if (tintPulseCo != null)
        {
            StopCoroutine(tintPulseCo);
            tintPulseCo = null;
        }

        if (bonusTintOverlay)
        {
            var c = bonusTintOverlay.color;
            c.a = 0f;
            bonusTintOverlay.color = c;
        }

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

    private void HandleInput(bool normalPhase)
    {
        if (!gameplayActive) return;
        if (resultPhase) return;
        if (isShowingOkay) return;

        bool blockLaneSnaps = bonusPhase && goldenWindowActive;

        if (!blockLaneSnaps && Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(Flash());
            TriggerCameramanGag();
            TriggerCameramanHop();
            TriggerScreenShake();
            HandleSnap(Lane.Left, normalPhase);
        }
        if (!blockLaneSnaps && Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(Flash());
            TriggerCameramanGag();
            TriggerCameramanHop();
            TriggerScreenShake();
            HandleSnap(Lane.Mid, normalPhase);
        }
        if (!blockLaneSnaps && Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(Flash());
            TriggerCameramanGag();
            TriggerCameramanHop();
            TriggerScreenShake();
            HandleSnap(Lane.Right, normalPhase);
        }
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
            missesThisBeat++;
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
            hitsThisBeat++;

            if (s.hitsToClear <= 0) s.cleared = true;

            slots[lane] = s;
            UpdateScoreHUD();
        }
    }

    private void UpdateScoreHUD()
    {
        if (scoreText) scoreText.text = score.ToString();
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

        // Safety: stop video & hide root if this object is disabled
        if (videoPlayer)
        {
            try { videoPlayer.Stop(); } catch { }
        }
        if (videoRoot) videoRoot.SetActive(false);
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
