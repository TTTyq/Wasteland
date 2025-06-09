using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackpackManager : MonoBehaviour
{
    [SerializeField] private GameObject colorSlotPrefab; // 颜色槽预制体
    [SerializeField] private Transform backpackPanel; // 背包面板
    [SerializeField] private int maxSlots =7; // 最大槽位数
    [SerializeField] private float slotSpacing = 15f; // 槽位间距

    private List<Color> collectedColors = new List<Color>();
    private List<GameObject> colorSlots = new List<GameObject>();

    private void Start()
    {
        InitializeBackpack();
    }

    private void InitializeBackpack()
    {
        // 创建初始槽位
        for (int i = 0; i < maxSlots; i++)
        {
            CreateColorSlot();
        }
    }

    private void CreateColorSlot()
    {
        if (colorSlotPrefab != null && backpackPanel != null)
        {
            GameObject slot = Instantiate(colorSlotPrefab, backpackPanel);
            colorSlots.Add(slot);
            // 不再手动设置RectTransform位置，交给VerticalLayoutGroup自动布局
        }
    }

    public void AddColor(Color color)
    {
        if (collectedColors.Count < maxSlots)
        {
            collectedColors.Add(color);
            UpdateColorSlot(collectedColors.Count - 1, color);
        }
    }

    private void UpdateColorSlot(int index, Color color)
    {
        if (index >= 0 && index < colorSlots.Count)
        {
            Image slotImage = colorSlots[index].GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.color = color;
            }
        }
    }

    public Color GetColor(int index)
    {
        if (index >= 0 && index < collectedColors.Count)
        {
            return collectedColors[index];
        }
        return Color.white;
    }

    public void RemoveColor(int index)
    {
        if (index >= 0 && index < collectedColors.Count)
        {
            collectedColors.RemoveAt(index);
            UpdateBackpackDisplay();
        }
    }

    private void UpdateBackpackDisplay()
    {
        // 更新所有槽位的显示
        for (int i = 0; i < colorSlots.Count; i++)
        {
            if (i < collectedColors.Count)
            {
                UpdateColorSlot(i, collectedColors[i]);
            }
            else
            {
                UpdateColorSlot(i, Color.clear);
            }
        }
    }
} 