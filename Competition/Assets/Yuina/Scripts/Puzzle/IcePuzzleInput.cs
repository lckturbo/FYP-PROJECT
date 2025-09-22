using UnityEngine;
using UnityEngine.InputSystem;

public class IcePuzzleInput : MonoBehaviour
{
    [SerializeField] private IcePuzzle puzzle;
    [SerializeField] private ArrowManager arrowManager;

    void Update()
    {
        if (puzzle == null) return;
        if (!puzzle.IsAcceptingInput()) return;

        // Key input (arrow keys or WASD)
        if (Keyboard.current.wKey != null && Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey != null && Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            puzzle.QueueMove(Vector2Int.up);
            arrowManager?.AddArrow(Vector2Int.up); // Display arrow
        }
        else if (Keyboard.current.sKey != null && Keyboard.current.sKey.wasPressedThisFrame ||
                 Keyboard.current.downArrowKey != null && Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            puzzle.QueueMove(Vector2Int.down);
            arrowManager?.AddArrow(Vector2Int.down);
        }
        else if (Keyboard.current.aKey != null && Keyboard.current.aKey.wasPressedThisFrame ||
                 Keyboard.current.leftArrowKey != null && Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            puzzle.QueueMove(Vector2Int.left);
            arrowManager?.AddArrow(Vector2Int.left);
        }
        else if (Keyboard.current.dKey != null && Keyboard.current.dKey.wasPressedThisFrame ||
                 Keyboard.current.rightArrowKey != null && Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            puzzle.QueueMove(Vector2Int.right);
            arrowManager?.AddArrow(Vector2Int.right);
        }

    }
}
