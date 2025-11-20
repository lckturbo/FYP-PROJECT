using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class MirrorInteract : MonoBehaviour, IDataPersistence
{
    [Header("Save / Load")]
    public string mirrorID;

    [Header("Mirror Logic Pivot")]
    public Transform mirrorObject;

    [Header("Art")]
    public Transform artRoot;
    public SpriteRenderer mirrorRenderer;

    public Sprite spriteAt0;
    public Sprite spriteAt90;
    public Sprite spriteAt180;
    public Sprite spriteAt270;

    [Header("Behaviour")]
    public bool rotateClockwise = true;

    private bool inRange;

    public float GetRotation() => mirrorObject ? mirrorObject.localEulerAngles.z : 0f;

    public void SetRotation(float angle)
    {
        if (!mirrorObject) return;

        float snapped = Mathf.Round(angle / 90f) * 90f;
        mirrorObject.localEulerAngles = new Vector3(0f, 0f, snapped);

        if (artRoot)
            artRoot.localEulerAngles = new Vector3(0f, 0f, -snapped);

        UpdateSpriteFromRotation(snapped);
    }

    private void Awake()
    {
        if (!mirrorRenderer && artRoot)
            mirrorRenderer = artRoot.GetComponentInChildren<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<NewPlayerMovement>())
            inRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<NewPlayerMovement>())
            inRange = false;
    }

    private void Update()
    {
        if (GameInputLock.inputLocked || Keyboard.current == null)
            return;

        if (!inRange || !mirrorObject)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            RotateOnce();
        }
    }

    private void RotateOnce()
    {
        AudioManager.instance.PlaySFXAtPoint("old-chair-moving-210885", transform.position);


        float delta = rotateClockwise ? -90f : 90f;

        Vector3 euler = mirrorObject.localEulerAngles;
        float newZ = Mathf.Round((euler.z + delta) / 90f) * 90f;

        mirrorObject.localEulerAngles = new Vector3(0f, 0f, newZ);

        if (artRoot)
            artRoot.localEulerAngles = new Vector3(0f, 0f, -newZ);

        UpdateSpriteFromRotation(newZ);
    }

    private void UpdateSpriteFromRotation(float angle)
    {
        if (!mirrorRenderer)
            return;

        float z = Mathf.Repeat(angle, 360f);
        int step = Mathf.RoundToInt(z / 90f) % 4;

        Sprite chosen = null;
        switch (step)
        {
            case 0: chosen = spriteAt0; break;
            case 1: chosen = spriteAt90; break;
            case 2: chosen = spriteAt180; break;
            case 3: chosen = spriteAt270; break;
        }

        if (chosen != null)
            mirrorRenderer.sprite = chosen;
    }

    public void LoadData(GameData data)
    {
        var entry = data.mirrors.Find(x => x.id == mirrorID);
        if (entry != null)
        {
            SetRotation(entry.rotation);
        }
        else
        {
            SetRotation(GetRotation());
        }
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
