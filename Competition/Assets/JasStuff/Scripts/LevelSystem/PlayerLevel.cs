using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public LevelSystem levelSystem = new LevelSystem();
    private void Start()
    {
        levelSystem.OnLevelUp += HandleLevelUp;
    }

    private void HandleLevelUp(int newLevel)
    {
        Debug.Log("Player's level: " + newLevel);
    }
}
