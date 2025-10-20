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
        List<Vector3Int> moves = new List<Vector3Int>();

        Dictionary<Vector3Int, Vector3Int[]> connections = new Dictionary<Vector3Int, Vector3Int[]>
        {
             {
                // TOP LEFT //
                new(1,5,0),
                new[]
                {
                    new Vector3Int(0,3,0),
                    new Vector3Int(3,6,0),
                    new Vector3Int(3,3,0)
                }
            },
            {
                // TOP MIDDLE //
                new(3,6,0),
                new[]
                {
                    new Vector3Int(1,5,0),
                    new Vector3Int(5,5,0),
                    new Vector3Int(3,3,0),
                }
            },
            {
                // TOP RIGHT //
                new(5,5,0),
                new[]
                {
                    new Vector3Int(6,3,0),
                    new Vector3Int(3,6,0),
                    new Vector3Int(3,3,0)
                }
            },
            {
                // CENTER LEFT
                new(0,3,0),
                new[]
                {
                    new Vector3Int(1,1,0),
                    new Vector3Int(1,5,0),
                    new Vector3Int(3,3,0)
                }
            },
            {
                // CENTER // 
                new(3,3,0),
                new[]{
                    new Vector3Int(1,1,0),
                    new Vector3Int(3,0,0),
                    new Vector3Int(5,1,0),
                    new Vector3Int(0,3,0),
                    new Vector3Int(6,3,0),
                    new Vector3Int(1,5,0),
                    new Vector3Int(3,6,0),
                    new Vector3Int(5,5,0)
                }
            },
            { 
                // CENTER RIGHT //
                new(6,3,0),
                new[]
                {
                    new Vector3Int(5,5,0),
                    new Vector3Int(5,1,0),
                    new Vector3Int(3,3,0)
                }
            },
            {
                // BOTTOM LEFT //
                new(1,1,0),
                new[]
                {
                    new Vector3Int(0,3,0),
                    new Vector3Int(3,3,0),
                    new Vector3Int(3,0,0)
                }
            },
            {
                // BOTTOM MIDDLE //
                new(3,0,0),
                new[]
                {
                    new Vector3Int(5,1,0),
                    new Vector3Int(1,1,0),
                    new Vector3Int(3,3,0)
                }
            },
            {
                // BOTTOM RIGHT //
                new(5,1,0),
                new[]
                {
                    new Vector3Int(3,0,0),
                    new Vector3Int(3,3,0),
                    new Vector3Int(6,3,0)
                }
            }
        };

        if (connections.ContainsKey(currCell))
        {
            foreach (var dest in connections[currCell])
            {
                // Skip invalid tiles
                if (!boardTileMap.HasTile(dest)) continue;

                // Skip occupied cells
                bool occupied = false;
                foreach (var obj in manager.spawnedObjs)
                {
                    var s = obj.GetComponent<Statue>();
                    if (s && s.CurrentCell == dest)
                    {
                        occupied = true;
                        break;
                    }
                }

                if (!occupied) moves.Add(dest);
            }
        }

        return moves;
    }

    public override bool TryMoveTo(Vector3 worldPos)
    {
        if (manager == null || boardTileMap == null) return false;

        Vector3Int targetCell = boardTileMap.WorldToCell(worldPos);
        if (!currentValidMoves.Contains(targetCell)) return false;

        Vector3 world = boardTileMap.CellToWorld(targetCell) + new Vector3(0.5f, 0.5f, -0.1f);
        transform.position = world;

        currentCell = targetCell;
        manager.OnMove(gameObject, currentCell, targetCell);

        highlightTileMap.ClearAllTiles();
        isHighlighted = false;

        return true;
    }
}
