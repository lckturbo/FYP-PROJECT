using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SceneMaterialSwitcher : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material battleMaterial;

    [Header("Optional Renderer Override")]
    [SerializeField] private Renderer targetRenderer; // auto-detected if null

    private void Awake()
    {
        if (!targetRenderer)
            targetRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        UpdateMaterialForScene();
        // Optional: auto-update on scene load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMaterialForScene();
    }

    private void UpdateMaterialForScene()
    {
        if (!targetRenderer) return;

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "jasBattle")
        {
            if (battleMaterial != null)
                targetRenderer.material = battleMaterial;
            Debug.Log($"[SceneMaterialSwitcher] {name}: switched to Battle Material (scene: {currentScene})");
        }
        else
        {
            if (defaultMaterial != null)
                targetRenderer.material = defaultMaterial;
            Debug.Log($"[SceneMaterialSwitcher] {name}: restored Default Material (scene: {currentScene})");
        }
    }
}
