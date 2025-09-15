using UnityEngine;

[CreateAssetMenu(menuName = "NewCharacter/Definition", fileName = "NewCharacterDefinition")]
public class NewCharacterDefinition : ScriptableObject
{
    [Header("General")]
    public string displayName;
    //public string className; //if we doing and have

    [TextArea] public string description;

    [Header("Art")]
    public Sprite normalArt;
    public Sprite pixelArt;
    public Sprite portrait;

    [Header("Gameplay")]
    public NewCharacterStats stats;
    //public UnitType unitType = UnitType.Melee;

    [Header("Prefab")]
    public GameObject playerPrefab;
}

//if we planning to add

public enum UnitType { Melee, Ranged }