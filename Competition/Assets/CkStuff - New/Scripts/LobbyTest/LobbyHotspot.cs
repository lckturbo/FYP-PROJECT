using UnityEngine;

public class LobbyHotspot : MonoBehaviour
{
    [SerializeField] private LobbyZoomController controller;
    [SerializeField] private float zoom = 2.2f; // tweak per hotspot

    public void OnClick()
    {
        controller.FocusOn((RectTransform)transform, zoom);
    }
}
