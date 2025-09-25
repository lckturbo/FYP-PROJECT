using UnityEngine;

[System.Serializable]
public class GameData
{
    public int selectedCharacterIndex;
    public Vector3 playerPosition;
    
    public GameData()
    {
        selectedCharacterIndex = -1;
        playerPosition = Vector3.zero;
    }
}
