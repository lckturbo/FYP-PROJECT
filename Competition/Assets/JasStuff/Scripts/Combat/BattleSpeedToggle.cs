using UnityEngine;
using UnityEngine.UI;

public class BattleSpeedToggle : MonoBehaviour
{
    [SerializeField] private TurnEngine engine;
    [SerializeField] private Toggle toggle;
    [SerializeField] private Text label;
    [SerializeField] private int unlockLevel = 2; 

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
    private void Update()
    {
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
            engine.BattleSpeed = 1f;
        }
        else
        {
            toggle.isOn = !Mathf.Approximately(engine.BattleSpeed, 1f);
        }
    }

    private void OnToggle(bool isOn)
    {
        if (!engine) return;

        int currentLevel = PartyLevelSystem.Instance != null
            ? PartyLevelSystem.Instance.levelSystem.level
            : 1;

        if (currentLevel >= unlockLevel)
        {
            engine.BattleSpeed = isOn ? 2f : 1f;
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
            label.text = $"Speed: LOCKED (Lv{unlockLevel})";
        }
        else
        {
            label.text = $"{engine.BattleSpeed}x Speed";
        }
    }
}