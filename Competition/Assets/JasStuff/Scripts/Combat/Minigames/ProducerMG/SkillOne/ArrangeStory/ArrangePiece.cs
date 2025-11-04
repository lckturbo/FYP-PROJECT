using UnityEngine;
using UnityEngine.EventSystems;

public class ArrangePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Unique ID for this piece, matches a slot's correctPieceID")]
    public int pieceID;

    [HideInInspector] public ArrangeSlot currentSlot;

    private Canvas canvas;
    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = rect.anchoredPosition;
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;

        if (currentSlot != null)
        {
            currentSlot.currentPiece = null;
            currentSlot = null;
        }

        transform.SetParent(canvas.transform, true); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );
        rect.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (currentSlot == null)
        {
            transform.SetParent(originalParent, false);
            ResetPosition();
        }
    }

    public void ResetPosition()
    {
        rect.anchoredPosition = originalPosition;
    }
}
