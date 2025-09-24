using UnityEngine;

public class EnemyScaler : MonoBehaviour
{
    [SerializeField] private PlayerLevel playerLevel;

    private void Start()
    {
        if (!playerLevel)
        {
            playerLevel = GameObject.FindWithTag("Player").GetComponent<PlayerLevel>();
            playerLevel.levelSystem.OnLevelUp += ScaleToPlayer;
            ScaleToPlayer(playerLevel.levelSystem.level);
        }

    }

    private void ScaleToPlayer(int playerLevelValue)
    {
    }

    private void OnDestroy()
    {
        if (!playerLevel)
            playerLevel.levelSystem.OnLevelUp -= ScaleToPlayer;
    }
}
