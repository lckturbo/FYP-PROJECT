using UnityEngine;

[CreateAssetMenu(menuName = "NewCharacter/Definition", fileName = "NewCharacterDefinition")]
public class NewCharacterDefinition : ScriptableObject
{
    [Header("General")]
    public string id;
    public string displayName;
    public NewCharacterStats runtimeStats;

    [TextArea] public string description;

    [Header("Art")]
    public Sprite normalArt;
    public Sprite pixelArt;
    public Sprite portrait;
    public Sprite portrait2;

    [Header("Gameplay")]
    public NewCharacterStats stats;
    //public UnitType unitType = UnitType.Melee;

    [Header("Prefab")]
    public GameObject playerPrefab;

    [Header("Battle UIs")]
    public Sprite basicAtk;
    public Sprite skill1;
    public Sprite skill2;
}

//if we planning to add

public enum UnitType { Melee, Ranged }