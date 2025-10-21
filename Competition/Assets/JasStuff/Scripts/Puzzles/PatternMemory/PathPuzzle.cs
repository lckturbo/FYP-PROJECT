using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathPuzzle : MonoBehaviour
{
    [SerializeField] private Tilemap puzzleTileMap;
    [SerializeField] private TileBase normalTile;
    [SerializeField] private TileBase litTile;

    [SerializeField] private float showDuration = 1.0f;
    [SerializeField] private float delayBtwTiles = 0.3f;

    [SerializeField] private List<Vector3Int> correctPath;

    private int currStep = 0;
    private bool canMove = false;

    private void Start()
    {
        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
        canMove = false;

        foreach (var cell in correctPath)
        {
            puzzleTileMap.SetTile(cell, litTile);
            yield return new WaitForSeconds(showDuration);
            puzzleTileMap.SetTile(cell, normalTile);
            yield return new WaitForSeconds(delayBtwTiles);
        }

        currStep = 0;
        canMove = true;
    }
}
