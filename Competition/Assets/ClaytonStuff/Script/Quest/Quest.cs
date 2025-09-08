using UnityEngine;

public abstract class Quest : MonoBehaviour
{
    public QuestData questData;
    public bool isCompleted;

    public delegate void QuestCompleted(Quest quest);
    public event QuestCompleted OnQuestCompleted;

    public abstract void StartQuest();
    public abstract void CheckProgress();

    //  Optional override for progress reporting
    public virtual string GetProgressText()
    {
        return isCompleted ? "Completed" : "";
    }

    protected void CompleteQuest()
    {
        isCompleted = true;
        Debug.Log($"{questData.questName} Completed!");
        OnQuestCompleted?.Invoke(this);
    }
}
