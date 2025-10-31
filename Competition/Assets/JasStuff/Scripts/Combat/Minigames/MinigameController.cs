using UnityEngine;

public class MinigameController : MonoBehaviour
{
    private Combatant currCombatant;
    private string globalMinigameID = "TakeABreak";
    private float globalMinigameChance = 2f;
    private float minigameChance = 35f;

    public void TriggerMinigame(string id)
    {
        currCombatant = GetComponentInParent<Combatant>();
        if (MinigameManager.instance == null) return;

        Debug.Log($"[MINIGAME] Animation-triggered minigame: {id}");

        float roll = Random.Range(0f, 100f);
        Debug.Log($"[MINIGAME] Rolled {roll:F1}% (threshold {minigameChance}%)");

        string finalID = id;

        if (roll < globalMinigameChance)
        {
            finalID = globalMinigameID;
            MinigameManager.instance.TriggerMinigameFromAnimation(finalID, OnMinigameComplete);
        }
        else if (roll > globalMinigameChance && roll <= minigameChance)
        {
            MinigameManager.instance.TriggerMinigameFromAnimation(finalID, OnMinigameComplete);
        }
        else
        {
            Debug.Log("Nothing playing, no chance");
        }
    }

    private void OnMinigameComplete(MinigameManager.ResultType result)
    {
        if (currCombatant == null) return;
        currCombatant.OnMinigameResult(result);
    }
}
