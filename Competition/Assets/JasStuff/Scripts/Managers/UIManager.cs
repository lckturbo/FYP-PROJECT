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
            AudioManager.instance.PlaySound("bgm");

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

            if (playBtn || settingsBtn || settingsUI || exitBtn)
            {
                settingsUI.SetActive(false);

                playBtn.onClick.RemoveAllListeners();
                playBtn.onClick.AddListener(() => ASyncManager.instance.LoadLevelBtn("CharSelection"));
                settingsBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(() => GameManager.instance.ChangeScene("Main"));
            }
        }
    }

    private void Start()
    {
        
    }
}
