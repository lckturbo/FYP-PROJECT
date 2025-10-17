using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class BoardManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Tilemap boardTileMap;
    public List<GameObject> spawnedObjs = new();

    [Header("Highlight Tile")]
    [SerializeField] protected Tilemap highlightTileMap;
    [SerializeField] protected TileBase highlightTile;

    protected bool puzzleSolved = false;

    protected virtual void Start()
    {
        SetupPuzzle();
    }
    protected abstract void SetupPuzzle();
    protected GameObject SpawnOnBoard(GameObject prefab, Vector3Int cellPos)
    {
        if (!prefab || !boardTileMap) return null;

        Vector3 worldPos = boardTileMap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
        GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        spawnedObjs.Add(obj);
        return obj;
    }
    public virtual void ResetBoard()
    {
        ClearBoard();
        SetupPuzzle();
    }
    protected void ClearBoard()
    {
        foreach (var obj in spawnedObjs)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjs.Clear();
    }

    public abstract void OnMove(GameObject moved, Vector3Int fromCell, Vector3Int toCell);
    protected abstract void LockAllPieces();
}
