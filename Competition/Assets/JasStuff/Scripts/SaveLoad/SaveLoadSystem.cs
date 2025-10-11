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
        Debug.Log("IM LOADING");

        if (!isNewGame)
            LoadGame();
        else
            Debug.Log("[SaveLoadSystem] Skipped LoadGame() because this is a New Game.");
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

        gameData = new GameData();

        if (EnemyTracker.instance) EnemyTracker.instance.ResetEnemies();
        gameData.playerPosition = Vector2.zero;
        gameData.hasSavedPosition = false;


        if (keepCharIndex)
            gameData.selectedCharacterIndex = savedIndex;
    }

    public void LoadGame()
    {
        // load saved data from file using data handler
        gameData = fileDataHandler.Load();
        // if no data, initialize new game
        if (gameData == null) NewGame();

        dataPersistenceObjs = FindAllDataPersistenceObjects();

        // push loaded data to all other scripts that need it
        foreach (IDataPersistence dataObjs in dataPersistenceObjs)
        {
            dataObjs.LoadData(gameData);
        }
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

        // save data to a file using data handler
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
