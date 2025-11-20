using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueTimePause : MonoBehaviour
{
    private void Update()
    {
        if (DialogueManager.Instance == null) return;

        // Do nothing in battle scene
        if (SceneManager.GetActiveScene().name == "jasBattle")
            return;

        // If pause menu is open ? DO NOT override time scale
        if (UIManager.instance != null && UIManager.instance.IsPaused())
            return;

        // Normal dialogue pause behavior
        if (DialogueManager.Instance.IsDialogueActive)
        {
            if (Time.timeScale != 0f)
                Time.timeScale = 0f;
        }
        else
        {
            if (Time.timeScale != 1f)
                Time.timeScale = 1f;
        }
    }
}
