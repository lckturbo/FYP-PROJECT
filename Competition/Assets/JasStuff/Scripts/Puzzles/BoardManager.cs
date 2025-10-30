using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public abstract class BoardManager : MonoBehaviour
{
    [SerializeField] protected ToggleableBlock doorToOpen;

    [Header("Door Focus")]
    [SerializeField] protected Transform doorFocusPoint;
    [SerializeField] protected float doorFocusDuration = 0.5f;
    protected NewCameraController cam;

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
        cam = FindObjectOfType<NewCameraController>();
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
    protected IEnumerator HandlePuzzleComplete()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = false;
        }

        yield return new WaitForSeconds(0.5f);

        if (cam && doorFocusPoint)
        {
            Debug.Log("[PathPuzzle] Panning camera to door...");
            yield return cam.StartCoroutine(cam.PanTo(doorFocusPoint, 0.8f));
        }

        if (doorToOpen != null)
        {
            doorToOpen.Open();
            Debug.Log("[PathPuzzle] Door opened!");
        }

        yield return new WaitForSeconds(doorFocusDuration);

        if (cam)
            yield return cam.StartCoroutine(cam.ReturnToPlayer(0.8f));

        Debug.Log("[PathPuzzle] Puzzle sequence complete.");

        if (player)
        {
            PlayerInput playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.enabled = true;
        }
    }
}
