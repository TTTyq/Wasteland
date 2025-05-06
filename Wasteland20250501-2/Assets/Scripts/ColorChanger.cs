using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ColorChanger : MonoBehaviour
{
    [SerializeField] private Material[] availableColors; // 可用的颜色材质数组
    private int currentColorIndex = 0;
    private Renderer objectRenderer;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    private void Start()
    {
        // 确保物体有必要的组件
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Rigidbody 组件，已自动添加");
            gameObject.AddComponent<Rigidbody>();
        }

        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Collider 组件，请添加适当的碰撞体");
        }

        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError($"{gameObject.name} 缺少 Renderer 组件！");
            return;
        }

        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 XRGrabInteractable 组件，已自动添加");
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }
        
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnActivated);
            Debug.Log($"{gameObject.name} 已设置完成，可以使用VR手柄进行交互：\n" +
                     "1. 使用 Grip 键（手柄侧面按钮）抓取物体\n" +
                     "2. 使用 Trigger 键（扳机键）改变颜色");
        }

        // 检查材质设置
        if (availableColors == null || availableColors.Length == 0)
        {
            Debug.LogError($"{gameObject.name} 没有设置可用颜色材质！请在Inspector中设置 Available Colors");
        }
    }

    private void OnActivated(ActivateEventArgs args)
    {
        ChangeColor();
    }

    public void ChangeColor()
    {
        if (availableColors != null && availableColors.Length > 0)
        {
            currentColorIndex = (currentColorIndex + 1) % availableColors.Length;
            objectRenderer.material = availableColors[currentColorIndex];
            Debug.Log($"{gameObject.name} 颜色已更改为索引 {currentColorIndex}");
            
            // 通知谜题管理器检查解决方案
            var puzzleManager = FindObjectOfType<ColorPuzzleManager>();
            if (puzzleManager != null)
            {
                puzzleManager.CheckPuzzleSolution();
            }
        }
    }

    public int GetCurrentColorIndex()
    {
        return currentColorIndex;
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnActivated);
        }
    }
} 