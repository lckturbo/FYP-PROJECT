using UnityEngine;

[CreateAssetMenu(menuName = "NewCharacter/Definition", fileName = "NewCharacterDefinition")]
public class NewCharacterDefinition : ScriptableObject
{
    public string displayName = "New Character";
    public Sprite portrait;
    public NewMovementConfig movement;
    public NewCharacterStats stats;
}
