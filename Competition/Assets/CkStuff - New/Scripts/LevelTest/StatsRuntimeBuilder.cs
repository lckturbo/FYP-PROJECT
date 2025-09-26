// StatsRuntimeBuilder.cs
using UnityEngine;

public static class StatsRuntimeBuilder
{
    public static NewCharacterStats BuildRuntimeStats(NewCharacterStats baseStats, int targetLevel, LevelGrowth growth)
    {
        if (baseStats == null) return null;

        // Create a runtime instance (not saved to disk)
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

        // --- apply growth from level 1 -> targetLevel ---
        // Level 1 = base stats. For each level above 1, apply additive/multipliers.
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

            // multiplicative (applied cumulatively)
            rt.maxHealth = Mathf.RoundToInt(rt.maxHealth * Mathf.Pow(growth.maxHealthMul, extraLevels));
            rt.atkDmg = Mathf.RoundToInt(rt.atkDmg * Mathf.Pow(growth.atkDmgMul, extraLevels));
            rt.actionvaluespeed = rt.actionvaluespeed * Mathf.Pow(growth.speedMul, extraLevels);
        }

        // keep any extra fields you might add in NewCharacterStats
        rt.level = targetLevel;
        rt.atkCD = baseStats.atkCD;
        rt.atkRange = baseStats.atkRange;

        return rt;
    }
}
