using System.Collections;
using UnityEngine;

public abstract class BaseMinigame : MonoBehaviour
{
    public MinigameManager.ResultType Result { get; protected set; } = MinigameManager.ResultType.Fail;
    public abstract IEnumerator Run();
}
