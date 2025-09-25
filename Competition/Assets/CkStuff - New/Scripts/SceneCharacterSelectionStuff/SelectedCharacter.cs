using UnityEngine;

[CreateAssetMenu(fileName = "SelectedCharacter", menuName = "Game/Selected Character")]
public class SelectedCharacter : ScriptableObject, IDataPersistence
{
    public NewCharacterDefinition definition;
    public int index = -1;

    public void Set(NewCharacterDefinition def, int idx = -1)
    {
        definition = def;
        index = idx;
    }

    public void LoadData(GameData data)
    {
        index = data.selectedCharacterIndex;
    }

    public void SaveData(ref GameData data)
    {
        data.selectedCharacterIndex = index;
    }
}
