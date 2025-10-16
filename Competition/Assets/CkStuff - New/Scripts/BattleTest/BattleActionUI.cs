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
    [SerializeField] private TMP_Text skill1CdText;
    [SerializeField] private TMP_Text skill2CdText;

    private Combatant _currentLeader;

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
        _currentLeader = leader;
        selector?.Clear();
        ShowMessage(null);
        if (panel) panel.SetActive(true);

        // NEW: update buttons by cooldown
        if (_currentLeader)
        {
            bool s1Ready = _currentLeader.IsSkill1Ready;
            bool s2Ready = _currentLeader.IsSkill2Ready;

            if (skillBtn) skillBtn.interactable = s1Ready;
            if (skill2Btn) skill2Btn.interactable = s2Ready;

            if (skill1CdText)
            {
                if (s1Ready) { skill1CdText.gameObject.SetActive(false); }
                else { skill1CdText.gameObject.SetActive(true); skill1CdText.text = $"CD: {_currentLeader.Skill1Remaining}"; }
            }
            if (skill2CdText)
            {
                if (s2Ready) { skill2CdText.gameObject.SetActive(false); }
                else { skill2CdText.gameObject.SetActive(true); skill2CdText.text = $"CD: {_currentLeader.Skill2Remaining}"; }
            }

            attackBtn.GetComponent<Image>().sprite = PlayerParty.instance.GetLeader().basicAtk;
            skillBtn.GetComponent<Image>().sprite = PlayerParty.instance.GetLeader().skill1;
            skill2Btn.GetComponent<Image>().sprite = PlayerParty.instance.GetLeader().skill2;
        }

        ResetAllSpriteColors();
    }

    private void ResetAllSpriteColors()
    {
        // Find all active NewHealth components in the scene
        var allHealths = FindObjectsOfType<NewHealth>();
        foreach (var health in allHealths)
        {
            var sr = health.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white; // restore to default (you can also store & use health.originalColor if you prefer)
            }
        }
    }

    private void HandleSelectionChanged(Combatant c)
    {
        if (c != null) ShowMessage(null);
    }

    private void OnAttack()
    {
        var target = selector ? selector.Current : null;
        if (target == null) { ShowMessage(infoLabel.text); return; }
        if (panel) panel.SetActive(false);
        engine.LeaderChooseBasicAttackTarget(target);
    }

    private void OnSkill1()
    {
        var target = selector ? selector.Current : null;
        if (target == null) { ShowMessage(infoLabel.text); return; }
        if (_currentLeader && !_currentLeader.IsSkill1Ready) return;
        if (panel) panel.SetActive(false);
        engine.LeaderChooseSkillTarget(0, target);
    }

    private void OnSkill2()
    {
        var target = selector ? selector.Current : null;
        if (target == null) { ShowMessage(infoLabel.text); return; }
        if (_currentLeader && !_currentLeader.IsSkill2Ready) return;
        if (panel) panel.SetActive(false);
        engine.LeaderChooseSkillTarget(1, target);
    }

    private void ShowMessage(string msg)
    {
        if (!infoLabel) return;
        if (string.IsNullOrEmpty(msg)) { infoLabel.gameObject.SetActive(false); }
        else { infoLabel.text = msg; infoLabel.gameObject.SetActive(true); }
    }
}
