using System.Collections;
using TMPro;
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
    [SerializeField] private TurnIndicator turnIndicator;

    [Header("Feedback")]
    [SerializeField] private TMP_Text infoLabel;
    [SerializeField] private TMP_Text skill1CdText;
    [SerializeField] private TMP_Text skill2CdText;

    [Header("Selection Visuals")]
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color selectedButtonColor = new Color(1f, 0.9f, 0.3f);

    [Header("Skill Tooltip UI")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipTitle;
    [SerializeField] private TMP_Text tooltipDescription;
    [SerializeField] private float hoverDelaySeconds = 0.75f;

    private Combatant _currentUnit;

    public enum SkillSlot
    {
        Basic,
        Skill1,
        Skill2
    }

    private enum PendingAction
    {
        None,
        Basic,
        Skill1,
        Skill2
    }

    private PendingAction _pendingAction = PendingAction.None;

    private Coroutine _tooltipRoutine;
    private SkillSlot _hoverSlot;
    private bool _hoverActive = false;

    private void Awake()
    {
        if (panel) panel.SetActive(false);
        if (attackBtn) attackBtn.onClick.AddListener(OnAttackClicked);
        if (skillBtn) skillBtn.onClick.AddListener(OnSkill1Clicked);
        if (skill2Btn) skill2Btn.onClick.AddListener(OnSkill2Clicked);

        ShowMessage(null);
        ResetButtonVisuals();

        if (tooltipPanel)
            tooltipPanel.SetActive(false);
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

        HideTooltipImmediate();
    }

    private void ShowForUnit(Combatant unit)
    {
        _currentUnit = unit;
        _pendingAction = PendingAction.None;

        selector?.Disable();
        selector?.Clear();
        ShowMessage(null);
        ResetButtonVisuals();
        HideTooltipImmediate();

        if (panel) panel.SetActive(true);

        if (unit != null)
            turnIndicator.ShowArrow(unit.transform);

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
                if (attackBtn) attackBtn.GetComponent<Image>().sprite = def.basicAtk;
                if (skillBtn) skillBtn.GetComponent<Image>().sprite = def.skill1;
                if (skill2Btn) skill2Btn.GetComponent<Image>().sprite = def.skill2;
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

    private void HandleSelectionChanged(Combatant target)
    {
        if (_pendingAction == PendingAction.None)
        {
            if (target != null) ShowMessage(null);
            return;
        }

        if (target == null)
        {
            ShowMessage("Select a target.");
            return;
        }

        if (!target.IsAlive || target.isPlayerTeam)
        {
            ShowMessage("Invalid target.");
            return;
        }

        ShowMessage(null);

        StartCoroutine(ExecutePendingActionAfterDelay(target));
    }

    private IEnumerator ExecutePendingActionAfterDelay(Combatant target)
    {
        var action = _pendingAction;
        _pendingAction = PendingAction.None;

        yield return new WaitForSeconds(0.15f);

        if (panel) panel.SetActive(false);
        selector?.Disable();
        ResetButtonVisuals();
        HideTooltipImmediate();

        switch (action)
        {
            case PendingAction.Basic:
                engine.PlayerChooseBasicAttackTarget(target);
                break;

            case PendingAction.Skill1:
                engine.PlayerChooseSkillTarget(0, target);
                break;

            case PendingAction.Skill2:
                engine.PlayerChooseSkillTarget(1, target);
                break;
        }
    }

    private void OnAttackClicked()
    {
        if (_currentUnit == null || !_currentUnit.IsAlive) return;

        _pendingAction = PendingAction.Basic;
        ShowMessage("Attack selected. Choose a target.");
        UpdateButtonVisualsForSelection();

        if (selector) selector.EnableForPlayerUnit(_currentUnit);
        HideTooltipImmediate();
    }

    private void OnSkill1Clicked()
    {
        if (_currentUnit == null || !_currentUnit.IsSkill1Ready) return;

        if (_currentUnit.skill1IsCommand)
        {
            if (panel) panel.SetActive(false);
            selector?.Disable();
            selector?.Clear();
            ResetButtonVisuals();
            HideTooltipImmediate();

            engine.PlayerChooseSkillTarget(0, _currentUnit);
            return;
        }

        _pendingAction = PendingAction.Skill1;
        ShowMessage("Skill 1 selected. Choose a target.");
        UpdateButtonVisualsForSelection();

        if (selector) selector.EnableForPlayerUnit(_currentUnit);
        HideTooltipImmediate();
    }

    private void OnSkill2Clicked()
    {
        if (_currentUnit == null || !_currentUnit.IsSkill2Ready) return;

        if (_currentUnit.Skill2IsSupport)
        {
            if (panel) panel.SetActive(false);
            selector?.Disable();
            selector?.Clear();
            ResetButtonVisuals();
            HideTooltipImmediate();

            engine.PlayerChooseSkillTarget(1, _currentUnit);
            return;
        }

        _pendingAction = PendingAction.Skill2;
        ShowMessage("Skill 2 selected. Choose a target.");
        UpdateButtonVisualsForSelection();

        if (selector) selector.EnableForPlayerUnit(_currentUnit);
        HideTooltipImmediate();
    }

    private void ResetButtonVisuals()
    {
        if (attackBtn)
            attackBtn.GetComponent<Image>().color = normalButtonColor;
        if (skillBtn)
            skillBtn.GetComponent<Image>().color = normalButtonColor;
        if (skill2Btn)
            skill2Btn.GetComponent<Image>().color = normalButtonColor;
    }

    private void UpdateButtonVisualsForSelection()
    {
        ResetButtonVisuals();

        switch (_pendingAction)
        {
            case PendingAction.Basic:
                if (attackBtn)
                    attackBtn.GetComponent<Image>().color = selectedButtonColor;
                break;

            case PendingAction.Skill1:
                if (skillBtn)
                    skillBtn.GetComponent<Image>().color = selectedButtonColor;
                break;

            case PendingAction.Skill2:
                if (skill2Btn)
                    skill2Btn.GetComponent<Image>().color = selectedButtonColor;
                break;
        }
    }

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

    public void BeginHover(SkillSlot slot)
    {
        if (_currentUnit == null) return;

        _hoverSlot = slot;
        _hoverActive = true;

        if (_tooltipRoutine != null)
            StopCoroutine(_tooltipRoutine);

        _tooltipRoutine = StartCoroutine(HoverDelayRoutine());
    }

    public void EndHover(SkillSlot slot)
    {
        if (!_hoverActive || slot != _hoverSlot)
            return;

        HideTooltipImmediate();
    }

    private IEnumerator HoverDelayRoutine()
    {
        float elapsed = 0f;
        while (elapsed < hoverDelaySeconds && _hoverActive)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _tooltipRoutine = null;

        if (_hoverActive)
            ShowTooltipForSlot(_hoverSlot);
    }

    private void ShowTooltipForSlot(SkillSlot slot)
    {
        if (tooltipPanel == null || tooltipTitle == null || tooltipDescription == null)
            return;

        if (_currentUnit == null) return;

        NewCharacterDefinition def = PlayerParty.instance.GetDefinitionFor(_currentUnit);
        if (def == null) return;

        string title = "";
        string desc = "";

        switch (slot)
        {
            case SkillSlot.Basic:
                title = def.basicSkillName;
                desc = def.basicDescription;
                break;

            case SkillSlot.Skill1:
                title = def.skill1Name;
                desc = def.skill1Description;
                break;

            case SkillSlot.Skill2:
                title = def.skill2Name;
                desc = def.skill2Description;
                break;
        }

        tooltipTitle.text = title;
        tooltipDescription.text = desc;
        tooltipPanel.SetActive(true);
    }

    private void HideTooltipImmediate()
    {
        _hoverActive = false;

        if (_tooltipRoutine != null)
        {
            StopCoroutine(_tooltipRoutine);
            _tooltipRoutine = null;
        }

        if (tooltipPanel)
            tooltipPanel.SetActive(false);
    }
}
