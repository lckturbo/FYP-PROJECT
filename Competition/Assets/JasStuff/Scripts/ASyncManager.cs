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

        isLoading = false;
    }
}
