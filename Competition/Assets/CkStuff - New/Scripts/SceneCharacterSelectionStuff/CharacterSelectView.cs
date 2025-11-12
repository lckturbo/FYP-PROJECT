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
    [SerializeField] private Image elemIcon;


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

    [Header("LEFT: Resist labels")]
    [SerializeField] private TMP_Text fireLabel;
    [SerializeField] private TMP_Text waterLabel;
    [SerializeField] private TMP_Text grassLabel;
    [SerializeField] private TMP_Text darkLabel;
    [SerializeField] private TMP_Text lightLabel;

    [Header("LEFT: Character Icon")]
    [SerializeField] private Image characterIcon;

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
        //var allies = new List<NewCharacterDefinition>();
        //allies.Add(def); // leader
        //for (int i = 0; i < roster.Length; i++)
        //{
        //    if (i == currentIndex) continue;
        //    if (allies.Count > 2) break;
        //    allies.Add(roster[i]);
        //}
        //PlayerParty.instance.SetupParty(def, allies);

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
        if (elemIcon)
        {
            elemIcon.enabled = true;
            elemIcon.sprite = GetElementSprite(s.attackElement);
        }


        TintResist(fireIcon, fireLabel, s.fireRes);
        TintResist(waterIcon, waterLabel, s.waterRes);
        TintResist(grassIcon, grassLabel, s.grassRes);
        TintResist(darkIcon, darkLabel, s.darkRes);
        TintResist(lightIcon, lightLabel, s.lightRes);


        // Art
        if (artNormal) artNormal.sprite = def.normalArt;
        if (artPixel) artPixel.sprite = def.pixelArt;

        // Icon
        if (characterIcon && def.icon)
        {
            characterIcon.sprite = def.icon;
            characterIcon.enabled = true;
        }
    }

    private Sprite GetElementSprite(NewElementType element)
    {
        return element switch
        {
            NewElementType.Fire => fireIcon ? fireIcon.sprite : null,
            NewElementType.Water => waterIcon ? waterIcon.sprite : null,
            NewElementType.Grass => grassIcon ? grassIcon.sprite : null,
            NewElementType.Dark => darkIcon ? darkIcon.sprite : null,
            NewElementType.Light => lightIcon ? lightIcon.sprite : null,
            _ => null
        };
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


    private void TintResist(Image icon, TMP_Text label, float val)
    {
        if (icon)
        {
            if (val < 0.999f) icon.color = new Color(0.6f, 1f, 0.6f, 1f);      // Weak (green)
            else if (val > 1.001f) icon.color = new Color(1f, 0.6f, 0.6f, 1f);   // Strong (red)
            else icon.color = Color.black;                                       // Neutral
        }

        if (label)
        {
            if (val < 0.999f) label.color = Color.green;
            else if (val > 1.001f) label.color = Color.red;
            else label.color = Color.black;
        }
    }


    private void Confirm()
    {
        var def = roster[currentIndex];
        var allies = new List<NewCharacterDefinition>();

        for (int i = 0; i < roster.Length; i++)
        {
            if (i == currentIndex) continue; // Skip the leader
            allies.Add(roster[i]); // Add all other roster members as allies
        }

        PlayerParty.instance.SetupParty(def, allies);

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
