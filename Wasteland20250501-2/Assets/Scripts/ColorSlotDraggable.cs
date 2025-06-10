using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorSlotDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Color SlotColor { get; private set; }
    private Image image;
    private Transform originalParent;
    private Canvas canvas;

    public void SetColor(Color color)
    {
        SlotColor = color;
        if (image == null) image = GetComponent<Image>();
        image.color = color;
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(canvas.transform, true);
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent, true);
        image.raycastTarget = true;
    }
} 