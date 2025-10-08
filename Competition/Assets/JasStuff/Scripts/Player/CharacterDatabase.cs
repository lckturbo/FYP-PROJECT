using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Game/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public NewCharacterDefinition[] roster;
    private Dictionary<string, NewCharacterDefinition> lookup;

    private void OnEnable()
    {
        lookup = new Dictionary<string, NewCharacterDefinition>();

        foreach(var def in roster)
        {
            if(!def && !string.IsNullOrEmpty(def.id))
            {
                if(!lookup.ContainsKey(def.id))
                    lookup.Add(def.id, def);
            }
        }
    }

    public NewCharacterDefinition GetByIndex(int index)
    {
        if (roster == null || roster.Length == 0) return null;
        if (index < 0 || index >= roster.Length) return null;
        return roster[index];
    }
}
