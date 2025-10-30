using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class StatueController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction interactAction;

    private Statue nearbyStatue;
    private Statue selectedStatue;
    private Tilemap boardTileMap;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        boardTileMap = FindObjectsOfType<Tilemap>().FirstOrDefault(s => s.gameObject.name == "Tilemap - Connect3 Puzzles");
        if (playerInput)
        {
            interactAction = playerInput.actions["Interaction"];
            interactAction.performed += OnInteract;
        }
    }

    private void OnDestroy()
    {
        interactAction.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        var statueManager = FindObjectOfType<StatueManager>();
        if (statueManager != null && !statueManager.IsWhiteTurn())
            return;

        if (selectedStatue != null)
        {
            Vector3Int cell = boardTileMap.WorldToCell(transform.position);

            if (selectedStatue.TryMoveTo(boardTileMap.GetCellCenterWorld(cell)))
            {
                selectedStatue = null;
                return;
            }
        }

        if (nearbyStatue != null)
        {
            if (selectedStatue != null && selectedStatue != nearbyStatue)
                selectedStatue.ToggleHighlight();

            selectedStatue = nearbyStatue;

            selectedStatue.ToggleHighlight();
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Statue piece))
            nearbyStatue = piece;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Statue piece) && piece == nearbyStatue)
            nearbyStatue = null;
    }
}
