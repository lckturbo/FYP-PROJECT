using System.Collections.Generic;
using UnityEngine;
using static GameData;

[System.Serializable]
public class GameData
{
    [System.Serializable]
    public class ChannelState
    {
        public int channelIndex;
        public bool isActivated;
    }
    [System.Serializable]
    public class ChestSaveEntry
    {
        public string id;
        public bool opened;
    }
    [System.Serializable]
    public class BreakableSaveEntry
    {
        public string id;
        public bool destroyed;
    }
    [System.Serializable]
    public class NPCQuestStageEntry
    {
        public string npcName;
        public int stageIndex;
    }
    [System.Serializable]
    public class BlockSaveData
    {
        public int id;
        public Vector2 position;

        public BlockSaveData(int id, Vector2 position)
        {
            this.id = id;
            this.position = position;
        }
    }
    [System.Serializable]
    public class StatueSaveData
    {
        public bool isCompleted;
        public Vector3Int position;
        public bool isWhite;
    }
    [System.Serializable]
    public class ChessPieceSaveData
    {
        public Piece.PieceType type;
        public bool isWhite;
        public Vector3Int position;

        public ChessPieceSaveData(Piece.PieceType type, bool isWhite, Vector3Int position)
        {
            this.type = type;
            this.isWhite = isWhite;
            this.position = position;
        }
    }
    [System.Serializable]
    public class BlockSaveEntry
    {
        public string id;      
        public bool isOpen;    
    }

    public bool hasSavedGame;

    // camera //
    public Vector3 cameraPosition;
    public bool cameraIsPanning;
    public bool cameraPositionSaved;

    // player //
    public int selectedCharacterIndex;
    public List<int> allyIndices;
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

    public List<ChannelState> switchChannelStates; // switches
    public List<ChestSaveEntry> openedChests; // chest
    public List<BreakableSaveEntry> brokenObjects; // breakable
    public List<NPCQuestStageEntry> npcQuestStages; // quests
    public List<BlockSaveData> pushableBlocks; // push-able blocks 
    public bool pathPuzzleCompleted;
    public List<StatueSaveData> statueStates;
    public bool connectPuzzleCompleted;
    public List<ChessPieceSaveData> chessPieces;
    public bool chessPuzzleCompleted;
    public List<BlockSaveEntry> savedBlocks;

    public GameData()
    {
        hasSavedGame = false;

        cameraPosition = new Vector3(0, 0, 0);
        cameraIsPanning = false;
        cameraPositionSaved = false;
        // player //
        selectedCharacterIndex = -1;
        allyIndices = new List<int>();
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

        switchChannelStates = new List<ChannelState>(); // switches
        openedChests = new List<ChestSaveEntry>(); // chests
        brokenObjects = new List<BreakableSaveEntry>(); // breakables
        npcQuestStages = new List<NPCQuestStageEntry>(); // quests
        pushableBlocks = new List<BlockSaveData>(); // push-able blocks
        pathPuzzleCompleted = false;
        statueStates = new List<StatueSaveData>();
        connectPuzzleCompleted = false;
        chessPieces = new List<ChessPieceSaveData>();
        chessPuzzleCompleted = false;
        savedBlocks = new List<BlockSaveEntry>();
    }
}
