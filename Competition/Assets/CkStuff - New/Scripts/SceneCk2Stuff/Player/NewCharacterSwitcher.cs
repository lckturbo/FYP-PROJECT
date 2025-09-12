using UnityEngine;
using UnityEngine.InputSystem;

public class NewCharacterSwitcher : MonoBehaviour
{
    public NewCharacterDefinition[] characters;
    public int currentIndex = 0;

    private NewPlayerMovement movement;
    private NewHealth health;

    void Awake()
    {
        movement = GetComponent<NewPlayerMovement>();
        health = GetComponent<NewHealth>();
        ApplyCurrent();
    }

    void Update()
    {
        // Optional hotkeys (Input System)
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) SetIndex(0);
        if (kb.digit2Key.wasPressedThisFrame) SetIndex(1);
        if (kb.digit3Key.wasPressedThisFrame) SetIndex(2);

        // Optional cycle
        if (kb.qKey.wasPressedThisFrame) Prev();
        if (kb.eKey.wasPressedThisFrame) Next();
    }

    public void Next() => SetIndex(currentIndex + 1);
    public void Prev() => SetIndex(currentIndex - 1);

    public void SetIndex(int idx)
    {
        if (characters == null || characters.Length == 0) return;
        currentIndex = WrapIndex(idx, characters.Length);
        ApplyCurrent();
    }

    public void ApplyCurrent()
    {
        if (characters == null || characters.Length == 0) return;

        int clamped = Mathf.Clamp(currentIndex, 0, characters.Length - 1);
        var def = characters[clamped];
        if (def == null)
        {
            Debug.LogWarning("[NewCharacterSwitcher] CharacterDefinition is null at index " + clamped);
            return;
        }

        // Apply the SAME stats to movement (reads walkSpeed) and health (reads HP/defense/elements)
        if (def.stats == null)
        {
            Debug.LogWarning("[NewCharacterSwitcher] Missing stats on definition at index " + clamped);
            return;
        }

        if (movement != null) movement.ApplyStats(def.stats);
        if (health != null) health.ApplyStats(def.stats);

        // TODO: swap visuals if your NewCharacterDefinition has portrait/animator/prefab refs
    }

    private int WrapIndex(int i, int len)
    {
        if (len <= 0) return 0;
        int m = i % len;
        return m < 0 ? m + len : m;
    }
}
