using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PieceController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction interactAction;

    private Piece nearbyPiece;
    private Piece selectedPiece;
    private Tilemap boardTileMap;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        boardTileMap = FindObjectsOfType<Tilemap>().FirstOrDefault(s => s.gameObject.name == "Tilemap - ChessBoard");
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
        if (selectedPiece != null)
        {
            Vector3Int cell = boardTileMap.WorldToCell(transform.position);

            if (selectedPiece.TryMoveTo(boardTileMap.GetCellCenterWorld(cell)))
            {
                selectedPiece = null;
                return;
            }
        }

        if (nearbyPiece != null)
        {
            if (selectedPiece != null && selectedPiece != nearbyPiece)
                selectedPiece.ToggleHighlight();

            selectedPiece = nearbyPiece;

            selectedPiece.ToggleHighlight();
            return;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Piece piece))
            nearbyPiece = piece;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Piece piece) && piece == nearbyPiece)
            nearbyPiece = null;
    }
}
