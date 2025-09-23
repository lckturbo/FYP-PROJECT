using UnityEngine;
using UnityEngine.UI;

public class AutoBattleToggle : MonoBehaviour
{
    [SerializeField] private TurnEngine engine;
    [SerializeField] private Toggle toggle;       // Use a Toggle
    [SerializeField] private Text label;          // Optional text

    private void Awake()
    {
        if (toggle)
        {
            toggle.onValueChanged.AddListener(OnToggle);
            if (engine) toggle.isOn = engine.autoBattle;
            UpdateLabel();
        }
    }

    private void OnToggle(bool value)
    {
        if (engine) engine.autoBattle = value;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (!label || !engine) return;
        label.text = engine.autoBattle ? "Auto: ON" : "Auto: OFF";
    }
}
