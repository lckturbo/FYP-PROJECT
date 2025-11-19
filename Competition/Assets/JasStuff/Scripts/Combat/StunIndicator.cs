using UnityEngine;

public class StunIndicator : MonoBehaviour
{
    [SerializeField] private GameObject stunPrefab;
    [SerializeField] private float yOffset = 1.4f;

    private GameObject currentStun;

    public void ShowStun(Transform target)
    {
        if (target == null || stunPrefab == null) return;

        if (currentStun != null)
            Destroy(currentStun);

        currentStun = Instantiate(
            stunPrefab,
            target.position + Vector3.up * yOffset,
            Quaternion.identity
        );

        currentStun.transform.SetParent(target, worldPositionStays: true);
    }

    public void HideStun()
    {
        if (currentStun != null)
            Destroy(currentStun);

        currentStun = null;
    }
}
