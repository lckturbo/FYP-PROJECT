using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveLoadSystem : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    public static SaveLoadSystem instance;
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjs;
    private FileDataHandler fileDataHandler;

    private bool isNewGame = false;
    public bool IsNewGame => isNewGame;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);

       // DontDestroyOnLoad(gameObject);
        fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dataPersistenceObjs = FindAllDataPersistenceObjects();

        if (!isNewGame) LoadGame();
        else isNewGame = false;
    }

    public void RegisterDataPersistenceObjects(IDataPersistence obj)
    {
        if (!dataPersistenceObjs.Contains(obj))
            dataPersistenceObjs.Add(obj);
    }

    public void NewGame(bool keepCharIndex = false)
    {
        isNewGame = true;

        int savedIndex = -1;

        if (keepCharIndex && gameData != null)
            savedIndex = gameData.selectedCharacterIndex;

        // Delete the save file
        fileDataHandler.DeleteSaveFile();
        Debug.Log("[SaveLoadSystem] Old save file deleted.");

        if (keepCharIndex)
        {
            gameData = new GameData();

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.ResetAllQuests();
            }

            if (PartyLevelSystem.Instance != null)
            {
                PartyLevelSystem.Instance.ResetLevel();
            }

            if (EnemyTracker.instance)
                EnemyTracker.instance.ResetEnemies();

            gameData.playerPosition = Vector2.zero;
            gameData.hasSavedPosition = false;
            gameData.hasCheckpoint = false;
            gameData.lastCheckpointID = 0;
            gameData.selectedCharacterIndex = savedIndex;
        }
        else
        {
            gameData = null;

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.ResetAllQuests();
            }

            if (PartyLevelSystem.Instance != null)
            {
                PartyLevelSystem.Instance.ResetLevel();
            }

            if (EnemyTracker.instance)
                EnemyTracker.instance.ResetEnemies();
        }
    }

    public void LoadGame()
    {
        gameData = fileDataHandler.Load();

        if (gameData == null || !gameData.hasSavedGame)
        {
            gameData = null;
            return;
        }
        dataPersistenceObjs = FindAllDataPersistenceObjects();

        foreach (IDataPersistence dataObjs in dataPersistenceObjs)
            dataObjs.LoadData(gameData);
    }

    public void SaveGame(bool savePlayer = true, bool saveEnemies = true)
    {
        if (gameData == null)
            gameData = new GameData();

        dataPersistenceObjs = FindAllDataPersistenceObjects();

        foreach (IDataPersistence dataObj in dataPersistenceObjs)
        {
            if (!savePlayer && dataObj.GetType().Name.Contains("Player")) continue;
            if (!saveEnemies && dataObj is EnemyTracker) continue;

            dataObj.SaveData(ref gameData);
        }

        gameData.hasSavedGame = true;
        fileDataHandler.Save(gameData);
    }

    public void SaveSettingsOnly()
    {
        if (gameData == null)
            gameData = new GameData();

        dataPersistenceObjs = FindAllDataPersistenceObjects();
        foreach (IDataPersistence dataObj in dataPersistenceObjs)
        {
            if (dataObj is SettingsManager)
                dataObj.SaveData(ref gameData);
        }

        fileDataHandler.Save(gameData);
    }


    public GameData GetGameData()
    {
        return gameData;
    }

    public bool HasSaveFile()
    {
        GameData data = fileDataHandler.Load();
        bool hasValidSave = data != null && data.hasSavedGame;

        //dis a helpers tool for checking for savefiles jas

        Debug.Log($"[SaveLoadSystem] HasSaveFile check:");
        Debug.Log($"  - Data is null: {data == null}");
        Debug.Log($"  - hasSavedGame: {(data != null ? data.hasSavedGame.ToString() : "N/A")}");
        Debug.Log($"  - Result: {hasValidSave}");

        return hasValidSave;
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjs = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjs);
    }
}