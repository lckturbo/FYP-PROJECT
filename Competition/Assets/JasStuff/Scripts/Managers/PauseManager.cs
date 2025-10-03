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

        pauseUI = GameObject.Find("PauseUI");
        resumeBtn = GameObject.Find("ResumeBtn").GetComponent<Button>();
        if (resumeBtn) resumeBtn.onClick.AddListener(() => Pause(false));
        settingsBtn = GameObject.Find("SettingsBtn").GetComponent<Button>();
        menuBtn = GameObject.Find("MainMenuBtn").GetComponent<Button>();
        if(menuBtn) menuBtn.onClick.AddListener(() => QuitToMenu());
        pauseUI.SetActive(false);
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
