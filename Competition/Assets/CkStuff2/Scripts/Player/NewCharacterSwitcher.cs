using UnityEngine;
using UnityEngine.InputSystem;

public class NewCharacterSwitcher : MonoBehaviour
{
    [Tooltip("Assign 3 NewCharacterDefinition assets here")]
    public NewCharacterDefinition[] characters;  // size 3
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
        // Hotkeys via the new Input System
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) { currentIndex = 0; ApplyCurrent(); }
        if (kb.digit2Key.wasPressedThisFrame) { currentIndex = 1; ApplyCurrent(); }
        if (kb.digit3Key.wasPressedThisFrame) { currentIndex = 2; ApplyCurrent(); }
    }

    public void ApplyCurrent()
    {
        if (characters == null || characters.Length == 0) return;

        currentIndex = Mathf.Clamp(currentIndex, 0, characters.Length - 1);
        var def = characters[currentIndex];
        if (def == null) return;

        if (movement != null && def.movement != null)
            movement.ApplyMovementConfig(def.movement);

        if (health != null && def.stats != null)
            health.ApplyStats(def.stats);

        // TODO: swap sprite/animator/controller per character if needed
        // e.g., GetComponentInChildren<SpriteRenderer>().sprite = def.portrait;
    }
}
