using UnityEngine;

public class PartyLevelSystem : MonoBehaviour, IDataPersistence
{
    public static PartyLevelSystem Instance { get; private set; }

    // Reuse your existing LevelSystem type
    public LevelSystem levelSystem = new LevelSystem();

    private void Awake()
    {
        if (transform.parent != null)
            transform.SetParent(null);
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        //DontDestroyOnLoad(gameObject);
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

    public void SaveData(ref GameData data)
    {
        data.playerLevel = levelSystem.level;
        data.playerCurrentXP = levelSystem.currXP;
    }

    public void LoadData(GameData data)
    {
        levelSystem.SetState(data.playerLevel, data.playerCurrentXP);
    }

}
