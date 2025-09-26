using UnityEngine;

public class EnemyScaler : MonoBehaviour
{
    [SerializeField] private LevelGrowth enemyGrowth;
    private NewHealth health;
    private EnemyBase enemyBase;

    private void Awake()
    {
        health = GetComponentInChildren<NewHealth>();
        enemyBase = GetComponent<EnemyBase>();
    }

    private void OnEnable()
    {
        if (PartyLevelSystem.Instance != null)
            PartyLevelSystem.Instance.levelSystem.OnLevelUp += ScaleToPartyLevel;

        // initial scale
        if (PartyLevelSystem.Instance != null)
            ScaleToPartyLevel(PartyLevelSystem.Instance.levelSystem.level);
        else
            ScaleToPartyLevel(1);
    }

    private void OnDisable()
    {
        if (PartyLevelSystem.Instance != null)
            PartyLevelSystem.Instance.levelSystem.OnLevelUp -= ScaleToPartyLevel;
    }

    private void ScaleToPartyLevel(int partyLevel)
    {
        if (!enemyBase) return;
        var es = enemyBase.GetEnemyStats();
        if (!es) return;

        var baseAsChar = ScriptableObject.CreateInstance<NewCharacterStats>();
        baseAsChar.Speed = es.Speed;
        baseAsChar.maxHealth = es.maxHealth;
        baseAsChar.attackreduction = es.attackreduction;
        baseAsChar.atkDmg = es.atkDmg;
        baseAsChar.actionvaluespeed = es.actionvaluespeed;
        baseAsChar.critRate = es.critRate;
        baseAsChar.critDamage = es.critDamage;
        baseAsChar.attackElement = es.attackElement;
        baseAsChar.defenseElement = es.defenseElement;
        baseAsChar.fireRes = es.fireRes;
        baseAsChar.waterRes = es.waterRes;
        baseAsChar.grassRes = es.grassRes;
        baseAsChar.darkRes = es.darkRes;
        baseAsChar.lightRes = es.lightRes;

        var rt = StatsRuntimeBuilder.BuildRuntimeStats(baseAsChar, partyLevel, enemyGrowth);
        if (health) health.ApplyStats(rt);
        var c = GetComponent<Combatant>();
        if (c) c.stats = rt;

        Debug.Log($"{name}: scaled to party level {partyLevel} (HP {rt.maxHealth}, ATK {rt.atkDmg})");
    }
}
