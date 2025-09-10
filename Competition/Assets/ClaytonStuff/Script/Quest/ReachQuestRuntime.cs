using UnityEngine;

public class ReachQuestRuntime : Quest
{
    private Transform player;
    private Transform target;

    public override void StartQuest()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        var data = (ReachQuestData)questData;
        GameObject targetObj = GameObject.Find(data.targetObjectName);
        if (targetObj != null)
        {
            target = targetObj.transform;
            Debug.Log($"Started Reach Quest: Go to {target.name}");
        }
        else
        {
            Debug.LogError($"Target object '{data.targetObjectName}' not found in scene!");
        }
    }

    public override void CheckProgress()
    {
        var data = (ReachQuestData)questData;
        if (target == null || player == null) return;

        if (Vector3.Distance(player.position, target.position) <= data.reachRadius && !isCompleted)
        {
            CompleteQuest();
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    //  Show target name in quest UI
    public override string GetProgressText()
    {
        if (isCompleted) return "Reached!";
        return target != null ? $"Go to {target.name}" : "Target missing";
    }
}
