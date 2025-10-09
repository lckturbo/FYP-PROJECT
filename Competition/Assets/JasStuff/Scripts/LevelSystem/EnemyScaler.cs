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

        ////  Log all base stats for debugging
        //Debug.Log(
        //    $"[EnemyScaler] {name} Base Stats before scaling:\n" +
        //    $"  Speed: {baseAsChar.Speed}\n" +
        //    $"  Max HP: {baseAsChar.maxHealth}\n" +
        //    $"  Attack Damage: {baseAsChar.atkDmg}\n" +
        //    $"  Attack Reduction: {baseAsChar.attackreduction}\n" +
        //    $"  Action Value Speed: {baseAsChar.actionvaluespeed}\n" +
        //    $"  Crit Rate: {baseAsChar.critRate}\n" +
        //    $"  Crit Damage: {baseAsChar.critDamage}\n" +
        //    $"  Attack Element: {baseAsChar.attackElement}\n" +
        //    $"  Defense Element: {baseAsChar.defenseElement}\n" +
        //    $"  Fire Res: {baseAsChar.fireRes}\n" +
        //    $"  Water Res: {baseAsChar.waterRes}\n" +
        //    $"  Grass Res: {baseAsChar.grassRes}\n" +
        //    $"  Dark Res: {baseAsChar.darkRes}\n" +
        //    $"  Light Res: {baseAsChar.lightRes}"
        //);

        ////  Log all base stats for debugging
        //Debug.Log(
        //    $"[EnemyScaler] {name} Base Stats before scaling:\n" +
        //    $"  Speed: {es.Speed}\n" +
        //    $"  Max HP: {es.maxHealth}\n" +
        //    $"  Attack Damage: {es.atkDmg}\n" +
        //    $"  Attack Reduction: {es.attackreduction}\n" +
        //    $"  Action Value Speed: {es.actionvaluespeed}\n" +
        //    $"  Crit Rate: {es.critRate}\n" +
        //    $"  Crit Damage: {es.critDamage}\n" +
        //    $"  Attack Element: {es.attackElement}\n" +
        //    $"  Defense Element: {es.defenseElement}\n" +
        //    $"  Fire Res: {es.fireRes}\n" +
        //    $"  Water Res: {es.waterRes}\n" +
        //    $"  Grass Res: {es.grassRes}\n" +
        //    $"  Dark Res: {es.darkRes}\n" +
        //    $"  Light Res: {es.lightRes}"
        //);

        var rt = StatsRuntimeBuilder.BuildRuntimeStats(baseAsChar, partyLevel, enemyGrowth);
        if (health) health.ApplyStats(rt);
        var c = GetComponent<Combatant>();
        if (c) c.stats = rt;

        Debug.Log($"{name}: Scaled to party level {partyLevel} ? Final (HP {rt.maxHealth}, ATK {rt.atkDmg})");
    }

}
