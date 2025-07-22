// ===============================================
// 3. ColorExtractor.cs - 保持原有修复
// ===============================================
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;


public class ColorExtractor : MonoBehaviour
{
    [SerializeField] private Material[] availableColors;
    [SerializeField] private float holdTime = 1.0f;
    [SerializeField] private BodySocketInventory bodySocketInventory;

    private int currentColorIndex = 0;
    private Renderer objectRenderer;
    private XRGrabInteractable grabInteractable;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool isExtracting = false;

    // 添加冷却时间防止重复触发
    private float extractCooldown = 0.5f;
    private float lastExtractTime = 0f;

    // 确保只有一个实例在处理
    private static bool isAnyExtractorActive = false;

    void Start()
    {
        objectRenderer = GetComponentInChildren<Renderer>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            // 设置XRI 3.1.2要求的最小值
            grabInteractable.attachEaseInTime = 0.15f;

            // 清除现有监听器防止重复
            grabInteractable.selectEntered.RemoveAllListeners();
            grabInteractable.selectExited.RemoveAllListeners();

            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);

            Debug.Log($"ColorExtractor initialized on {gameObject.name}");
        }
        else
        {
            Debug.LogError($"ColorExtractor on {gameObject.name}: No XRGrabInteractable found!");
        }
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        // 全局锁检查
        if (isAnyExtractorActive)
        {
            Debug.Log("Another extractor is active, ignoring");
            return;
        }

        // 冷却检查
        if (Time.time - lastExtractTime < extractCooldown)
        {
            Debug.Log("ColorExtractor: Still in cooldown, ignoring extract attempt");
            return;
        }

        if (isExtracting)
        {
            Debug.Log("ColorExtractor: Already extracting, ignoring");
            return;
        }

        Debug.Log($"ColorExtractor: Starting extraction on {gameObject.name}");
        isExtracting = true;
        isAnyExtractorActive = true;
        isHolding = true;
        holdTimer = 0f;
        StartCoroutine(HoldTimer());
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        Debug.Log($"ColorExtractor: Stopping extraction on {gameObject.name}");
        isHolding = false;
        isExtracting = false;
        isAnyExtractorActive = false;
    }

    private IEnumerator HoldTimer()
    {
        while (isHolding && holdTimer < holdTime)
        {
            holdTimer += Time.deltaTime;
            yield return null;
        }

        if (isHolding)
        {
            ChangeColor();
        }

        // 重置状态
        isExtracting = false;
        isAnyExtractorActive = false;
    }

    public void ChangeColor()
    {
        if (availableColors != null && availableColors.Length > 0 && bodySocketInventory != null)
        {
            Material previousMaterial = objectRenderer.material;
            currentColorIndex = (currentColorIndex + 1) % availableColors.Length;
            objectRenderer.material = availableColors[currentColorIndex];

            // 更新最后提取时间
            lastExtractTime = Time.time;

            bodySocketInventory.AddColorOrb(previousMaterial);
            Debug.Log($"ColorExtractor: Successfully extracted color from {gameObject.name}");
        }
        else
        {
            Debug.LogError("ColorExtractor: Missing required components for color change");
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEnter);
            grabInteractable.selectExited.RemoveListener(OnSelectExit);
        }

        // 清理全局状态
        if (isExtracting)
        {
            isAnyExtractorActive = false;
        }
    }
}