using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleZone : MonoBehaviour
{
    [SerializeField] private int zoneID;
    public int ZoneID => zoneID;
    [SerializeField] private Tilemap puzzleTileMap;

    [Header("Tiles")]
    [SerializeField] private TileBase highlightTile;
    [SerializeField] private TileBase correctTile;
    [SerializeField] private TileBase wrongTile;

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

        BoundsInt bounds = puzzleTileMap.cellBounds;
        int x = 0, y = 0;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (puzzleTileMap.HasTile(pos))
                puzzleTileMap.SetTile(pos, highlightTile);
        }

        yield return new WaitForSeconds(duration);

        x = 0; y = 0;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (originalTiles[x, y] != null)
                puzzleTileMap.SetTile(pos, originalTiles[x, y]);

            y++;
            if (y >= bounds.size.y)
            {
                y = 0;
                x++;
            }
        }
    }
    public void FlashFeedback(bool correct)
    {
        StartCoroutine(FlashFeedbackRoutine(correct));
    }

    private IEnumerator FlashFeedbackRoutine(bool correct)
    {
        if (!puzzleTileMap) yield break;

        BoundsInt bounds = puzzleTileMap.cellBounds;
        TileBase feedbackTile = correct ? correctTile : wrongTile;

        foreach (var pos in bounds.allPositionsWithin)
        {
            if (puzzleTileMap.HasTile(pos))
                puzzleTileMap.SetTile(pos, feedbackTile);
        }

        yield return new WaitForSeconds(1.0f);

        int x = 0, y = 0;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (originalTiles[x, y] != null)
                puzzleTileMap.SetTile(pos, originalTiles[x, y]);

            y++;
            if (y >= bounds.size.y)
            {
                y = 0;
                x++;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            FindObjectOfType<PathPuzzle>().OnZoneStepped(this);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            FindObjectOfType<PathPuzzle>().ClearLastZone();
    }
}