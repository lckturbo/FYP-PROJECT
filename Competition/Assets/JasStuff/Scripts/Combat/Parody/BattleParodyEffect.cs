using System.Collections;
using UnityEngine;

public class BattleParodyEffect : MonoBehaviour
{
    private Camera cam;
    private Vector3 originalPos;
    private float originalSize;
    private Coroutine activeCoroutine;

    [Header("ZoomCrit Settings")]
    [SerializeField] private float zoomSize = 2.5f;
    [SerializeField] private float zoomDuration = 0.3f;
    //[SerializeField] private float holdDuration = 1f;

    private void Awake()
    {
        cam = Camera.main;
        if(cam!= null)
        {
            originalPos = cam.transform.position;
            originalSize = cam.orthographicSize;
        }
    }

    public void ZoomIn(Combatant attacker)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(ZoomInRoutine(attacker));
    }

    public void ZoomOut()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(ZoomOutRoutine());
    }
    private IEnumerator ZoomInRoutine(Combatant attacker)
    {
        Vector3 focusPoint = attacker.transform.position + new Vector3(0, 0f, -10f);

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(originalSize, zoomSize, elapsed / zoomDuration);
            cam.transform.position = Vector3.Lerp(originalPos, focusPoint, elapsed / zoomDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = zoomSize;
        cam.transform.position = focusPoint;
    }

    private IEnumerator ZoomOutRoutine()
    {
        Vector3 focusPoint = cam.transform.position;

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            cam.orthographicSize = Mathf.Lerp(zoomSize, originalSize, elapsed / zoomDuration);
            cam.transform.position = Vector3.Lerp(focusPoint, originalPos, elapsed / zoomDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = originalSize;
        cam.transform.position = originalPos;
    }

}
