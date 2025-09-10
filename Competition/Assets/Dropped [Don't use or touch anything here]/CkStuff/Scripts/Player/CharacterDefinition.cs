using UnityEngine;

[CreateAssetMenu(menuName = "Character/Definition", fileName = "CharacterDefinition")]
public class CharacterDefinition : ScriptableObject
{
    public string displayName = "New Character";
    public Sprite portrait;
    public MovementConfig movement;
    public CharacterStats stats;
}
