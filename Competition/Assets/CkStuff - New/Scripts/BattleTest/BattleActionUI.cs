using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleActionUI : MonoBehaviour
{
    [SerializeField] private TurnEngine engine;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button attackBtn;
    [SerializeField] private Button skillBtn;
    [SerializeField] private Button skill2Btn;
    [SerializeField] private TargetSelector selector;

    [Header("Feedback")]
    [SerializeField] private TMP_Text infoLabel;

    private void Awake()
    {
        if (panel) panel.SetActive(false);
        if (attackBtn) attackBtn.onClick.AddListener(OnAttack);
        if (skillBtn) skillBtn.onClick.AddListener(OnSkill1);
        if (skill2Btn) skill2Btn.onClick.AddListener(OnSkill2);

        ShowMessage(null);
    }

    private void OnEnable()
    {
        if (engine) engine.OnLeaderTurnStart += ShowForLeader;
        if (selector) selector.OnSelectionChanged += HandleSelectionChanged;
    }

    private void OnDisable()
    {
        if (engine) engine.OnLeaderTurnStart -= ShowForLeader;
        if (selector) selector.OnSelectionChanged -= HandleSelectionChanged;
    }

    private void ShowForLeader(Combatant leader)
    {
        selector?.Clear();
        ShowMessage(null);
        if (panel) panel.SetActive(true);
    }

    private void HandleSelectionChanged(Combatant c)
    {
        if (c != null) ShowMessage(null);
    }

    private void OnAttack()
    {
        var target = selector ? selector.Current : null;

        if (target == null)
        {
            ShowMessage(infoLabel.text);
            return;
        }

        if (panel) panel.SetActive(false);
        engine.LeaderChooseBasicAttackTarget(target);
    }

    private void OnSkill1()
    {
        var target = selector ? selector.Current : null;

        if (target == null)
        {
            ShowMessage(infoLabel.text);
            return;
        }

        if (panel) panel.SetActive(false);
        engine.LeaderChooseSkillTarget(0, target);
    }

    private void OnSkill2()
    {
        var target = selector ? selector.Current : null;

        if (target == null)
        {
            ShowMessage(infoLabel.text);
            return;
        }

        if (panel) panel.SetActive(false);
        engine.LeaderChooseSkillTarget(1, target);
    }

    // --- helpers ---
    private void ShowMessage(string msg)
    {
        if (!infoLabel) return;

        if (string.IsNullOrEmpty(msg))
        {
            infoLabel.gameObject.SetActive(false);
        }
        else
        {
            infoLabel.text = msg;
            infoLabel.gameObject.SetActive(true);
        }
    }
}
