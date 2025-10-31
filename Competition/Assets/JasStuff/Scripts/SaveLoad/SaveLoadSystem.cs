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

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
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

        fileDataHandler.DeleteSaveFile();
        Debug.Log("[SaveLoadSystem] Old save file deleted.");
        gameData = new GameData();
        fileDataHandler.Save(gameData);

        if (keepCharIndex && gameData != null)
            savedIndex = gameData.selectedCharacterIndex;

        if (EnemyTracker.instance) EnemyTracker.instance.ResetEnemies();
        gameData.playerPosition = Vector2.zero;
        gameData.hasSavedPosition = false;
        gameData.hasCheckpoint = false;
        gameData.lastCheckpointID = 0;

        if (keepCharIndex)
            gameData.selectedCharacterIndex = savedIndex;
    }

    public void LoadGame()
    {
        gameData = fileDataHandler.Load();
        if (gameData == null) NewGame();

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

        fileDataHandler.Save(gameData);
    }

    public GameData GetGameData()
    {
        return gameData;
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjs = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjs);
    }
}
