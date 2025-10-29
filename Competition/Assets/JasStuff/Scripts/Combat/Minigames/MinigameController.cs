using UnityEngine;

public class MinigameController : MonoBehaviour
{
    private Combatant currCombatant;
    private float globalMinigameChance = 2f;
    private float minigameChance = 40f;
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

        //float roll = Random.Range(0f, 100f);
        //Debug.Log($"[MINIGAME] Rolled {roll:F1}% (threshold {minigameChance}%)");

        //string finalID = id;

        ////if (roll < globalMinigameChance)
        ////{
        ////    finalID = globalMinigameID;
        ////    MinigameManager.instance.StartMinigame(finalID, OnMinigameComplete);
        ////}
        ////else if (roll > globalMinigameChance && roll <= minigameChance)
        ////{
        //MinigameManager.instance.StartMinigame(finalID, OnMinigameComplete);
        ////}
        ////else
        ////{
        ////    Debug.Log("Nothing playing, no chance");
        ////}
    }

    private void OnMinigameComplete(MinigameManager.ResultType result)
    {
        if (currCombatant == null) return;
        currCombatant.OnMinigameResult(result);
    }
}
