using UnityEngine;
using TMPro;

public class BattleTimerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private TMP_Text timeText;

    private bool _visible = false;

    private void Awake()
    {
        if (!battleSystem) battleSystem = FindObjectOfType<BattleSystem>(true);
        if (!timeText) timeText = GetComponent<TMP_Text>();

        UpdateDisplay(0f);
    }

    private void OnEnable()
    {
        BattleSystem.OnLeaderSpawned += HandleBattleStart;
        BattleManager.OnGlobalBattleEnd += HandleBattleEnd;

        if (battleSystem != null && battleSystem.enabled && battleSystem.gameObject.activeInHierarchy)
        {
            _visible = true;
            gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        BattleSystem.OnLeaderSpawned -= HandleBattleStart;
        BattleManager.OnGlobalBattleEnd -= HandleBattleEnd;
    }

    private void Update()
    {
        if (_visible && battleSystem != null)
            UpdateDisplay(battleSystem.BattleElapsed);
    }

    private void HandleBattleStart(GameObject leader)
    {
        _visible = true;
        gameObject.SetActive(true);
        UpdateDisplay(0f);
    }

    private void HandleBattleEnd(string reason, bool victory)
    {
        _visible = false;
        gameObject.SetActive(false);
    }

    private void UpdateDisplay(float time)
    {
        if (!timeText) return;

        int minutes = (int)(time / 60f);
        int seconds = (int)(time % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }
}
