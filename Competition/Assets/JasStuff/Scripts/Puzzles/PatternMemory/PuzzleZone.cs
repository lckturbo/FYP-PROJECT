using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleZone : MonoBehaviour
{
    [SerializeField] private int zoneID;
    [SerializeField] private Tilemap puzzleTileMap;
    private Color baseColor;

    private void Awake()
    {
        if (puzzleTileMap) baseColor = puzzleTileMap.color;
    }
    public void Highlight(float duration)
    {
        StartCoroutine(HighlightRoutine(duration));
    }
    private IEnumerator HighlightRoutine(float duration)
    {
        puzzleTileMap.color = Color.yellow;
        yield return new WaitForSeconds(duration);
        puzzleTileMap.color = baseColor;
    }
}