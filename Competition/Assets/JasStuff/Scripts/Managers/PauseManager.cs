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
    public bool IsPaused() => isPaused;
    [HideInInspector] public bool canPause = true;
    private bool isOpen = false;

    [Header("Stats UI")]
    [SerializeField] private StatsDisplay statsDisplay;
    [SerializeField] private RectTransform statsContainer;
    [SerializeField] private GameObject statRowPrefab;

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
        if (scn.name == "Main" || scn.name == "Lobby" || scn.name == "CharSelection" || scn.name == "Credits") return;

        pauseUI = GameObject.Find("PauseUI");
        resumeBtn = GameObject.Find("ResumeBtn").GetComponent<Button>();
        if (resumeBtn) resumeBtn.onClick.AddListener(() => Pause(false));
        settingsBtn = GameObject.Find("SettingsBtn").GetComponent<Button>();
        if (settingsBtn) settingsBtn.onClick.AddListener(() =>
        {
            //UIManager.instance.ToggleSettings(!isOpen);
            ToggleSettingsMenu();
        });
        menuBtn = GameObject.Find("MainMenuBtn").GetComponent<Button>();
        if (menuBtn) menuBtn.onClick.AddListener(() => QuitToMenu());
        if (statsDisplay == null) statsDisplay = FindObjectOfType<StatsDisplay>();
        ShowPauseUI(false);
    }
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Main" || SceneManager.GetActiveScene().name == "Lobby") return;
        if (!canPause) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Pause(!isPaused);
    }

    private void ToggleSettingsMenu()
    {
        isOpen = !isOpen; 
        UIManager.instance.ToggleSettings(isOpen);

        ShowPauseUI(!isOpen);
    }

    private void Pause(bool v)
    {
        isPaused = v;
        ShowPauseUI(v);
        Time.timeScale = v ? 0 : 1;
    }
    public void ShowPauseUI(bool v)
    {
        if (!pauseUI) return;
        pauseUI.SetActive(v);

        if (v)
        {
            if (statsDisplay) statsDisplay.DisplayStats();
        }
    }
    private void QuitToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseUI.SetActive(false);
        ASyncManager.instance.LoadLevelBtn("Main");
    }
}
