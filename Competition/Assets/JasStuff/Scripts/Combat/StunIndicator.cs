using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunIndicator : MonoBehaviour
{
    [SerializeField] private GameObject stunPrefab;
    [SerializeField] private float yOffset = 1.4f;

    private Dictionary<Transform, GameObject> stunIcons = new Dictionary<Transform, GameObject>();

    public void ShowStun(Transform target)
    {
        if (target == null || stunPrefab == null) return;

        if (stunIcons.TryGetValue(target, out GameObject icon) && icon != null)
        {
            icon.transform.position = target.position + Vector3.up * yOffset;
            return;
        }

        GameObject newIcon = Instantiate(
            stunPrefab,
            target.position + Vector3.up * yOffset,
            Quaternion.identity
        );

        newIcon.transform.SetParent(target, worldPositionStays: true);

        stunIcons[target] = newIcon;
    }


    public void HideStun(Transform target)
    {
        if (target == null) return;

        if (stunIcons.TryGetValue(target, out GameObject icon) && icon != null) 
        {
            Destroy(icon);
        }

        stunIcons.Remove(target);
    }

    public void HideAll()
    { 
        foreach (var kvp in stunIcons)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value);
        }

        stunIcons.Clear();
    }
}
