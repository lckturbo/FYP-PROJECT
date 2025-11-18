using UnityEngine;

public class PlayerBuffHandler : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private GameObject attackBuffVFX;
    [SerializeField] private GameObject defenseBuffVFX;
    [SerializeField] private GameObject Skill2BuffEffect;

    private bool isSkill2BuffActive = false;
    public bool IsSkill2BuffActive => isSkill2BuffActive;


    public PlayerLevelApplier levelApplier { get; private set; }
    private NewCharacterStats runtimeStats => levelApplier != null ? levelApplier.runtimeStats : null;

    private int currentAttackBuff = 0;
    private int currentDefenseBuff = 0;

    private float attackBuffEndTime = 0f;
    private float defenseBuffEndTime = 0f;

    public bool IsBuffActive => currentAttackBuff > 0 || currentDefenseBuff > 0;

    private void Awake()
    {
        levelApplier = GetComponent<PlayerLevelApplier>();
        if (levelApplier == null)
            Debug.LogWarning($"{name}: No PlayerLevelApplier found for runtime stats.");
    }

    private void Start() => UpdateBuffVFX();

    private void OnEnable()
    {
        BattleManager.OnClearAllBuffs += RemoveStoredBuffs;
    }

    private void OnDisable()
    {
        BattleManager.OnClearAllBuffs -= RemoveStoredBuffs;
    }

    public void ApplyAttackBuff(int amount, float duration)
    {
        if (runtimeStats == null) return;

        if (currentAttackBuff != 0)
            RemoveAttackBuff();

        currentAttackBuff = amount;
        runtimeStats.atkDmg += amount;
        attackBuffEndTime = duration > 0 ? Time.time + duration : 0f;

        BuffData.instance?.StoreAttackBuff(amount, runtimeStats);
        UpdateBuffVFX();
        Debug.Log($"Applied +{amount} ATK for {duration}s.");
    }

    public void ApplyDefenseBuff(int amount, float duration)
    {
        if (runtimeStats == null) return;

        if (currentDefenseBuff != 0)
            RemoveDefenseBuff();

        currentDefenseBuff = amount;
        runtimeStats.attackreduction += amount;
        defenseBuffEndTime = duration > 0 ? Time.time + duration : 0f;

        BuffData.instance?.StoreDefenseBuff(amount, runtimeStats);
        UpdateBuffVFX();
        Debug.Log($"Applied +{amount} DEF for {duration}s.");
    }

    private void Update()
    {
        if (currentAttackBuff != 0 && attackBuffEndTime > 0 && Time.time >= attackBuffEndTime)
            RemoveAttackBuff();

        if (currentDefenseBuff != 0 && defenseBuffEndTime > 0 && Time.time >= defenseBuffEndTime)
            RemoveDefenseBuff();
    }

    public void RemoveAttackBuff()
    {
        if (runtimeStats != null)
            runtimeStats.atkDmg -= currentAttackBuff;

        // remove only ONE buff belonging to this ally
        BuffData.instance?.RemoveBuff(runtimeStats, BuffData.BuffEntry.BuffType.Attack);

        currentAttackBuff = 0;
        UpdateBuffVFX();
    }


    private void RemoveDefenseBuff()
    {
        if (runtimeStats != null)
            runtimeStats.attackreduction -= currentDefenseBuff;

        currentDefenseBuff = 0;
        BuffData.instance?.RemoveBuff(runtimeStats, BuffData.BuffEntry.BuffType.Defense);

        UpdateBuffVFX();
        Debug.Log("Defense buff expired.");
    }

    public void RemoveStoredBuffs()
    {
        if (runtimeStats == null || BuffData.instance == null) return;

        foreach (var buff in BuffData.instance.allBuffs)
        {
            if (buff.target == runtimeStats)
            {
                if (buff.type == BuffData.BuffEntry.BuffType.Attack)
                    runtimeStats.atkDmg -= buff.amount;

                if (buff.type == BuffData.BuffEntry.BuffType.Defense)
                    runtimeStats.attackreduction -= buff.amount;
            }
        }

        BuffData.instance.ClearAllBuffs();

        currentAttackBuff = 0;
        currentDefenseBuff = 0;

        UpdateBuffVFX();
    }

    public void ApplySkill2Buff(float duration = 0f)
    {
        isSkill2BuffActive = true;

        // No runtime stat changes here — Combatant already did it
        UpdateBuffVFX();
    }

    public void RemoveSkill2Buff()
    {
        isSkill2BuffActive = false;
        UpdateBuffVFX();
    }


    public void UpdateBuffVFX()
    {
        if (attackBuffVFX != null)
            attackBuffVFX.SetActive(currentAttackBuff > 0);

        if (defenseBuffVFX != null)
            defenseBuffVFX.SetActive(currentDefenseBuff > 0);

        if (Skill2BuffEffect != null)
            Skill2BuffEffect.SetActive(isSkill2BuffActive);
    }

}
