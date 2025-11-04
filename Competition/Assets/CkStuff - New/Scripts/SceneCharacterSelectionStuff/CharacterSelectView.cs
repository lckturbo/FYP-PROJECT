using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectView : MonoBehaviour, IDataPersistence
{
    [Header("Data")]
    [SerializeField] private NewCharacterDefinition[] roster;
    [SerializeField] private SelectedCharacter selectedStore;

    [Header("LEFT: Labels")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text critRateText;
    [SerializeField] private TMP_Text critDmgText;
    [SerializeField] private TMP_Text elemText;

    [Header("LEFT: Segmented Bars (10 blocks each)")]
    [SerializeField] private Transform healthBarContainer;
    [SerializeField] private Transform dmgBarContainer;
    [SerializeField] private Transform defBarContainer;

    [Header("LEFT: Resist icons (tint only)")]
    [SerializeField] private Image fireIcon;
    [SerializeField] private Image waterIcon;
    [SerializeField] private Image grassIcon;
    [SerializeField] private Image darkIcon;
    [SerializeField] private Image lightIcon;

    [Header("CENTER: Art Images")]
    [SerializeField] private Image artNormal;
    [SerializeField] private Image artPixel;

    [Header("Buttons")]
    [SerializeField] private Button selectButton;

    [Header("UI Normalization")]
    [SerializeField] private bool autoCapStats = true;
    [SerializeField] private float capHeadroom = 0.5f;
    [SerializeField] private float healthMaxUI;
    [SerializeField] private float damageMaxUI;
    [SerializeField] private float defenseMaxUI;

    private int currentIndex;

    private void Start()
    {
        if (roster == null || roster.Length == 0)
        {
            Debug.LogWarning("CharacterSelectView: roster is empty.");
            return;
        }

        if (autoCapStats)
            ComputeAutoCapsFromRoster();

        Bind(0);

        if (selectButton) selectButton.onClick.AddListener(Confirm);
    }

    private void ComputeAutoCapsFromRoster()
    {
        float maxHP = 1f;
        float maxATK = 1f;
        float maxDEF = 1f;

        foreach (var def in roster)
        {
            if (!def || def.stats == null) continue;
            var s = def.stats;
            if (s.maxHealth > maxHP) maxHP = s.maxHealth;
            if (s.atkDmg > maxATK) maxATK = s.atkDmg;
            if (s.attackreduction > maxDEF) maxDEF = s.attackreduction;
        }

        float hr = Mathf.Max(1.0f, capHeadroom);
        healthMaxUI = Mathf.Max(1f, maxHP * hr);
        damageMaxUI = Mathf.Max(1f, maxATK * hr);
        defenseMaxUI = Mathf.Max(1f, maxDEF * hr);
    }

    public void Bind(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, roster.Length - 1);
        var def = roster[currentIndex];
        if (!def || def.stats == null) return;

        selectedStore.Set(def, currentIndex);
        var allies = new List<NewCharacterDefinition>();
        allies.Add(def); // leader
        for (int i = 0; i <= roster.Length; i++)
        {
            if (i == currentIndex) continue;
            if (allies.Count > 2) break;
            allies.Add(roster[i]);
        }
        PlayerParty.instance.SetupParty(def, allies);

        nameText.text = def.displayName;
        descText.text = def.description;

        var s = def.stats;

        float hpRatio = s.maxHealth / Mathf.Max(1f, healthMaxUI);
        float dmgRatio = s.atkDmg / Mathf.Max(1f, damageMaxUI);
        float defRatio = s.attackreduction / Mathf.Max(1f, defenseMaxUI);

        UpdateBarSegments(healthBarContainer, hpRatio);
        UpdateBarSegments(dmgBarContainer, dmgRatio);
        UpdateBarSegments(defBarContainer, defRatio);

        if (critRateText) critRateText.text = $"{Mathf.RoundToInt(s.critRate * 100f)}%";
        if (critDmgText) critDmgText.text = $"×{s.critDamage:0.##}";
        if (elemText) elemText.text = s.attackElement.ToString();

        // Resist coloring
        TintResist(fireIcon, s.fireRes);
        TintResist(waterIcon, s.waterRes);
        TintResist(grassIcon, s.grassRes);
        TintResist(darkIcon, s.darkRes);
        TintResist(lightIcon, s.lightRes);

        // Art
        if (artNormal) artNormal.sprite = def.normalArt;
        if (artPixel) artPixel.sprite = def.pixelArt;
    }

private void UpdateBarSegments(Transform container, float ratio)
{
    if (!container) return;
    int total = container.childCount;
    int activeCount = Mathf.RoundToInt(ratio * total);

    // Calculate spacing
    float totalWidth = ((RectTransform)container).rect.width;
    float segmentWidth = total > 0 ? totalWidth / total : 0f;

    for (int i = 0; i < total; i++)
    {
        var seg = container.GetChild(i);
        var img = seg.GetComponent<Image>();
        if (img)
        {
            img.color = (i < activeCount)
                ? new Color(0.8156863f, 0.5254902f, 0.3372549f, 1)
                : new Color(1, 1, 1, 1);
        }

        // Position evenly
        if (seg.TryGetComponent<RectTransform>(out RectTransform rect))
        {
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2((i + 0.5f) * segmentWidth, 0f);
        }
    }
}


    private void TintResist(Image icon, float val)
    {
        if (!icon) return;
        if (val < 0.999f) icon.color = new Color(0.6f, 1f, 0.6f, 1f);
        else if (val > 1.001f) icon.color = new Color(1f, 0.6f, 0.6f, 1f);
        else icon.color = Color.black;
    }

    private void Confirm()
    {
        SaveLoadSystem.instance.SaveGame();
        GameManager.instance.ChangeScene("SampleScene");
    }

    public int GetCurrentIndex() => currentIndex;

    public void LoadData(GameData data) { }

    public void SaveData(ref GameData data)
    {
        data.selectedCharacterIndex = currentIndex; // leader

        var party = PlayerParty.instance.GetFullParty();
        foreach (var memberDef in party)
        {
            if (memberDef == roster[currentIndex]) continue;

            int index = System.Array.IndexOf(roster, memberDef);
            if (index >= 0)
                data.allyIndices.Add(index); // allies
        }
    }
}
