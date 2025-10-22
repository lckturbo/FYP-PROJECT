using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleZone : MonoBehaviour
{
    [SerializeField] private int zoneID;
    public int ZoneID => zoneID;
    [SerializeField] private Tilemap puzzleTileMap;

    [Header("Tiles")]
    [SerializeField] private TileBase normalTile;
    [SerializeField] private TileBase highlightTile;

    private TileBase[,] originalTiles;

    private void Awake()
    {
        if (puzzleTileMap)
        {
            CacheOriginalTiles();
        }
    }

    private void CacheOriginalTiles()
    {
        BoundsInt bounds = puzzleTileMap.cellBounds;
        originalTiles = new TileBase[bounds.size.x, bounds.size.y];
        int x = 0, y = 0;
        foreach (var pos in bounds.allPositionsWithin)
        {
            originalTiles[x, y] = puzzleTileMap.GetTile(pos);
            y++;
            if (y >= bounds.size.y)
            {
                y = 0;
                x++;
            }
        }
    }

    public void Highlight(float duration)
    {
        StartCoroutine(HighlightRoutine(duration));
    }

    private IEnumerator HighlightRoutine(float duration)
    {
        if (!puzzleTileMap) yield break;

        foreach (var pos in puzzleTileMap.cellBounds.allPositionsWithin)
        {
            if (puzzleTileMap.HasTile(pos))
                puzzleTileMap.SetTile(pos, highlightTile);
        }

        yield return new WaitForSeconds(duration);

        foreach (var pos in puzzleTileMap.cellBounds.allPositionsWithin)
        {
            if (puzzleTileMap.HasTile(pos))
                puzzleTileMap.SetTile(pos, normalTile);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            FindObjectOfType<PathPuzzle>().OnZoneStepped(this);
    }
}