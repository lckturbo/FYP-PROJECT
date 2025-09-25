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

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);

        fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);

        //gameData = fileDataHandler.Load();
        //if (gameData == null) NewGame();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        dataPersistenceObjs = FindAllDataPersistenceObjects();
        LoadGame();
    }
    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "jasBattle") return;

        SaveGame();
    }

    public void RegisterDataPersistenceObjects(IDataPersistence obj)
    {
        if (!dataPersistenceObjs.Contains(obj))
            dataPersistenceObjs.Add(obj);
    }
    public void NewGame()
    {
        gameData = new GameData();
    }
    public void LoadGame()
    {
        // load saved data from file using data handler
        gameData = fileDataHandler.Load();
        // if no data, initialize new game
        if (gameData == null) NewGame();

        // push loaded data to all other scripts that need it
        foreach(IDataPersistence dataObjs in dataPersistenceObjs)
        {
            dataObjs.LoadData(gameData);
        }

        Debug.Log("Loading: " + gameData);
    }
    public void SaveGame()
    {
        dataPersistenceObjs = FindAllDataPersistenceObjects();
        
        // pass data to other scripts so they can update
        foreach (IDataPersistence dataObjs in dataPersistenceObjs)
        {
            dataObjs.SaveData(ref gameData);
        }
        Debug.Log("Saving: " + gameData.playerPosition);
        // save data to a file using data handler
        fileDataHandler.Save(gameData);
    }

    //public void OnApplicationQuit()
    //{
    //    SaveGame();
    //}

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
