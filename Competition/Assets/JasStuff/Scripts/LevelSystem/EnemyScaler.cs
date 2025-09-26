using UnityEngine;

public class EnemyScaler : MonoBehaviour
{
    [SerializeField] private PlayerLevel playerLevel;

    private void OnEnable()
    {
        PlayerSpawner.OnPlayerSpawned += HandlePlayerSpawned;
    }

    private void OnDisable()
    {
        PlayerSpawner.OnPlayerSpawned -= HandlePlayerSpawned;
    }

    private void HandlePlayerSpawned(Transform playerTransform)
    {
        playerLevel = playerTransform.gameObject.GetComponent<PlayerLevel>();
        playerLevel.levelSystem.OnLevelUp += ScaleToPlayer;
        ScaleToPlayer(playerLevel.levelSystem.level);
    }

    //private void Start()
    //{
    //    if (!playerLevel)
    //    {
    //        playerLevel.levelSystem.OnLevelUp += ScaleToPlayer;
    //        ScaleToPlayer(playerLevel.levelSystem.level);
    //    }

    //}
    private void OnDestroy()
    {
        if (!playerLevel)
            playerLevel.levelSystem.OnLevelUp -= ScaleToPlayer;
    }

    private void ScaleToPlayer(int playerLevelValue)
    {
    }
}
