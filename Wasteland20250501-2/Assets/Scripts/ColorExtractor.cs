using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class ColorExtractor : MonoBehaviour
{
    [SerializeField] private Material[] availableColors; // 可用的颜色材质数组
    [SerializeField] private float holdTime = 1.0f; // 长按时间
    private int currentColorIndex = 0;
    private Renderer objectRenderer;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private BodySocketInventory bodySocketInventory;
    private Material currentMaterial;

    private void Start()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Rigidbody 组件，已自动添加");
            gameObject.AddComponent<Rigidbody>();
        }

        if (GetComponentInChildren<Collider>() == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Collider 组件，请添加适当的碰撞体");
        }

        objectRenderer = GetComponentInChildren<Renderer>();
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
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);
            Debug.Log($"{gameObject.name} 已设置完成，可以使用VR手柄进行交互：\n" +
                     "1. 使用 Grip 键（手柄侧面按钮）抓取物体并长按\n" +
                     "2. 长按达到指定时间后自动改变颜色");
        }

        if (availableColors == null || availableColors.Length == 0)
        {
            Debug.LogError($"{gameObject.name} 没有设置可用颜色材质！请在Inspector中设置 Available Colors");
        }

        bodySocketInventory = FindObjectOfType<BodySocketInventory>();
        if (bodySocketInventory == null)
        {
            Debug.LogError("BodySocketInventory not found in scene!");
        }
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log("Select enter event triggered");
        isHolding = true;
        holdTimer = 0f;
        StartCoroutine(HoldTimer());
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        Debug.Log("Select exit event triggered");
        isHolding = false;
        holdTimer = 0f;
    }

    private IEnumerator HoldTimer()
    {
        Debug.Log("Hold timer started");
        while (isHolding && holdTimer < holdTime)
        {
            holdTimer += Time.deltaTime;
            yield return null;
        }

        if (isHolding)
        {
            Debug.Log("Hold time reached, changing color");
            ChangeColor();
        }
    }

    public void ChangeColor()
    {
        if (availableColors != null && availableColors.Length > 0)
        {
            // 记录切换前的材质
            Material previousMaterial = objectRenderer.material;
            currentColorIndex = (currentColorIndex + 1) % availableColors.Length;
            objectRenderer.material = availableColors[currentColorIndex];
            Debug.Log($"{gameObject.name} 材质已更改为索引 {currentColorIndex}");
            
            // 将之前的材质添加到背包系统
            if (bodySocketInventory != null)
            {
                bodySocketInventory.AddColorOrb(previousMaterial);
                Debug.Log($"已将材质加入背包");
            }
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEnter);
            grabInteractable.selectExited.RemoveListener(OnSelectExit);
        }
    }
}
