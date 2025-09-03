using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    public CharacterDefinition[] characters;  // size 3
    public int currentIndex = 0;

    PlayerMovement movement;
    Health health;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<Health>();
        ApplyCurrent();
    }

    void Update()
    {
        // hotkeys for testing
        if (Input.GetKeyDown(KeyCode.Alpha1)) { currentIndex = 0; ApplyCurrent(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { currentIndex = 1; ApplyCurrent(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { currentIndex = 2; ApplyCurrent(); }
    }

    public void ApplyCurrent()
    {
        if (characters == null || characters.Length == 0) return;
        var def = characters[Mathf.Clamp(currentIndex, 0, characters.Length - 1)];
        if (def == null) return;

        if (movement && def.movement) movement.ApplyMovementConfig(def.movement);
        if (health && def.stats) health.ApplyStats(def.stats);

        // TODO: swap sprite/animator/controller per character if needed
        // e.g., GetComponentInChildren<SpriteRenderer>().sprite = def.portrait;
    }
}
