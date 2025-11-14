using UnityEngine;

public class MinigameController : MonoBehaviour
{
    [SerializeField] private TurnEngine engine;
    private Combatant currCombatant;
    private string globalMinigameID = "TakeABreak";
    private float globalMinigameChance = 2f;
    private float minigameChance;

    private void Awake()
    {
        if (engine == null)
        {
            engine = FindObjectOfType<TurnEngine>();
            if (engine == null)
                Debug.LogWarning("[MinigameController] No TurnEngine found in scene!");
        }
    }

    public void TriggerMinigame(string id)
    {
        //if (engine != null && engine.autoBattle) return;

        //currCombatant = GetComponentInParent<Combatant>();

        //if (currCombatant != null && currCombatant.blockMinigames)
        //{
        //    Debug.Log("[MINIGAME] Skipped because blockMinigames is TRUE.");
        //    return;
        //}

        //if (!currCombatant.isLeader)
        //    minigameChance = 15;
        //else
        //    minigameChance = 45;

        //if (MinigameManager.instance == null) return;

        //float roll = Random.Range(0f, 100f);
        //Debug.Log($"[MINIGAME] Rolled {roll:F1}% (threshold {minigameChance}%)");

        //string finalID = id;

        //if (roll < globalMinigameChance)
        //{
        //    finalID = globalMinigameID;
        //    MinigameManager.instance.TriggerMinigameFromAnimation(finalID, OnMinigameComplete);
        //}
        //else if (roll > globalMinigameChance && roll <= minigameChance)
        //{
        //    MinigameManager.instance.TriggerMinigameFromAnimation(finalID, OnMinigameComplete);
        //}
        //else
        //{
        //    Debug.Log("Nothing playing, no chance");
        //}
    }

    private void OnMinigameComplete(MinigameManager.ResultType result, string id)
    {
        if (currCombatant == null) return;
        currCombatant.OnMinigameResult(result, id);
    }
}
