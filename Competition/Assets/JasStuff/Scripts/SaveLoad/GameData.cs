using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // player //
    public int selectedCharacterIndex;
    public Vector2 playerPosition;
    public bool hasSavedPosition;

    // enemies //
    public List<string> defeatedEnemies;
    
    public GameData()
    {
        selectedCharacterIndex = -1;
        playerPosition = Vector2.zero;
        hasSavedPosition = false;
        defeatedEnemies = new List<string>();
    }
}
