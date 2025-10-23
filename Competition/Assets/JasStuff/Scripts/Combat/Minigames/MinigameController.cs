using UnityEngine;

public class MinigameController : MonoBehaviour
{
    private Combatant currCombatant;
    public void TriggerMinigame(string id)
    {
        currCombatant = GetComponentInParent<Combatant>();

        if (MinigameManager.instance != null)
            MinigameManager.instance.StartMinigame(id, OnMinigameComplete);
    }

    private void OnMinigameComplete(MinigameManager.ResultType result)
    {
        if (currCombatant == null) return;
        currCombatant.OnMinigameResult(result);
    }
}
