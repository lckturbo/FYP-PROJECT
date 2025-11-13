using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    private GameObject currentArrow;

    public void ShowArrow(Transform target)
    {
        if (currentArrow != null) Destroy(currentArrow);
        currentArrow = Instantiate(arrowPrefab, target.position + Vector3.up * 1.4f, Quaternion.identity);
        currentArrow.transform.SetParent(target);
    }

    public void HideArrow()
    {
        if (currentArrow != null) Destroy(currentArrow);
    }
}
