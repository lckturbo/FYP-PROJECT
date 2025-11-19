using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueTimePause : MonoBehaviour
{
    private void Update()
    {
        if (DialogueManager.Instance == null) return;

        if (SceneManager.GetActiveScene().name != "jasBattle")
        {
            if (DialogueManager.Instance.IsDialogueActive)
            {
                // Pause gameplay
                if (Time.timeScale != 0f)
                    Time.timeScale = 0f;
            }
            else
            {
                // Resume gameplay
                if (Time.timeScale != 1f)
                    Time.timeScale = 1f;
            }
        }
    }
}
