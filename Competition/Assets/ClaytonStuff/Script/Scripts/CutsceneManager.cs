using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager instance;

    [Header("Cutscene Prefab")]
    [SerializeField] private GameObject cutscenePrefab;
    [SerializeField] private float cutsceneDuration = 5;

    private GameObject activeCutscene;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Spawns the cutscene prefab and plays it before switching scene.
    /// </summary>
    public void PlayCutsceneThenLoad(string nextScene)
    {
        if (cutscenePrefab == null)
        {
            Debug.LogError("Cutscene prefab not assigned!");
            SceneManager.LoadScene(nextScene);
            return;
        }

        if (activeCutscene != null)
        {
            Destroy(activeCutscene);
        }

        StartCoroutine(PlayCutsceneRoutine(nextScene));
    }

    private IEnumerator PlayCutsceneRoutine(string nextScene)
    {
        // Spawn the cutscene prefab
        activeCutscene = Instantiate(cutscenePrefab);

        // Get animator if it exists
        Animator anim = activeCutscene.GetComponent<Animator>();

        if (anim != null)
        {
            AnimatorClipInfo[] clips = anim.GetCurrentAnimatorClipInfo(0);
            if (clips.Length > 0)
                cutsceneDuration = clips[0].clip.length;
            anim.SetTrigger("Play");
        }

        // Wait for animation to finish
        yield return new WaitForSecondsRealtime(cutsceneDuration);

        // Load next scene
        SceneManager.LoadScene(nextScene);
    }
}
