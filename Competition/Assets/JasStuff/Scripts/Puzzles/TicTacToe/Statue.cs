using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Statue : BoardEntity
{
    private StatueManager manager;

    public void Init(Tilemap board, Tilemap highlight, TileBase tile, StatueManager mgr)
    {
        base.Init(board, highlight, tile);
        manager = mgr;
    }

    public override List<Vector3Int> GetValidMoves(Vector3Int currCell)
    {
        //    List<Vector3Int> moves = new List<Vector3Int>();

        //    // Define connections
        //    Dictionary<Vector3Int, Vector3Int[]> connections = new Dictionary<Vector3Int, Vector3Int[]>
        //{
        //    { new(1,5,0), new[]{ new Vector3Int(3,3,0) } }, // Top-left
        //    { new(3,5,0), new[]{ new Vector3Int(3,3,0) } }, // Top-middle
        //    { new(5,5,0), new[]{ new Vector3Int(3,3,0), new Vector3Int(5,3,0) } }, // Top-right
        //    { new(1,3,0), new[]{ new Vector3Int(3,3,0) } }, // Mid-left
        //    { new(3,3,0), new[]{
        //        new Vector3Int(1,5,0), new Vector3Int(3,5,0), new Vector3Int(5,5,0),
        //        new Vector3Int(1,3,0), new Vector3Int(5,3,0),
        //        new Vector3Int(1,1,0), new Vector3Int(3,1,0), new Vector3Int(5,1,0)
        //    } }, // Center
        //    {
        //            new(5,3,0),
        //            new[]{
        //                new Vector3Int(3,3,0),
        //                new Vector3Int(5,5,0),
        //                new Vector3Int(5,1,0)
        //            }
        //        }, // Mid-right
        //    {
        //            new(1,1,0),
        //            new[]{
        //                new Vector3Int(3,3,0)
        //            }
        //        }, // Bottom-left
        //    {
        //            new(3,1,0),
        //            new[]{
        //                new Vector3Int(3,3,0)
        //            }
        //        }, // Bottom-middle
        //    { new(5,1,0),
        //            new[]{
        //                new Vector3Int(3,3,0),
        //                new Vector3Int(5,3,0)
        //            }
        //        }, // Bottom-right
        //};

        //    if (connections.ContainsKey(currCell))
        //    {
        //        foreach (var dest in connections[currCell])
        //        {
        //            // Skip invalid tiles
        //            if (!boardTileMap.HasTile(dest)) continue;

        //            // Skip occupied cells
        //            bool occupied = false;
        //            foreach (var obj in manager.spawnedObjs)
        //            {
        //                var s = obj.GetComponent<Statue>();
        //                if (s && s.CurrentCell == dest)
        //                {
        //                    occupied = true;
        //                    break;
        //                }
        //            }

        //            if (!occupied)
        //                moves.Add(dest);
        //        }
        //    }

        //    return moves;

        return null;
    }

    public override bool TryMoveTo(Vector3 worldPos)
    {
        if (manager == null || boardTileMap == null) return false;

        Vector3Int targetCell = boardTileMap.WorldToCell(worldPos);
        if (!currentValidMoves.Contains(targetCell)) return false;

        Vector3 world = boardTileMap.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, -0.1f);
        transform.position = world;

        manager.OnMove(gameObject, currentCell, targetCell);
        currentCell = targetCell;

        highlightTileMap.ClearAllTiles();
        isHighlighted = false;

        return true;
    }
}
