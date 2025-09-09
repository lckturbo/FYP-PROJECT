using UnityEngine;

public class CollectionQuestRuntime : Quest
{
    private int currentAmount = 0;

    public override void StartQuest()
    {
        var data = (CollectionQuestData)questData;
        Debug.Log($"Started Collection Quest: Collect {data.requiredAmount} {data.itemName}");
    }

    public override void CheckProgress()
    {
        var data = (CollectionQuestData)questData;
        if (currentAmount >= data.requiredAmount && !isCompleted)
        {
            CompleteQuest();
        }
    }

    public void AddItem(string collectedItem)
    {
        var data = (CollectionQuestData)questData;
        if (collectedItem == data.itemName && !isCompleted)
        {
            currentAmount++;
            Debug.Log($"{collectedItem} collected ({currentAmount}/{data.requiredAmount})");
            CheckProgress();
        }
    }

    //  Progress string
    public override string GetProgressText()
    {
        var data = (CollectionQuestData)questData;
        return $"{currentAmount}/{data.requiredAmount}";
    }
}
