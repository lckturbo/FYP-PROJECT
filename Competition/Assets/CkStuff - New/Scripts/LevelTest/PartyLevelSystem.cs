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
}
