using System.Collections;
using System.Linq;
using UnityEngine;

public class BattleParodyEffect : MonoBehaviour
{
    public enum ParodyType
    {
        None,
        ZoomEffect
    }

    public ParodyType type;
    private Camera cam;
    private Vector3 originalPos;
    private float originalSize;
    private Coroutine activeCoroutine;

    [Header("ZoomCrit Settings")]
    [SerializeField] private float zoomSize = 2.5f;
    [SerializeField] private float zoomDuration = 0.3f;
    [SerializeField] private float holdDuration = 1f;

    private void Awake()
    {
        cam = Camera.main;
        if(cam!= null)
        {
            originalPos = cam.transform.position;
            originalSize = cam.orthographicSize;
        }
    }

    public void PlayParody(ParodyType type, Combatant attacker, Combatant target)
    {
        if (cam == null) return;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);

        switch (type)
        {
            case ParodyType.ZoomEffect:
                StartCoroutine(ZoomEffect(attacker, target));
                break;
        }
    }
    private IEnumerator ZoomEffect(Combatant attacker, Combatant target)
    {
        Vector3 focusPoint = attacker.transform.position + new Vector3(0, 1f, -10f);

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(originalSize, zoomSize, elapsed / zoomDuration);
            cam.transform.position = Vector3.Lerp(originalPos, focusPoint, elapsed / zoomDuration);

            float scaleFactor = Mathf.Lerp(1f, 1f / cam.orthographicSize, elapsed / zoomDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = zoomSize;
        cam.transform.position = focusPoint;

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(zoomSize, originalSize, elapsed / zoomDuration);
            cam.transform.position = Vector3.Lerp(focusPoint, originalPos, elapsed / zoomDuration);

            float scaleFactor = Mathf.Lerp(1f / zoomSize, 1f, elapsed / zoomDuration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = originalSize;
        cam.transform.position = originalPos;
    }
}
