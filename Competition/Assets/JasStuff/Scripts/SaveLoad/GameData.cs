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

    [System.Serializable]
    public class MirrorSaveEntry
    {
        public string id;
        public float rotation;
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

    // player level //
    public int playerLevel;
    public int playerCurrentXP;

    // enemies //
    public List<string> defeatedEnemies;

    // audio //
    public float masterVolume;
    public float bgmVolume;
    public float sfxVolume;

    // battle settings //
    public bool autoBattleUnlocked;
    public bool battleSpeedUnlocked;

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
    public int chessPuzzleSolutionIndex; // save which solution is active
    public int chessPuzzleCurrentStep; // save current progress in solution
    public List<BlockSaveEntry> savedBlocks;

    public List<InventoryItemData> mainInventoryData;
    public List<InventoryItemData> subInventoryData;
    public int money;

    // beams
    public List<MirrorSaveEntry> mirrors;
    public bool beamReceiverSolved;

    [System.Serializable]
    public class InventoryItemData
    {
        public string itemName;
        public int quantity;

        public InventoryItemData(string itemName, int quantity)
        {
            this.itemName = itemName;
            this.quantity = quantity;
        }
    }

    public bool hasSpawnedStartingItem;

    [System.Serializable]
    public class PuzzleSaveEntry
    {
        public string puzzleID;
        public bool solved;
    }

    public List<PuzzleSaveEntry> savedPuzzles = new List<PuzzleSaveEntry>();
    public List<string> clearedFogIds;

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

        // player level //
        playerLevel = 1;
        playerCurrentXP = 0;

        // enemies //
        defeatedEnemies = new List<string>();

        // audio //
        masterVolume = 1f;
        bgmVolume = 1f;
        sfxVolume = 1f;

        // battle settings //
        autoBattleUnlocked = false;
        battleSpeedUnlocked = false;

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
        chessPuzzleSolutionIndex = -1; 
        chessPuzzleCurrentStep = 0; 
        savedBlocks = new List<BlockSaveEntry>();
        mirrors = new List<MirrorSaveEntry>();
        beamReceiverSolved = false;

        // existing initializations...
        mainInventoryData = new List<InventoryItemData>();
        subInventoryData = new List<InventoryItemData>();
        money = 0;

        savedPuzzles = new List<PuzzleSaveEntry>();
        hasSpawnedStartingItem = false;
        clearedFogIds = new List<string>();
    }
}