using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class MirrorInteract : MonoBehaviour
{
    public Transform mirrorObject;
    public bool rotateClockwise = true;

    private bool inRange;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        if (!mirrorObject) mirrorObject = transform;
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.GetComponentInParent<NewPlayerMovement>())
            inRange = true;
    }

    private void OnTriggerExit2D(Collider2D c)
    {
        if (c.GetComponentInParent<NewPlayerMovement>())
            inRange = false;
    }

    private void Update()
    {
        if (!inRange || Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            float angle = rotateClockwise ? -90f : 90f;
            mirrorObject.Rotate(0f, 0f, angle);
        }
    }
}
