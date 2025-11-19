using System.Collections;
using UnityEngine;

public class BattleParodyEffect : MonoBehaviour
{
    private Camera cam;
    private Vector3 originalPos;
    private float originalSize;
    private Coroutine activeCoroutine;

    private Vector3 shakeOrigin;

    [Header("ZoomCrit Settings")]
    [SerializeField] private float zoomSize = 2.5f;
    [SerializeField] private float zoomDuration = 0.3f;

    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.15f;
    [SerializeField] private float shakeFrequency = 25f;

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
    public void ShakeScreen()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        shakeOrigin = cam.transform.position;

        activeCoroutine = StartCoroutine(ScreenShakeRoutine());
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
    private IEnumerator ScreenShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * shakeMagnitude;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f * shakeMagnitude;

            cam.transform.position = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = originalPos;
    }

}
