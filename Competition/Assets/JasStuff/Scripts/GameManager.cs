using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Button playBtn;
    [SerializeField] private Button creditsBtn;
    [SerializeField] private Button exitBtn;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
            Destroy(gameObject);

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
            creditsBtn = GameObject.Find("CreditsBtn").GetComponent<Button>();
            exitBtn = GameObject.Find("ExitBtn").GetComponent<Button>();

            if (creditsBtn || exitBtn)
            {
                creditsBtn.onClick.RemoveAllListeners();
                creditsBtn.onClick.AddListener(() => ChangeScene("CreditsScene"));
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(() => Application.Quit());
            }
        }
        else if (scnName == "Lobby")
        {
            playBtn = GameObject.Find("PlayBtn").GetComponent<Button>();
            exitBtn = GameObject.Find("ReturnBtn").GetComponent<Button>();

            if (playBtn || exitBtn)
            {
                playBtn.onClick.RemoveAllListeners();
                playBtn.onClick.AddListener(() => ChangeScene("CharSelection"));
                exitBtn.onClick.RemoveAllListeners();
                exitBtn.onClick.AddListener(() => ChangeScene("Main"));
            }
        }
    }

    public void Update()
    {
        // for testing
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
            SceneManager.LoadScene("jas");

        if (SceneManager.GetActiveScene().name == "Main")
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                ChangeScene("Lobby");
        }
    }
    public void ChangeScene(string scn)
    {
        SceneManager.LoadScene(scn);
    }
}
