using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float yOffset = 1.4f;

    private GameObject currentArrow;
    private Coroutine hideRoutine;

    public void ShowArrow(Transform target)
    {
        if (target == null || arrowPrefab == null) return;

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (currentArrow != null) Destroy(currentArrow);

        currentArrow = Instantiate(
            arrowPrefab,
            target.position + Vector3.up * yOffset,
            Quaternion.identity
        );

        currentArrow.transform.SetParent(target, worldPositionStays: true);
    }

    public void HideArrow(float delay = 0f)
    {
        if (currentArrow == null) return;

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }
        hideRoutine = StartCoroutine(DelayedHide(delay));
    }

    private System.Collections.IEnumerator DelayedHide(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (currentArrow != null)
            Destroy(currentArrow);

        currentArrow = null;
        hideRoutine = null;
    }
}
