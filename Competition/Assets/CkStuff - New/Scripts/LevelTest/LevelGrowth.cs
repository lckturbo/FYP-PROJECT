using UnityEngine;

[CreateAssetMenu(fileName = "LevelGrowth", menuName = "Stats/Level Growth")]
public class LevelGrowth : ScriptableObject
{
    [Header("Additive per level (applied each level-up)")]
    public int maxHealthAdd;
    public int atkDmgAdd;
    public float defenseAdd;
    public float speedAdd;
    public float critRateAdd;
    public float critDmgAdd;

    [Header("Curve Control (higher = stronger curve)")]
    [Range(0f, 5f)] public float maxHealthCurve;
    [Range(0f, 5f)] public float atkDmgCurve;
    [Range(0f, 5f)] public float defenseCurve;
    [Range(0f, 5f)] public float speedCurve;
    [Range(0f, 5f)] public float critRateCurve;
    [Range(0f, 5f)] public float critDmgCurve;

    [Header("Multipliers per level (applied cumulatively)")]
    public float maxHealthMul = 1f;
    public float atkDmgMul = 1f;
    public float speedMul = 1f;

    // === NEW: curved additive scaling ===
    public float GetAddValue(float baseAdd, float curve, int level)
    {
        if (level <= 0) return 0f;
        // curved exponential growth — example: baseAdd * level * (1 + curve * (level - 1) * 0.05f)
        return baseAdd * level * (1f + curve * (level - 1) * 0.05f);
    }
}
