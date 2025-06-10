using UnityEngine;
using SatProductions;

public class PaintPuzzleJudge : MonoBehaviour
{
    public static PaintPuzzleJudge Instance { get; private set; }

    [System.Serializable]
    public class AreaTarget
    {
        public int areaIndex;
        public Color targetColor;
    }

    public AreaTarget[] targets;
    private Color[] currentColors;

    [SerializeField] private EasyDoor doorController;

    private bool isPuzzleSolved = false;

    private void Awake()
    {
        Instance = this;
        currentColors = new Color[targets.Length];
    }

    public void SetAreaColor(int index, Color color)
    {
        if (index >= 0 && index < currentColors.Length)
        {
            currentColors[index] = color;
            CheckSolution();
        }
    }

    private void CheckSolution()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (currentColors[i] != targets[i].targetColor)
                return;
        }
        // 全部正确
        if (!isPuzzleSolved)
        {
            isPuzzleSolved = true;
            Debug.Log("Puzzle Solved! Door is opening...");
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