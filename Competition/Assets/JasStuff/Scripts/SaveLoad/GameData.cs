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

    public GameData()
    {
        selectedCharacterIndex = -1;
        playerPosition = Vector2.zero;
        hasSavedPosition = false;

        defeatedEnemies = new List<string>();

        bgmVolume = 1f;
        sfxVolume = 1f;

        hasCheckpoint = false;
        lastCheckpointID = 0;
    }
}
