using UnityEngine;
using UnityEngine.UI;

public class BattleSpeedToggle : MonoBehaviour
{
    [SerializeField] private TurnEngine engine;
    [SerializeField] private Toggle toggle;
    [SerializeField] private Text label;
    private void Awake()
    {
        if (toggle)
        {
            toggle.onValueChanged.AddListener(OnToggle);
            if (engine)
                toggle.isOn = !Mathf.Approximately(engine.BattleSpeed, 1f);
            UpdateLabel();
        }
    }

    private void OnToggle(bool isOn)
    {
        if (!engine) return;

        engine.BattleSpeed = isOn ? 2f : 1f;
        UpdateLabel();
    }


    private void UpdateLabel()
    {
        if (!label || !engine) return;
        label.text = $"{engine.BattleSpeed}x Speed";
    }
}
