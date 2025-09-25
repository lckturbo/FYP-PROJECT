using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);
    }

    public void Update()
    {
        // for testing
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
            SceneManager.LoadScene("jas");
    }
    public void ChangeScene(string scn)
    {
        SceneManager.LoadScene(scn);
    }
}
