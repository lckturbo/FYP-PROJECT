using UnityEngine;

[CreateAssetMenu(fileName = "SelectedCharacter", menuName = "Game/Selected Character")]
public class SelectedCharacter : ScriptableObject
{
    [SerializeField] private CharacterDatabase database;

    public NewCharacterDefinition definition;
    public int index = -1;

    public void Set(NewCharacterDefinition def, int idx = -1)
    {
        definition = def;
        index = idx;
    }

    public void RestoreFromIndex(int idx)
    {
        if (!database) return;

        var def = database.GetByIndex(idx);
        if (def)
        {
            definition = def;
            index = idx;
        }
    }
}
