using UnityEngine;
using System.Collections;

public class HitstopManager : MonoBehaviour
{
    public static HitstopManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public IEnumerator DoHitstop(float duration, TurnEngine turnEngine)
    {
        turnEngine.SetPaused(true);

        yield return new WaitForSecondsRealtime(duration);

        turnEngine.SetPaused(false);
    }

    public void TriggerHitstop(float duration)
    {
        var t = FindObjectOfType<TurnEngine>();
        StartCoroutine(DoHitstop(duration, t));
    }
}
