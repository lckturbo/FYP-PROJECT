// StatsRuntimeBuilder.cs
using UnityEngine;

public static class StatsRuntimeBuilder
{
    // === Existing: for player-side NewCharacterStats ===
    public static NewCharacterStats BuildRuntimeStats(NewCharacterStats baseStats, int targetLevel, LevelGrowth growth)
    {
        if (baseStats == null) return null;

        var rt = ScriptableObject.CreateInstance<NewCharacterStats>();

        // --- copy base ---
        rt.Speed = baseStats.Speed;
        rt.maxHealth = baseStats.maxHealth;
        rt.attackreduction = baseStats.attackreduction;
        rt.atkDmg = baseStats.atkDmg;
        rt.actionvaluespeed = baseStats.actionvaluespeed;
        rt.critRate = baseStats.critRate;
        rt.critDamage = baseStats.critDamage;

        rt.attackElement = baseStats.attackElement;
        rt.defenseElement = baseStats.defenseElement;
        rt.fireRes = baseStats.fireRes;
        rt.waterRes = baseStats.waterRes;
        rt.grassRes = baseStats.grassRes;
        rt.darkRes = baseStats.darkRes;
        rt.lightRes = baseStats.lightRes;

        // --- growth from level 1 -> targetLevel ---
        int extraLevels = Mathf.Max(0, targetLevel - 1);
        if (growth && extraLevels > 0)
        {
            // additive
            rt.maxHealth += growth.maxHealthAdd * extraLevels;
            rt.atkDmg += growth.atkDmgAdd * extraLevels;
            rt.attackreduction += growth.defenseAdd * extraLevels;
            rt.actionvaluespeed += growth.speedAdd * extraLevels;
            rt.critRate += growth.critRateAdd * extraLevels;
            rt.critDamage += growth.critDmgAdd * extraLevels;

            // multiplicative (cumulative)
            rt.maxHealth = Mathf.RoundToInt(rt.maxHealth * Mathf.Pow(growth.maxHealthMul, extraLevels));
            rt.atkDmg = Mathf.RoundToInt(rt.atkDmg * Mathf.Pow(growth.atkDmgMul, extraLevels));
            rt.actionvaluespeed = rt.actionvaluespeed * Mathf.Pow(growth.speedMul, extraLevels);
        }

        // keep any extra fields
        rt.level = targetLevel;
        rt.atkCD = baseStats.atkCD;
        rt.atkRange = baseStats.atkRange;

        return rt;
    }

    // === New: generic overload for any BaseStats (e.g., EnemyStats) ===
    public static BaseStats BuildRuntimeStats(BaseStats baseStats, int targetLevel, LevelGrowth growth)
    {
        if (baseStats == null) return null;

        // If it's already NewCharacterStats, reuse the player method
        if (baseStats is NewCharacterStats ncs)
            return BuildRuntimeStats(ncs, targetLevel, growth);

        // Otherwise, clone into a NewCharacterStats runtime (inherits BaseStats)
        var rt = ScriptableObject.CreateInstance<NewCharacterStats>();

        // --- copy base (BaseStats fields) ---
        rt.Speed = baseStats.Speed;
        rt.maxHealth = baseStats.maxHealth;
        rt.attackreduction = baseStats.attackreduction;
        rt.atkDmg = baseStats.atkDmg;
        rt.actionvaluespeed = baseStats.actionvaluespeed;
        rt.critRate = baseStats.critRate;
        rt.critDamage = baseStats.critDamage;

        rt.attackElement = baseStats.attackElement;
        rt.defenseElement = baseStats.defenseElement;
        rt.fireRes = baseStats.fireRes;
        rt.waterRes = baseStats.waterRes;
        rt.grassRes = baseStats.grassRes;
        rt.darkRes = baseStats.darkRes;
        rt.lightRes = baseStats.lightRes;

        // --- growth from level 1 -> targetLevel ---
        int extraLevels = Mathf.Max(0, targetLevel - 1);
        if (growth && extraLevels > 0)
        {
            // additive
            rt.maxHealth += growth.maxHealthAdd * extraLevels;
            rt.atkDmg += growth.atkDmgAdd * extraLevels;
            rt.attackreduction += growth.defenseAdd * extraLevels;
            rt.actionvaluespeed += growth.speedAdd * extraLevels;
            rt.critRate += growth.critRateAdd * extraLevels;
            rt.critDamage += growth.critDmgAdd * extraLevels;

            // multiplicative (cumulative)
            rt.maxHealth = Mathf.RoundToInt(rt.maxHealth * Mathf.Pow(growth.maxHealthMul, extraLevels));
            rt.atkDmg = Mathf.RoundToInt(rt.atkDmg * Mathf.Pow(growth.atkDmgMul, extraLevels));
            rt.actionvaluespeed = rt.actionvaluespeed * Mathf.Pow(growth.speedMul, extraLevels);
        }

        // tag level (safe defaults for enemy-only fields)
        rt.level = targetLevel;
        rt.atkCD = 0f;
        rt.atkRange = 0f;

        // Return as BaseStats (Combatant.stats and NewHealth.ApplyStats both accept BaseStats)
        return rt;
    }
}
