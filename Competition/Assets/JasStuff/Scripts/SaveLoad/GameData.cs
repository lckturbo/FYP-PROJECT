using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    [System.Serializable]
    public class ChannelState
    {
        public int channelIndex;
        public bool isActivated;
    }

    // camera //
    public Vector3 cameraPosition;
    public bool cameraIsPanning;
    public bool cameraPositionSaved;

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
    public List<ChannelState> switchChannelStates;  

    public GameData()
    {
        cameraPosition = new Vector3(0, 0, 0);
        cameraIsPanning = false;
        cameraPositionSaved = false;
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
        switchChannelStates = new List<ChannelState>();
    }
}
