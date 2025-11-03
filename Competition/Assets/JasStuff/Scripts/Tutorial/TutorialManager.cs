using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(gameObject);
    }
}
