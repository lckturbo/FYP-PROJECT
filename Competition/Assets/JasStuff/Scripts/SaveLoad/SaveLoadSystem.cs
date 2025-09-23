using UnityEngine;

public class SaveLoadSystem : MonoBehaviour
{
    public static SaveLoadSystem instance;
    private GameData gameData;
    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        LoadGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }
    public void LoadGame()
    {
        // TODO: load saved data from file using data handler
        // if no data, initialize new game
        if (gameData == null) 
            NewGame();

        // TODO: push loaded data to all other scripts that need it
    }
    public void SaveGame()
    {
        // TODO: pass data to other scripts so they can update
        // TODO: save data to a file using data handler
    }

    public void OnApplicationQuit()
    {
        SaveGame();
    }
}
