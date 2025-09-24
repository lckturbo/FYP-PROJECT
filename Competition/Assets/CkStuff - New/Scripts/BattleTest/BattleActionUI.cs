using UnityEngine;
using UnityEngine.UI;

public class BattleActionUI : MonoBehaviour
{
    [SerializeField] private TurnEngine engine;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button attackBtn;
    [SerializeField] private Button skillBtn;

    private void Awake()
    {
        if (panel) panel.SetActive(false);
        if (attackBtn) attackBtn.onClick.AddListener(OnAttack);
        if (skillBtn) skillBtn.onClick.AddListener(OnSkill);
    }

    private void OnEnable()
    {
        if (engine) engine.OnLeaderTurnStart += ShowForLeader;
    }
    private void OnDisable()
    {
        if (engine) engine.OnLeaderTurnStart -= ShowForLeader;
    }

    private void ShowForLeader(Combatant leader)
    {
        if (panel) panel.SetActive(true);
    }

    private void OnAttack()
    {
        if (panel) panel.SetActive(false);
        engine.LeaderChooseBasicAttack();
    }

    // placeholder; same as attack for now
    private void OnSkill()
    {
        if (panel) panel.SetActive(false);
        engine.LeaderChooseSkill(0);
    }
}
