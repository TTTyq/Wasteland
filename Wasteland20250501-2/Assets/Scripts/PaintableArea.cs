using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PaintableArea : MonoBehaviour, IDropHandler
{
    public int areaIndex; // 该色块的索引
    private Image image;  // 如果是UI色块
    private Renderer meshRenderer; // 如果是3D色块

    private void Awake()
    {
        image = GetComponent<Image>();
        meshRenderer = GetComponent<Renderer>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggable = eventData.pointerDrag?.GetComponent<ColorSlotDraggable>();
        if (draggable != null)
        {
            Color color = draggable.SlotColor;
            if (image != null)
                image.color = color;
            if (meshRenderer != null)
                meshRenderer.material.color = color;

            // 通知判定管理器
            PaintPuzzleJudge.Instance.SetAreaColor(areaIndex, color);
        }
    }
} 