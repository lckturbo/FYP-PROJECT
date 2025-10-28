using System.Collections;
using UnityEngine;

public class FreezeFrame : BaseMinigame
{
    [SerializeField] private Animator anim;

    public override IEnumerator Run()
    {
        BattleManager.instance?.SetBattlePaused(true);
        if (anim)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("start");
            yield return new WaitForSecondsRealtime(4.0f);
        }
    }
}
