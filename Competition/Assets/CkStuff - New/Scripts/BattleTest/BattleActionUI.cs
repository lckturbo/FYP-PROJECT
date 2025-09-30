using UnityEngine;
using UnityEngine.UI;

public class BattleActionUI : MonoBehaviour
{
    [SerializeField] private TurnEngine engine;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button attackBtn;
    [SerializeField] private Button skillBtn;
    [SerializeField] private Button skill2Btn;
    [SerializeField] private TargetSelector selector;

    private void Awake()
    {
        if (panel) panel.SetActive(false);
        if (attackBtn) attackBtn.onClick.AddListener(OnAttack);
        if (skillBtn) skillBtn.onClick.AddListener(OnSkill1);
        if (skill2Btn) skill2Btn.onClick.AddListener(OnSkill2);
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
        selector?.Clear();
        if (panel) panel.SetActive(true);
    }

    private void OnAttack()
    {
        var target = selector ? selector.Current : null;
        if (panel) panel.SetActive(false);
        engine.LeaderChooseBasicAttackTarget(target);
    }

    private void OnSkill1()
    {
        var target = selector ? selector.Current : null;
        if (panel) panel.SetActive(false);
        engine.LeaderChooseSkillTarget(0, target);
    }

    private void OnSkill2()
    {
        var target = selector ? selector.Current : null;
        if (panel) panel.SetActive(false);
        engine.LeaderChooseSkillTarget(1, target);
    }
}
