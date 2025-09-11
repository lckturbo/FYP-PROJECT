using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Selection Data")]
    public CharacterDefinition selectedCharacter;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetSelectedCharacter(CharacterDefinition def)
    {
        selectedCharacter = def;
        Debug.Log("[GameManager] Character selected: " + def.name);
    }
}
