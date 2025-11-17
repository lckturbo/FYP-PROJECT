using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager instance;

    [Header("Cutscene Prefab")]
    [SerializeField] private GameObject cutscenePrefab;
    [SerializeField] private float cutsceneDuration = 5f;

    [Header("Skip UI")]
    [SerializeField] private GameObject skipButtonUI;   // optional
    [SerializeField] private Button skipButton;          // optional

    private GameObject activeCutscene;
    private Coroutine cutsceneRoutine;
    private bool cutscenePlaying = false;
    private string targetScene;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void PlayCutsceneThenLoad(string nextScene)
    {
        targetScene = nextScene;

        if (cutscenePrefab == null)
        {
            Debug.LogError("Cutscene prefab not assigned!");
            SceneManager.LoadScene(nextScene);
            return;
        }

        if (activeCutscene != null)
            Destroy(activeCutscene);

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipCutscene);

        cutsceneRoutine = StartCoroutine(PlayCutsceneRoutine());
    }

    private IEnumerator PlayCutsceneRoutine()
    {
        cutscenePlaying = true;

        // Spawn cutscene
        activeCutscene = Instantiate(cutscenePrefab);

        // Enable skip UI
        if (skipButtonUI != null)
            skipButtonUI.SetActive(true);

        Animator anim = activeCutscene.GetComponent<Animator>();

        // Calculate duration if using animator
        if (anim != null)
        {
            AnimatorClipInfo[] clips = anim.GetCurrentAnimatorClipInfo(0);
            if (clips.Length > 0)
                cutsceneDuration = clips[0].clip.length;

            anim.SetTrigger("Play");
        }

        float t = 0f;

        while (t < cutsceneDuration)
        {
            if (!cutscenePlaying)
                yield break; // stopped early by skip

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        FinishCutscene();
    }

    private void Update()
    {
        if (!cutscenePlaying) return;

        // Keyboard skip:
        if (Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return))
        {
            SkipCutscene();
        }
    }

    /// <summary>
    /// Immediately skips the cutscene.
    /// </summary>
    public void SkipCutscene()
    {
        if (!cutscenePlaying) return;

        Debug.Log("[Cutscene] Skipped");

        cutscenePlaying = false;

        // Stop coroutine
        if (cutsceneRoutine != null)
            StopCoroutine(cutsceneRoutine);

        FinishCutscene();
    }

    private void FinishCutscene()
    {
        cutscenePlaying = false;

        if (activeCutscene != null)
            Destroy(activeCutscene);

        if (skipButtonUI != null)
            skipButtonUI.SetActive(false);

        SceneManager.LoadScene(targetScene);
    }
}
