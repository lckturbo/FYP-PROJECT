using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ASyncManager : MonoBehaviour
{
    public static ASyncManager instance;
    [Header("Screens")]
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject loadingScreen;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

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

        if (scnName == "Main" || scnName == "Lobby")
        {
            mainScreen = GameObject.Find("MainCanvas");
            loadingScreen = GameObject.Find("LoadingCanvas");
        }
    }
    public void LoadLevelBtn(string levelToLoad)
    {
        mainScreen.SetActive(false);
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(levelToLoad));
    }

    IEnumerator LoadLevelAsync(string levelToLoad)
    {
        float minLoadTime = 5f;
        float elapsedTime = 0f;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        while (!loadOperation.isDone)
        {
            elapsedTime += Time.deltaTime;

            float progValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingSlider.value = Mathf.Min(progValue, elapsedTime / minLoadTime);

            if (progValue >= 1f && elapsedTime >= minLoadTime)
                loadOperation.allowSceneActivation = true;

            yield return null;
        }
    }
}
