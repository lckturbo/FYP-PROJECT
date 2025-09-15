using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectView : MonoBehaviour
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

    [Header("LEFT: Bars (0..1 fill)")]
    [SerializeField] private Image healthFill;  // Image type=Filled
    [SerializeField] private Image dmgFill;     // Image type=Filled
    [SerializeField] private Image defFill;     // Image type=Filled

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

    // normalization caps so fills feel consistent in UI
    [Header("UI Normalization (tune in Inspector)")]
    [SerializeField] private float healthMaxUI = 200f;
    [SerializeField] private float damageMaxUI = 50f;

    private int currentIndex;

    void Start()
    {
        if (roster == null || roster.Length == 0)
        {
            Debug.LogWarning("CharacterSelectView: roster is empty.");
            return;
        }

        Bind(0);
        if (selectButton) selectButton.onClick.AddListener(Confirm);
    }

    public void Bind(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, roster.Length - 1);
        var def = roster[currentIndex];
        if (!def || def.stats == null) return;

        // Store for game scene (live preview is nice)
        selectedStore.Set(def, currentIndex);

        // LEFT: texts
        nameText.text = def.displayName;
        descText.text = def.description;

        var s = def.stats;

        // Bars (0..1) – clamp to avoid UI spikes
        if (healthFill) healthFill.fillAmount = Mathf.Clamp01(s.maxHealth / Mathf.Max(1f, healthMaxUI));
        if (dmgFill) dmgFill.fillAmount = Mathf.Clamp01(s.atkDmg / Mathf.Max(1f, damageMaxUI));
        if (defFill) defFill.fillAmount = Mathf.Clamp01(s.attackreduction); // 0..1

        // Values
        if (critRateText) critRateText.text = $"{Mathf.RoundToInt(s.critRate * 100f)}%";
        if (critDmgText) critDmgText.text = $"×{s.critDamage:0.##}";
        if (elemText) elemText.text = s.attackElement.ToString();

        // Resist coloring (green <1, grey =1, red >1)
        TintResist(fireIcon, s.fireRes);
        TintResist(waterIcon, s.waterRes);
        TintResist(grassIcon, s.grassRes);
        TintResist(darkIcon, s.darkRes);
        TintResist(lightIcon, s.lightRes);

        // CENTER: art
        if (artNormal) artNormal.sprite = def.normalArt;
        if (artPixel) artPixel.sprite = def.pixelArt;
    }

    private void TintResist(Image icon, float val)
    {
        if (!icon) return;
        if (val < 0.999f) icon.color = new Color(0.6f, 1f, 0.6f, 1f); // resist
        else if (val > 1.001f) icon.color = new Color(1f, 0.6f, 0.6f, 1f); // weak
        else icon.color = Color.white;                   // neutral
    }

    private void Confirm()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SceneCk2");
    }

    // Exposed for external callers (e.g., circle buttons)
    public int GetCurrentIndex() => currentIndex;
}
