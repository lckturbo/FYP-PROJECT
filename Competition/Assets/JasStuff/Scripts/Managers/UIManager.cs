using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
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

        if (scnName == "Main")
        {
            AudioManager.instance.StopAllSounds();
            //AudioManager.instance.PlaySound("bgm");
            AudioManager.instance.PlaySound("MainMenuBGM");

            creditsBtn = GameObject.Find("CreditsBtn").GetComponent<Button>();
            exitBtn = GameObject.Find("ExitBtn").GetComponent<Button>();

            if (creditsBtn || exitBtn)
            {
                creditsBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(() => Application.Quit());
            }
        }
        else if (scnName == "Lobby")
        {
            AudioManager.instance.StopAllSounds();
            AudioManager.instance.PlaySound("bgm");

            playBtn = GameObject.Find("PlayBtn").GetComponent<Button>();
            settingsBtn = GameObject.Find("SettingsBtn").GetComponent<Button>();
            settingsUI = GameObject.Find("SettingsUI");
            exitBtn = GameObject.Find("ReturnBtn").GetComponent<Button>();

            if (settingsUI)
            {
                BGMSlider = GameObject.Find("BGMSlider").GetComponent<Slider>();
                SFXSlider = GameObject.Find("SFXSlider").GetComponent<Slider>();

                if(BGMSlider || SFXSlider)
                {
                    BGMSlider.onValueChanged.RemoveAllListeners();
                    //BGMSlider.onValueChanged.AddListener(SettingsManager.instance.OnBGMVolumeChanged);
                    //BGMSlider.value = Mathf.Clamp01(AudioManager.instance.GetBgmVol());
                    SFXSlider.onValueChanged.RemoveAllListeners();
                }
                settingsUI.SetActive(false);
            }
            if (playBtn || settingsBtn || exitBtn)
            {
                playBtn.onClick.RemoveAllListeners();
                playBtn.onClick.AddListener(() => ASyncManager.instance.LoadLevelBtn("CharSelection"));
                settingsBtn.onClick.RemoveAllListeners();
                settingsBtn.onClick.AddListener(() => ToggleSettings(!isOpen));
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(() => GameManager.instance.ChangeScene("Main"));
            }
        }
    }

    private void ToggleSettings(bool v)
    {
        isOpen = v;
        settingsUI.SetActive(v);
    }
}
