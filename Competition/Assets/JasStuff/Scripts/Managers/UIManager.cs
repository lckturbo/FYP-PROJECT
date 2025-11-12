using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IDataPersistence
{
    public static UIManager instance;

    [Header("MainMenu / Lobby Buttons")]
    [SerializeField] private Button loadBtn;
    [SerializeField] private Button newBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button creditsBtn;
    [SerializeField] private Button exitBtn;

    [Header("Pause UI/Buttons")]
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private Button resumeBtn;
    [SerializeField] private Button menuBtn;

    [Header("Settings UI/Buttons")]
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private Slider BGMSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Button backBtn;

    [Header("Stats UI (For SampleScene)")]
    [SerializeField] private StatsDisplay statsDisplay;

    private bool isOpen;
    private bool isPaused;
    public bool IsSettingsOpen() => isOpen;
    public bool IsPaused() => isPaused;
    [HideInInspector] public bool canPause = true;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "Main" || SceneManager.GetActiveScene().name == "Lobby" || SceneManager.GetActiveScene().name == "Credits") return;
        if (!canPause || isOpen || (MinigameManager.instance && MinigameManager.instance.IsMinigameActive())) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause(!isPaused);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string scnName = scene.name;
        AudioManager.instance.StopAllSounds();

        canPause = true;

        switch (scnName)
        {
            case "Main":
                AudioManager.instance.PlaySound("NewAdventure");
                SetupMainMenu();
                return;
            case "Lobby":
                AudioManager.instance.PlaySound("bgm");
                SetupLobbyMenu();
                RefreshLobbyButtons(); 
                return;
            case "CharSelection":
                AudioManager.instance.PlaySound("MainMenuBGM");
                return;
            case "SampleScene":
                AudioManager.instance.PlaySound("BattleForPeace");
                SetupMainGameMenu();
                return;
            case "jasBattle":
                AudioManager.instance.PlaySound("Pride");
                SetupBattleMenu();
                return;
            case "Credits":
                AudioManager.instance.PlaySound("MainMenuBGM");
                SetupCreditsMenu();
                return;
        }
    }

    // ---- SETUPS ---- //
    private void SetupMainMenu()
    {
        settingsUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "SettingsUI");
        if (settingsUI) SetupSettingUI();

        settingsBtn = GameObject.Find("SettingsBtn")?.GetComponent<Button>();
        creditsBtn = GameObject.Find("CreditsBtn")?.GetComponent<Button>();
        exitBtn = GameObject.Find("ExitBtn")?.GetComponent<Button>();

        if (settingsBtn || creditsBtn || exitBtn)
        {
            settingsBtn.onClick.RemoveAllListeners();
            settingsBtn.onClick.AddListener(() => ToggleSettings(!isOpen));

            creditsBtn.onClick.RemoveAllListeners();
            creditsBtn.onClick.AddListener(() => GameManager.instance.ChangeScene("Credits"));

            exitBtn.onClick.RemoveAllListeners();
            exitBtn.onClick.AddListener(() => Application.Quit());
        }
    }
    private void SetupLobbyMenu()
    {
        settingsUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "SettingsUI");
        if (settingsUI) SetupSettingUI();

        newBtn = GameObject.Find("NewBtn")?.GetComponent<Button>();
        loadBtn = GameObject.Find("LoadBtn")?.GetComponent<Button>();
        settingsBtn = GameObject.Find("SettingsBtn")?.GetComponent<Button>();
        exitBtn = GameObject.Find("ReturnBtn")?.GetComponent<Button>();

        if (newBtn || loadBtn || settingsBtn || exitBtn)
        {
            newBtn.onClick.RemoveAllListeners();
            newBtn.onClick.AddListener(() =>
            {
                SaveLoadSystem.instance.NewGame();
                if (CutsceneManager.instance != null)
                {
                    CutsceneManager.instance.PlayCutsceneThenLoad("CharSelection");
                }
                else
                {
                    ASyncManager.instance.LoadLevelBtn("CharSelection");
                }
            });

            loadBtn.onClick.RemoveAllListeners();
            loadBtn.onClick.AddListener(() =>
            {
                SaveLoadSystem.instance.LoadGame();
                ASyncManager.instance.LoadLevelBtn("SampleScene");
            });

            settingsBtn.onClick.RemoveAllListeners();
            settingsBtn.onClick.AddListener(() => ToggleSettings(!isOpen));

            exitBtn.onClick.RemoveAllListeners();
            exitBtn.onClick.AddListener(() =>
            {
                GameManager.instance.ChangeScene("Main");
                //SaveLoadSystem.instance.SaveGame();
            });
            RefreshLobbyButtons();
        }
    }

    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            RefreshLobbyButtons();
        }
    }

    private void RefreshLobbyButtons()
    {
        loadBtn = GameObject.Find("LoadBtn")?.GetComponent<Button>();

        if (loadBtn != null && SaveLoadSystem.instance != null)
        {
            bool hasSave = SaveLoadSystem.instance.HasSaveFile();
            loadBtn.interactable = hasSave;
            Debug.Log($"[UIManager] Load button refreshed. HasSave: {hasSave}");
        }
    }
    private void SetupMainGameMenu()
    {
        SetupPauseUI();
        settingsUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "SettingsUI");
        if (settingsUI) SetupSettingUI();
        if (pauseUI && !statsDisplay)
        {
            if (!statsDisplay)
                statsDisplay = pauseUI.GetComponentsInChildren<StatsDisplay>().FirstOrDefault(s => s.name == "StatsContainer");
            else
                statsDisplay = null;
        }
    }
    private void SetupBattleMenu()
    {
        SetupPauseUI();
        settingsUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "SettingsUI");
        if (settingsUI) SetupSettingUI();
    }
    private void SetupCreditsMenu()
    {
        exitBtn = GameObject.Find("Black").GetComponent<Button>();

        if (exitBtn)
        {
            exitBtn.onClick.RemoveAllListeners();
            exitBtn.onClick.AddListener(() =>
            {
                //SaveLoadSystem.instance.NewGame();
                GameManager.instance.ChangeScene("Main");
            });
        }
    }

    // ---- SETTINGS ---- //
    public void SetupSettingUI()
    {
        if (settingsUI)
        {
            BGMSlider = settingsUI.GetComponentsInChildren<Slider>(true).FirstOrDefault(s => s.name == "BGMSlider");
            SFXSlider = settingsUI.GetComponentsInChildren<Slider>(true).FirstOrDefault(s => s.name == "SFXSlider");
            backBtn = settingsUI.GetComponentsInChildren<Button>(true).FirstOrDefault(s => s.name == "ExitBtn");

            if (BGMSlider || SFXSlider || backBtn)
            {
                BGMSlider.SetValueWithoutNotify(AudioManager.instance.GetBgmVol());
                SFXSlider.SetValueWithoutNotify(AudioManager.instance.GetSFXVol());

                BGMSlider.onValueChanged.RemoveAllListeners();
                BGMSlider.onValueChanged.AddListener(SettingsManager.instance.OnBGMVolumeChanged);
                //BGMSlider.value = AudioManager.instance.GetBgmVol();

                SFXSlider.onValueChanged.RemoveAllListeners();
                SFXSlider.onValueChanged.AddListener(SettingsManager.instance.OnSFXVolumeChanged);
                //SFXSlider.value = AudioManager.instance.GetSFXVol();

                backBtn.onClick.AddListener(() =>
                {
                    ToggleSettings(false);
                    
                    if(SceneManager.GetActiveScene().name == "Main" || SceneManager.GetActiveScene().name == "Lobby")
                        SaveLoadSystem.instance.SaveSettingsOnly();
                    else
                        SaveLoadSystem.instance.SaveGame();

                    if (isPaused)
                        ShowPauseUI(true);
                });
            }
        }
    }
    public void ToggleSettings(bool v)
    {
        if (settingsUI)
        {
            isOpen = v;
            settingsUI.SetActive(v);
        }
    }

    // ---- PAUSE ---- //
    private void SetupPauseUI()
    {
        pauseUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "PauseUI");

        if (pauseUI)
        {
            resumeBtn = pauseUI.GetComponentsInChildren<Button>().FirstOrDefault(s => s.name == "ResumeBtn");
            if (resumeBtn) resumeBtn.onClick.AddListener(() => TogglePause(false));
            settingsBtn = pauseUI.GetComponentsInChildren<Button>().FirstOrDefault(s => s.name == "SettingsBtn");
            if (settingsBtn) settingsBtn.onClick.AddListener(() =>
            {
                ToggleSettings(true);
                ShowPauseUI(false);
            });
            menuBtn = pauseUI.GetComponentsInChildren<Button>().FirstOrDefault(s => s.name == "MainMenuBtn");
            if (menuBtn) menuBtn.onClick.AddListener(() =>
            {
                TogglePause(false);
                if (SceneManager.GetActiveScene().name == "SampleScene")
                    SaveLoadSystem.instance.SaveGame();

                if(SceneManager.GetActiveScene().name == "jasBattle")
                {
                    TurnEngine turnEngine = FindObjectOfType<TurnEngine>();
                    if(turnEngine != null)
                        turnEngine.BattleSpeed = 1f;
                    Time.timeScale = 1f;
                }
                ASyncManager.instance.LoadLevelBtn("Main");
            });
            if (statsDisplay == null)
                statsDisplay = FindObjectOfType<StatsDisplay>();
        }
    }

    private void TogglePause(bool v)
    {
        isPaused = v;
        Time.timeScale = v ? 0 : 1;

        Debug.Log($"[UIManager] Pause toggled: {v}, timeScale = {Time.timeScale}");

        if (SceneManager.GetActiveScene().name == "jasBattle")
        {
            BattleManager.instance.SetBattlePaused(v);
            Debug.Log($"[UIManager] Battle paused: {v}");
        }

        ShowPauseUI(v);
    }

    public void ShowPauseUI(bool v)
    {
        if (!pauseUI) return;
        pauseUI.SetActive(v);
        Debug.Log($"[UIManager] Pause toggled: {v}, timeScale = {Time.timeScale}");
        if (v && statsDisplay) statsDisplay.DisplayStats();
    }

    // ---- SAVELOAD ---- //
    public void LoadData(GameData data)
    {
        if (BGMSlider != null) BGMSlider.value = data.bgmVolume;
        if (SFXSlider != null) SFXSlider.value = data.sfxVolume;
    }

    public void SaveData(ref GameData data) { }
}
