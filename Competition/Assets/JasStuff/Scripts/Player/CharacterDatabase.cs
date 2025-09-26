using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Game/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public NewCharacterDefinition[] roster;

    public NewCharacterDefinition GetByIndex(int index)
    {
        if (roster == null || roster.Length == 0) return null;
        if (index < 0 || index >= roster.Length) return null;
        return roster[index];
    }
}
