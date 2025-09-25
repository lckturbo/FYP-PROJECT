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
        while(currXP >= xpNextLevel)
        {
            currXP -= xpNextLevel;
            level++;
            OnLevelUp?.Invoke(level);

        }
    }

    // for restarts/debugging/respawn
    public void ResetLevel()
    {
        level = 1;
        currXP = 0;
    }
}
