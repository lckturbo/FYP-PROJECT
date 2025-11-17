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
    public Sprite icon;

    [Header("Battle Portraits")]
    public Sprite battlePortrait;

    [Header("UI Settings")]
    public Color uiColor = Color.white;

    [Header("Gameplay")]
    public NewCharacterStats stats;

    [Header("Prefab")]
    public GameObject playerPrefab;

    [Header("Battle UIs")]
    public Sprite basicAtk;
    public Sprite skill1;
    public Sprite skill2;

    [Header("Skill Info")]
    public string basicSkillName = "Basic Attack";
    [TextArea] public string basicDescription;

    public string skill1Name = "Skill 1";
    [TextArea] public string skill1Description;

    public string skill2Name = "Skill 2";
    [TextArea] public string skill2Description;
}

public enum UnitType { Melee, Ranged }
