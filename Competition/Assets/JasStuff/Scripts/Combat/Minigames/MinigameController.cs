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
        if (engine != null && engine.autoBattle) return;

        currCombatant = GetComponentInParent<Combatant>();

        if (currCombatant != null && currCombatant.blockMinigames)
        {
            Debug.Log("[MINIGAME] Skipped because blockMinigames is TRUE.");
            return;
        }

        if (!currCombatant.isLeader)
            minigameChance = 7;
        else
            minigameChance = 15;

        if (MinigameManager.instance == null) return;

        float roll = Random.Range(0f, 50f);
        Debug.Log($"Roll = {roll}");

        bool playGlobal = roll < globalMinigameChance;
        bool playLocal = !playGlobal && roll < minigameChance;

        if (playGlobal)
        {
            MinigameManager.instance.TriggerMinigameFromAnimation(globalMinigameID, OnMinigameComplete);
        }
        else if (playLocal)
        {
            MinigameManager.instance.TriggerMinigameFromAnimation(id, OnMinigameComplete);
        }
        else
        {
            Debug.Log("No minigame triggered.");
        }
    }

    private void OnMinigameComplete(MinigameManager.ResultType result, string id)
    {
        if (currCombatant == null) return;
        currCombatant.OnMinigameResult(result, id);
    }
}
