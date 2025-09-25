using UnityEngine;
using UnityEngine.UI;

public class BattleActionUI : MonoBehaviour
{
    [SerializeField] private TurnEngine engine;
    [SerializeField] private GameObject panel;

    [Header("Buttons")]
    [SerializeField] private Button attackBtn;
    [SerializeField] private Button skill1Btn;
    [SerializeField] private Button skill2Btn;

    private void Awake()
    {
        if (panel) panel.SetActive(false);

        if (attackBtn) attackBtn.onClick.AddListener(OnAttack);
        if (skill1Btn) skill1Btn.onClick.AddListener(OnSkill1);
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
        if (panel) panel.SetActive(true);
    }

    private void OnAttack()
    {
        if (panel) panel.SetActive(false);
        engine.LeaderChooseBasicAttack();
    }

    private void OnSkill1()
    {
        if (panel) panel.SetActive(false);
        engine.LeaderChooseSkill(0); // first skill slot
    }

    private void OnSkill2()
    {
        if (panel) panel.SetActive(false);
        engine.LeaderChooseSkill(1); // second skill slot
    }
}
