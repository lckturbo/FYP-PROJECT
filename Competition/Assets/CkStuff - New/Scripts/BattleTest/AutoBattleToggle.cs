using UnityEngine;
using UnityEngine.UI;

public class AutoBattleToggle : MonoBehaviour
{
    [SerializeField] private TurnEngine engine;
    [SerializeField] private Toggle toggle;
    [SerializeField] private Text label;
    [SerializeField] private int unlockLevel = 3; 

    private void Awake()
    {
        if (toggle)
        {
            toggle.onValueChanged.AddListener(OnToggle);
            UpdateToggleState();
            UpdateLabel();
        }
    }

    private void OnEnable()
    {
        UpdateToggleState();
        UpdateLabel();
    }

    private void Start()
    {
        UpdateToggleState();
        UpdateLabel();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
            unlockLevel = 1;

        UpdateToggleState();
        UpdateLabel();
    }

    private void UpdateToggleState()
    {
        if (!toggle || !engine) return;

        int currentLevel = PartyLevelSystem.Instance != null
            ? PartyLevelSystem.Instance.levelSystem.level
            : 1;

        bool isUnlocked = currentLevel >= unlockLevel;
        toggle.interactable = isUnlocked;

        if (!isUnlocked)
        {
            toggle.isOn = false;
            engine.autoBattle = false;
        }
        else
        {
            toggle.isOn = engine.autoBattle;
        }
    }

    private void OnToggle(bool value)
    {
        if (!engine) return;

        int currentLevel = PartyLevelSystem.Instance != null
            ? PartyLevelSystem.Instance.levelSystem.level
            : 1;

        if (currentLevel >= unlockLevel)
        {
            engine.autoBattle = value;
            UpdateLabel();
        }
    }

    private void UpdateLabel()
    {
        if (!label || !engine) return;

        int currentLevel = PartyLevelSystem.Instance != null
            ? PartyLevelSystem.Instance.levelSystem.level
            : 1;

        if (currentLevel < unlockLevel)
        {
            label.text = $"Auto: LOCKED (Lv{unlockLevel})";
        }
        else
        {
            label.text = engine.autoBattle ? "Auto: ON" : "Auto: OFF";
        }
    }
}