using UnityEngine;

public static class StatsRuntimeBuilder
{
    // === Player-side NewCharacterStats ===
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
            // ====== Additive with curve support ======
            rt.maxHealth += Mathf.RoundToInt(growth.GetAddValue(growth.maxHealthAdd, growth.maxHealthCurve, extraLevels));
            rt.atkDmg += Mathf.RoundToInt(growth.GetAddValue(growth.atkDmgAdd, growth.atkDmgCurve, extraLevels));
            rt.attackreduction += growth.GetAddValue(growth.defenseAdd, growth.defenseCurve, extraLevels);
            rt.actionvaluespeed += growth.GetAddValue(growth.speedAdd, growth.speedCurve, extraLevels);
            rt.critRate += growth.GetAddValue(growth.critRateAdd, growth.critRateCurve, extraLevels);
            rt.critDamage += growth.GetAddValue(growth.critDmgAdd, growth.critDmgCurve, extraLevels);

            // ====== Multiplicative (cumulative) ======
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

    // === Enemy-side / generic BaseStats ===
    public static BaseStats BuildRuntimeStats(BaseStats baseStats, int targetLevel, LevelGrowth growth)
    {
        if (baseStats == null) return null;

        // If it's already NewCharacterStats, reuse player logic
        if (baseStats is NewCharacterStats ncs)
            return BuildRuntimeStats(ncs, targetLevel, growth);

        // Otherwise, clone into a NewCharacterStats runtime
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
            // ====== Additive with curve support ======
            rt.maxHealth += Mathf.RoundToInt(growth.GetAddValue(growth.maxHealthAdd, growth.maxHealthCurve, extraLevels));
            rt.atkDmg += Mathf.RoundToInt(growth.GetAddValue(growth.atkDmgAdd, growth.atkDmgCurve, extraLevels));
            rt.attackreduction += growth.GetAddValue(growth.defenseAdd, growth.defenseCurve, extraLevels);
            rt.actionvaluespeed += growth.GetAddValue(growth.speedAdd, growth.speedCurve, extraLevels);
            rt.critRate += growth.GetAddValue(growth.critRateAdd, growth.critRateCurve, extraLevels);
            rt.critDamage += growth.GetAddValue(growth.critDmgAdd, growth.critDmgCurve, extraLevels);

            // ====== Multiplicative (cumulative) ======
            rt.maxHealth = Mathf.RoundToInt(rt.maxHealth * Mathf.Pow(growth.maxHealthMul, extraLevels));
            rt.atkDmg = Mathf.RoundToInt(rt.atkDmg * Mathf.Pow(growth.atkDmgMul, extraLevels));
            rt.actionvaluespeed = rt.actionvaluespeed * Mathf.Pow(growth.speedMul, extraLevels);
        }

        // Tag level (safe defaults for enemies)
        rt.level = targetLevel;
        rt.atkCD = 0f;
        rt.atkRange = 0f;

        return rt;
    }
}
