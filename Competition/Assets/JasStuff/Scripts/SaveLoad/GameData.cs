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

    // audio //
    public float bgmVolume;
    public float sfxVolume;

    // checkpoints //
    public bool hasCheckpoint;
    public int lastCheckpointID;

    // switches //
    public Dictionary<int, bool> switchStates;

    public GameData()
    {
        // player //
        selectedCharacterIndex = -1;
        playerPosition = Vector2.zero;
        hasSavedPosition = false;
        
        // enemies //
        defeatedEnemies = new List<string>();

        // audio //
        bgmVolume = 1f;
        sfxVolume = 1f;

        // checkpoints //
        hasCheckpoint = false;
        lastCheckpointID = 0;

        // switches //
        switchStates = new Dictionary<int, bool>();
    }
}
