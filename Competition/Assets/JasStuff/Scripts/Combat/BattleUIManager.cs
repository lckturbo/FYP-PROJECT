using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager instance;

    [Header("Assign all battle UI canvases here")]
    public Canvas[] canvases;

    void Awake()
    {
        if(instance == null)
            instance = this;
    }

    public void HideAllUI()
    {
        foreach (var c in canvases)
            if (c) c.gameObject.SetActive(false);
    }

    public void ShowAllUI()
    {
        foreach (var c in canvases)
            if (c) c.gameObject.SetActive(true);
    }
}
