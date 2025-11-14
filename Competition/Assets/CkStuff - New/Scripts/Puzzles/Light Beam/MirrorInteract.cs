using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class MirrorInteract : MonoBehaviour, IDataPersistence
{
    // save load
    public string mirrorID;
    public float GetRotation() => mirrorObject.localEulerAngles.z;
    public void SetRotation(float angle) => mirrorObject.localEulerAngles = new Vector3(0, 0, angle);


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
        if (GameInputLock.inputLocked)
            return;

        if (!inRange || Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            float angle = rotateClockwise ? -90f : 90f;
            mirrorObject.Rotate(0f, 0f, angle);
        }
    }

    public void LoadData(GameData data)
    {
        var entry = data.mirrors.Find(x => x.id == mirrorID);
        if (entry != null)
            SetRotation(entry.rotation);
    }

    public void SaveData(ref GameData data)
    {
        var existing = data.mirrors.Find(x => x.id == mirrorID);

        if (existing != null)
            existing.rotation = GetRotation();
        else
            data.mirrors.Add(new GameData.MirrorSaveEntry
            {
                id = mirrorID,
                rotation = GetRotation()
            });
    }
}
