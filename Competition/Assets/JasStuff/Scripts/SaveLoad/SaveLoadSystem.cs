using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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
    }

    private void Start()
    {
        this.fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.dataPersistenceObjs = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }
    public void LoadGame()
    {
        // load saved data from file using data handler
        this.gameData = fileDataHandler.Load();
        // if no data, initialize new game
        if (this.gameData == null) NewGame();

        // push loaded data to all other scripts that need it
        foreach(IDataPersistence dataObjs in dataPersistenceObjs)
        {
            dataObjs.LoadData(gameData);
        }
    }
    public void SaveGame()
    {
        // pass data to other scripts so they can update
        foreach (IDataPersistence dataObjs in dataPersistenceObjs)
        {
            dataObjs.SaveData(ref gameData);
        }
        // save data to a file using data handler
        fileDataHandler.Save(gameData);
    }

    public void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjs = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjs);
    }
}
