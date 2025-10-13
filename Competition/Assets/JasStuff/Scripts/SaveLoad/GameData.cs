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
    public int lastCheckpointID;
    public bool hasCheckpoint;

    public GameData()
    {
        selectedCharacterIndex = -1;
        playerPosition = Vector2.zero;
        hasSavedPosition = false;

        defeatedEnemies = new List<string>();

        bgmVolume = 1f;
        sfxVolume = 1f;

        lastCheckpointID = -1;
        hasCheckpoint = false;
    }
}
