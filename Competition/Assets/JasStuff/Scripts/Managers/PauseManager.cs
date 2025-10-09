using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button menuBtn;

    private bool isPaused = false;
    private bool isOpen = false;
    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scn, LoadSceneMode mode)
    {
        if (scn.name == "Main" || scn.name == "Lobby" || scn.name == "CharSelection") return;

        pauseUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "PauseUI");
        if (pauseUI)
        {
            resumeBtn = pauseUI.GetComponentsInChildren<Button>(true).FirstOrDefault(s => s.name == "ResumeBtn");
            settingsBtn = pauseUI.GetComponentsInChildren<Button>(true).FirstOrDefault(s => s.name == "SettingsBtn");
            menuBtn = pauseUI.GetComponentsInChildren<Button>(true).FirstOrDefault(s => s.name == "MainMenuBtn");

            if(resumeBtn || settingsBtn || menuBtn)
            {
                resumeBtn.onClick.AddListener(() => Pause(false));
                settingsBtn.onClick.AddListener(()=> UIManager.instance.ToggleSettings(!isOpen));
                menuBtn.onClick.AddListener(() => QuitToMenu());

            }
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Main" || SceneManager.GetActiveScene().name == "Lobby") return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Pause(!isPaused);
    }

    private void Pause(bool v)
    {
        isPaused = v;
        pauseUI.SetActive(v);
        Time.timeScale = v ? 0 : 1;
    }

    private void QuitToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseUI.SetActive(false);
        //GameManager.instance.ChangeScene("Main");
        // TODO: loading scn back to menu
        ASyncManager.instance.LoadLevelBtn("Main");
    }
}
