using UnityEngine;

[DisallowMultipleComponent]
public class MinimapFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float height = -10f; // z offset for minimap

    private void Start()
    {
        // Try to auto-assign the player if not set manually
        if (player == null)
        {
            // Prefer the main camera's current target
            var mainCam = FindObjectOfType<NewCameraController>();
            if (mainCam != null && mainCam.target != null)
            {
                player = mainCam.target;
            }
            else
            {
                // Fallback: find the first object tagged "Player"
                var found = GameObject.FindGameObjectWithTag("Player");
                if (found != null)
                    player = found.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 newPos = player.position;
        newPos.z = height;
        transform.position = newPos;
    }
}
