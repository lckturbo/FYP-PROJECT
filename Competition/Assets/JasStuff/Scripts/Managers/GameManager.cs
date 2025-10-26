using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        // If this GameObject is a child
        if (transform.parent != null)
        {
            // Detach from its parent first
            transform.SetParent(null);
        }
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SaveLoadSystem.instance.NewGame();
    }

    public void Update()
    {
        // for testing
        if (SceneManager.GetActiveScene().name == "Main")
        {
            bool settingsOpen = UIManager.instance != null && UIManager.instance.IsSettingsOpen();

            if (!settingsOpen && (Input.anyKeyDown || Input.GetMouseButtonDown(0)) && !EventSystem.current.IsPointerOverGameObject())
                ASyncManager.instance.LoadLevelBtn("Lobby");
        }
    }
    public void ChangeScene(string scn)
    {
        SceneManager.LoadScene(scn);
    }
}
