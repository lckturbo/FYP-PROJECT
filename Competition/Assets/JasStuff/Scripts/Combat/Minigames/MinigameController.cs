using UnityEngine;

public class MinigameController : MonoBehaviour
{
    [SerializeField] private TurnEngine engine; // Will auto-assign if left empty
    private Combatant currCombatant;
    private string globalMinigameID = "TakeABreak";
    private float globalMinigameChance = 2f;
    private float minigameChance = 135;

    private void Awake()
    {
        //  Auto-find TurnEngine in scene if not assigned
        if (engine == null)
        {
            engine = FindObjectOfType<TurnEngine>();
            if (engine == null)
                Debug.LogWarning("[MinigameController] No TurnEngine found in scene!");
        }
    }

    public void TriggerMinigame(string id)
    {
        //  Skip minigame if Auto Battle is ON
        if (engine != null && engine.autoBattle)
        {
            Debug.Log("[MINIGAME] Auto Battle is ON — skipping minigame trigger.");
            return;
        }

        currCombatant = GetComponentInParent<Combatant>();

        if (currCombatant == null || !currCombatant.isLeader)
        {
            Debug.Log("[MINIGAME] Not leader — minigame ignored.");
            return;
        }
        if (MinigameManager.instance == null) return;

        float roll = Random.Range(0f, 100f);
        Debug.Log($"[MINIGAME] Rolled {roll:F1}% (threshold {minigameChance}%)");

        string finalID = id;

        //You can re - enable your chance logic later if desired
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

    private void OnMinigameComplete(MinigameManager.ResultType result, string id)
    {
        if (currCombatant == null) return;
        currCombatant.OnMinigameResult(result, id);
    }
}
