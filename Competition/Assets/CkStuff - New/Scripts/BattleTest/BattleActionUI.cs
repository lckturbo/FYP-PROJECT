using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    [Header("Feedback")]
    [SerializeField] private TMP_Text infoLabel;
    [SerializeField] private TMP_Text skill1CdText;
    [SerializeField] private TMP_Text skill2CdText;

    private Combatant _currentUnit;

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
        if (engine) engine.OnPlayerTurnStart += ShowForUnit;
        if (selector) selector.OnSelectionChanged += HandleSelectionChanged;
    }

    private void OnDisable()
    {
        if (engine) engine.OnPlayerTurnStart -= ShowForUnit;
        if (selector) selector.OnSelectionChanged -= HandleSelectionChanged;
    }

    private void ShowForUnit(Combatant unit)
    {
        _currentUnit = unit;
        selector?.Clear();
        ShowMessage(null);
        if (panel) panel.SetActive(true);

        if (_currentUnit)
        {
            bool s1Ready = _currentUnit.IsSkill1Ready;
            bool s2Ready = _currentUnit.IsSkill2Ready;

            if (skillBtn) skillBtn.interactable = s1Ready;
            if (skill2Btn) skill2Btn.interactable = s2Ready;

            if (skill1CdText)
            {
                skill1CdText.gameObject.SetActive(!s1Ready);
                if (!s1Ready) skill1CdText.text = $"CD: {_currentUnit.Skill1Remaining}";
            }
            if (skill2CdText)
            {
                skill2CdText.gameObject.SetActive(!s2Ready);
                if (!s2Ready) skill2CdText.text = $"CD: {_currentUnit.Skill2Remaining}";
            }

            NewCharacterDefinition def = PlayerParty.instance.GetDefinitionFor(_currentUnit);
            if (def != null)
            {
                attackBtn.GetComponent<Image>().sprite = def.basicAtk;
                skillBtn.GetComponent<Image>().sprite = def.skill1;
                skill2Btn.GetComponent<Image>().sprite = def.skill2;
            }
        }

        ResetAllSpriteColors();
    }

    private void ResetAllSpriteColors()
    {
        foreach (var health in FindObjectsOfType<NewHealth>())
        {
            var sr = health.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.white;
        }
    }

    private void HandleSelectionChanged(Combatant c)
    {
        if (c != null) ShowMessage(null);
    }

    private void OnAttack()
    {
        var target = selector ? selector.Current : null;
        if (target == null) { ShowMessage("Select a target first."); return; }
        if (panel) panel.SetActive(false);
        engine.PlayerChooseBasicAttackTarget(target);
    }

    private void OnSkill1()
    {
        var target = selector ? selector.Current : null;
        if (target == null) { ShowMessage("Select a target first."); return; }
        if (_currentUnit && !_currentUnit.IsSkill1Ready) return;
        if (panel) panel.SetActive(false);
        engine.PlayerChooseSkillTarget(0, target);
    }

    private void OnSkill2()
    {
        if (_currentUnit == null) return;
        if (!_currentUnit.IsSkill2Ready) return;

        // Self-cast support Skill2
        if (_currentUnit.Skill2IsSupport)
        {
            if (panel) panel.SetActive(false);
            engine.PlayerChooseSkillTarget(1, _currentUnit);
            selector?.Clear();
            return;
        }

        var target = selector ? selector.Current : null;
        if (target == null)
        {
            ShowMessage("Select a target first.");
            return;
        }

        if (panel) panel.SetActive(false);
        engine.PlayerChooseSkillTarget(1, target);
    }

    private void ShowMessage(string msg)
    {
        if (!infoLabel) return;
        if (string.IsNullOrEmpty(msg))
            infoLabel.gameObject.SetActive(false);
        else
        {
            infoLabel.text = msg;
            infoLabel.gameObject.SetActive(true);
        }
    }
}
