using UnityEngine;

[CreateAssetMenu(fileName = "SelectedCharacter", menuName = "Game/Selected Character")]
public class SelectedCharacter : ScriptableObject
{
    public NewCharacterDefinition definition;
    public int index = -1;

    public void Set(NewCharacterDefinition def, int idx = -1)
    {
        definition = def;
        index = idx;
    }
}
