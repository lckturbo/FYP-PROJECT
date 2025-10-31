using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class NewCameraController : MonoBehaviour, IDataPersistence
{
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    [Range(0.02f, 0.5f)] public float smoothTime = 0.15f;

    private Vector3 _vel;
    private Coroutine _activePan;
    private bool _isPanning;
    private Transform _panTarget;
    private Transform _originalTarget;
    private Camera _cam;

    private void Awake()
    {
        _originalTarget = target;
        _cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        Transform followTarget = _isPanning && _panTarget ? _panTarget : target;
        if (!followTarget) return;

        Vector3 desired = followTarget.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);
    }

    public IEnumerator PanTo(Transform newTarget, float duration)
    {
        if (!newTarget) yield break;
        _isPanning = true;
        _panTarget = newTarget;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    public IEnumerator ReturnToPlayer(float duration)
    {
        if (!_originalTarget) _originalTarget = target;
        _isPanning = false;
        _panTarget = null;
        target = _originalTarget;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        if (_originalTarget)
            target = _originalTarget;
    }

    public IEnumerator ZoomTo(float targetSize, float duration)
    {
        if (!_cam) yield break;

        float startSize = _cam.orthographicSize;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            _cam.orthographicSize = Mathf.Lerp(startSize, targetSize, progress);
            yield return null;
        }

        _cam.orthographicSize = targetSize;
    }

    public void LoadData(GameData data)
    {
        if (data.cameraPositionSaved)
        {
            transform.position = data.cameraPosition;
            _isPanning = data.cameraIsPanning;
        }
    }

    public void SaveData(ref GameData data)
    {
        data.cameraPosition = transform.position;
        data.cameraIsPanning = _isPanning;
        data.cameraPositionSaved = true;
    }
}
