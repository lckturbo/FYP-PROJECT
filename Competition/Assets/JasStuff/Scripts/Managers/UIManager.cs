using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IDataPersistence
{
    public static UIManager instance;

    [Header("Buttons")]
    [SerializeField] private Button playBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button creditsBtn;
    [SerializeField] private Button exitBtn;

    [Header("UIs")]
    [SerializeField] private GameObject settingsUI;

    [Header("Settings")]
    [SerializeField] private Slider BGMSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] private Button backBtn;
    private bool isOpen;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    { 
        string scnName = scene.name;

        SetSettings();

        if (scnName == "Main")
        {
            AudioManager.instance.StopAllSounds();
            AudioManager.instance.PlaySound("MainMenuBGM");

            settingsBtn = GameObject.Find("SettingsBtn").GetComponent<Button>();
            creditsBtn = GameObject.Find("CreditsBtn").GetComponent<Button>();
            exitBtn = GameObject.Find("ExitBtn").GetComponent<Button>();

            if (settingsBtn || creditsBtn || exitBtn)
            {
                settingsBtn.onClick.RemoveAllListeners();
                settingsBtn.onClick.AddListener(() => ToggleSettings(!isOpen));

                creditsBtn.onClick.RemoveAllListeners();
                creditsBtn.onClick.AddListener(() =>
                {
                    GameManager.instance.ChangeScene("Credits");
                });

                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(() => Application.Quit());
            }
        }
        else if (scnName == "Lobby")
        {
            AudioManager.instance.StopAllSounds();
            AudioManager.instance.PlaySound("bgm");
            //AudioManager.instance.PlaySound("MainMenuBGM");

            playBtn = GameObject.Find("PlayBtn").GetComponent<Button>();
            settingsBtn = GameObject.Find("SettingsBtn").GetComponent<Button>();
            exitBtn = GameObject.Find("ReturnBtn").GetComponent<Button>();

            if (playBtn || settingsBtn || exitBtn)
            {
                playBtn.onClick.RemoveAllListeners();
                playBtn.onClick.AddListener(() => ASyncManager.instance.LoadLevelBtn("CharSelection"));

                settingsBtn.onClick.RemoveAllListeners();
                settingsBtn.onClick.AddListener(() => ToggleSettings(!isOpen));

                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(() =>
                {
                    GameManager.instance.ChangeScene("Main");
                    SaveLoadSystem.instance.SaveGame();
                });
            }
        }
    }

    public void SetSettings()
    {
        settingsUI = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "SettingsUI");

        if (settingsUI)
        {
            BGMSlider = settingsUI.GetComponentsInChildren<Slider>(true).FirstOrDefault(s => s.name == "BGMSlider");
            SFXSlider = settingsUI.GetComponentsInChildren<Slider>(true).FirstOrDefault(s => s.name == "SFXSlider");
            backBtn = settingsUI.GetComponentsInChildren<Button>(true).FirstOrDefault(s => s.name == "ExitBtn");

            if (BGMSlider || SFXSlider || backBtn)
            {
                BGMSlider.onValueChanged.RemoveAllListeners();
                BGMSlider.onValueChanged.AddListener(SettingsManager.instance.OnBGMVolumeChanged);
                BGMSlider.value = AudioManager.instance.GetBgmVol();

                SFXSlider.onValueChanged.RemoveAllListeners();
                SFXSlider.onValueChanged.AddListener(SettingsManager.instance.OnSFXVolumeChanged);
                SFXSlider.value = AudioManager.instance.GetSFXVol();

                backBtn.onClick.AddListener(() =>
                {
                    ToggleSettings(false);
                    SaveLoadSystem.instance.SaveGame();
                });
            }
        }
    }

    public void ToggleSettings(bool v)
    {
        isOpen = v;
        settingsUI.SetActive(v);
    }

    public bool isSettingsOpen()
    {
        return isOpen;
    }

    public void LoadData(GameData data)
    {
        if (BGMSlider != null) BGMSlider.value = data.bgmVolume;
        if (SFXSlider != null) SFXSlider.value = data.sfxVolume;
    }

    public void SaveData(ref GameData data) { }
}
