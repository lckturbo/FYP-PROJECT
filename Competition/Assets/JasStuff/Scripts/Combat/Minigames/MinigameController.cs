using UnityEngine;

public class MinigameController : MonoBehaviour
{
    private Combatant currCombatant;

    /// <summary>
    /// Called directly from animation event.
    /// Example: Animation Event calls TriggerMinigame("Wordle")
    /// </summary>
    public void TriggerMinigame(string id)
    {
        currCombatant = GetComponentInParent<Combatant>();
        if (MinigameManager.instance == null) return;

        Debug.Log($"[MINIGAME] Animation-triggered minigame: {id}");
        MinigameManager.instance.TriggerMinigameFromAnimation(id, OnMinigameComplete);
    }

    private void OnMinigameComplete(MinigameManager.ResultType result)
    {
        if (currCombatant == null) return;
        currCombatant.OnMinigameResult(result);
    }
}
