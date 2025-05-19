using UnityEngine;
using System.Collections.Generic;
using SatProductions; // 添加命名空间引用

public class ColorPuzzleManager : MonoBehaviour
{
    [System.Serializable]
    public class ColorPuzzleObject
    {
        public GameObject puzzleObject;
        public int correctColorIndex;
    }

    [SerializeField] private List<ColorPuzzleObject> puzzleObjects = new List<ColorPuzzleObject>();
    [SerializeField] private EasyDoor doorController; // 使用 EasyDoor 组件

    private bool isPuzzleSolved = false;

    private void Start()
    {
        // 检查并获取 EasyDoor 组件
        if (doorController == null)
        {
            doorController = GetComponentInChildren<EasyDoor>();
            if (doorController == null)
            {
                Debug.LogError("未找到 EasyDoor 组件！请确保门对象上有 EasyDoor 组件。");
            }
        }

        // 为每个谜题物体添加颜色改变组件
        foreach (var puzzleObject in puzzleObjects)
        {
            if (puzzleObject.puzzleObject != null)
            {
                var colorChanger = puzzleObject.puzzleObject.GetComponent<ColorChanger>();
                if (colorChanger == null)
                {
                    colorChanger = puzzleObject.puzzleObject.AddComponent<ColorChanger>();
                }
            }
        }
    }

    public void CheckPuzzleSolution()
    {
        bool allCorrect = true;
        foreach (var puzzleObject in puzzleObjects)
        {
            if (puzzleObject.puzzleObject != null)
            {
                var colorChanger = puzzleObject.puzzleObject.GetComponent<ColorChanger>();
                if (colorChanger != null)
                {
                    if (colorChanger.GetCurrentColorIndex() != puzzleObject.correctColorIndex)
                    {
                        allCorrect = false;
                        break;
                    }
                }
            }
        }

        if (allCorrect && !isPuzzleSolved)
        {
            isPuzzleSolved = true;
            Debug.Log("Puzzle Solved! Door is opening...");
            
            // 使用 EasyDoor 打开门
            if (doorController != null)
            {
                doorController.OpenDoor();
            }
            else
            {
                Debug.LogError("无法打开门：未找到 EasyDoor 组件！");
            }
        }
    }
} 