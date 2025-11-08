using UnityEngine;

public class PartyLevelSystem : MonoBehaviour
{
    public static PartyLevelSystem Instance { get; private set; }

    // Reuse your existing LevelSystem type
    public LevelSystem levelSystem = new LevelSystem();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    // Call this after battles / quests / kills
    public void AddXP(int amount)
    {
        levelSystem.AddXP(amount);
        Debug.Log($"[PartyXP] +{amount} XP (Lv {levelSystem.level}, {levelSystem.currXP}/{levelSystem.xpNextLevel})");
    }

    public void ResetLevel()
    {
        levelSystem.ResetLevel();
    }

    // NEW: Save level data
    public void SaveData(ref GameData data)
    {
        data.playerLevel = levelSystem.level;
        data.playerCurrentXP = levelSystem.currXP;
    }

    // NEW: Load level data
    public void LoadData(GameData data)
    {
        // Use reflection to set private properties
        var levelField = typeof(LevelSystem).GetProperty("level");
        var xpField = typeof(LevelSystem).GetProperty("currXP");

        if (levelField != null)
            levelField.SetValue(levelSystem, data.playerLevel);
        if (xpField != null)
            xpField.SetValue(levelSystem, data.playerCurrentXP);
    }
}
