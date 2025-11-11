using UnityEngine;
using UnityEngine.EventSystems;

public class ArrangeSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("The correct piece that fits this silhouette")]
    public int correctPieceID;

    [HideInInspector] public ArrangePiece currentPiece;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        ArrangePiece piece = eventData.pointerDrag.GetComponent<ArrangePiece>();
        if (piece == null) return;

        if (currentPiece != null)
            currentPiece.ResetPosition();

        currentPiece = piece;
        piece.currentSlot = this;

        piece.transform.SetParent(transform, worldPositionStays: false);

        var rect = piece.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    public bool IsCorrect()
    {
        return currentPiece != null && currentPiece.pieceID == correctPieceID;
    }
}
