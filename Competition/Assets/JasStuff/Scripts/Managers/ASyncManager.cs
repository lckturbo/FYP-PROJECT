using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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

    public UnityEvent onStart;
    public UnityEvent onEnd;

    private bool isLoading = false;

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);
    }

    public void LoadLevelBtn(string levelToLoad)
    {
        if (isLoading) return;

        isLoading = true;
        if(mainScreen) mainScreen.SetActive(false);
        loadingScreen.SetActive(true);
        onStart?.Invoke();

        StartCoroutine(LoadLevelAsync(levelToLoad));
    }

    IEnumerator LoadLevelAsync(string levelToLoad)
    {
        float minLoadTime = 3f;
        float elapsedTime = 0f;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        while (!loadOperation.isDone)
        {
            elapsedTime += Time.deltaTime;

            float progValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingSlider.value = Mathf.Min(progValue, elapsedTime / minLoadTime);

            if (progValue >= 1f && elapsedTime >= minLoadTime)
            {
                onEnd?.Invoke();
                loadOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        isLoading = false;
    }
}
