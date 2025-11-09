[System.Serializable]
public class LevelSystem
{
    public int level { get; private set; } = 1;
    public int currXP { get; private set; } = 0;
    public int xpNextLevel => 50 + (level * level * 20);

    public event System.Action<int> OnLevelUp;

    public void AddXP(int amt)
    {
        currXP += amt;
        while (currXP >= xpNextLevel)
        {
            currXP -= xpNextLevel;
            level++;
            OnLevelUp?.Invoke(level);
        }
    }

    public void ResetLevel()
    {
        level = 1;
        currXP = 0;
    }

    public static int GetRequiredXPForLevel(int lvl)
    {
        return 50 + (lvl * lvl * 20);
    }
    public void SetState(int newLevel, int newXP)
    {
        level = newLevel;
        currXP = newXP;
    }

}
