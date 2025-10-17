using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class BoardBase : MonoBehaviour
{
    [Header("Board References")]
    [SerializeField] protected Tilemap boardTileMap;

    [Header("Board Settings")]
    [SerializeField] protected int boardWidth;
    [SerializeField] protected int boardHeight;

    protected virtual void Start() => GenerateBoard();
    protected abstract void GenerateBoard();
    public virtual Vector3 GetWorldCenter(Vector3Int cellPos)
    {
        return boardTileMap.GetCellCenterWorld(cellPos);
    }
    public virtual bool IsInsideBoard(Vector3Int cellPos)
    {
        return cellPos.x >= 0 && cellPos.x < boardWidth && cellPos.y >= 0 && cellPos.y < boardHeight;
    }

}
