using UnityEngine;
using System.Collections.Generic;

public class ColorPuzzleManager : MonoBehaviour
{
    [System.Serializable]
    public class ColorPuzzleObject
    {
        public GameObject puzzleObject;
        public int correctColorIndex;
    }

    [SerializeField] private List<ColorPuzzleObject> puzzleObjects = new List<ColorPuzzleObject>();
    [SerializeField] private GameObject doorObject;
    [SerializeField] private float doorOpenSpeed = 2f;
    [SerializeField] private float doorOpenHeight = 3f;

    private bool isPuzzleSolved = false;
    private Vector3 doorInitialPosition;
    private Vector3 doorTargetPosition;

    private void Start()
    {
        if (doorObject != null)
        {
            doorInitialPosition = doorObject.transform.position;
            doorTargetPosition = doorInitialPosition + Vector3.up * doorOpenHeight;
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

    private void Update()
    {
        if (isPuzzleSolved && doorObject != null)
        {
            doorObject.transform.position = Vector3.Lerp(
                doorObject.transform.position,
                doorTargetPosition,
                Time.deltaTime * doorOpenSpeed
            );
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
                    // 检查当前颜色索引是否匹配正确颜色
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
        }
    }
} 