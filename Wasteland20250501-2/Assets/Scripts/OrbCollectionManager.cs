using UnityEngine;
using System.Collections.Generic;

public class OrbCollectionManager : MonoBehaviour
{
    [Header("收集设置")]
    [SerializeField] private int requiredOrbCount = 7;
    [SerializeField] private BodySocketInventory inventory;

    [Header("要激活的物体")]
    [SerializeField] private GameObject[] objectsToActivate;

    [Header("特效设置 (可选)")]
    [SerializeField] private GameObject activationEffect;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private float effectDuration = 2f;

    [Header("调试")]
    [SerializeField] private bool enableDebugLogs = true;

    // 状态追踪
    private bool hasActivated = false;
    private int lastOrbCount = 0;

    // 组件引用
    private AudioSource audioSource;

    void Start()
    {
        // 获取音频组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && activationSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 验证设置
        ValidateSetup();

        // 确保目标物体初始是隐藏的
        SetObjectsActive(false);

        LogDebug($"OrbCollectionManager initialized. Need {requiredOrbCount} orbs to activate {objectsToActivate.Length} objects.");
    }

    void Update()
    {
        // 检查小球数量变化
        CheckOrbCount();
    }

    private void CheckOrbCount()
    {
        if (hasActivated || inventory == null) return;

        int currentOrbCount = inventory.GetAvailableOrbCount();

        // 只在数量变化时输出日志，避免刷屏
        if (currentOrbCount != lastOrbCount)
        {
            LogDebug($"Orb count changed: {lastOrbCount} → {currentOrbCount}/{requiredOrbCount}");
            lastOrbCount = currentOrbCount;

            // 检查是否达到要求
            if (currentOrbCount >= requiredOrbCount)
            {
                ActivateObjects();
            }
        }
    }

    private void ActivateObjects()
    {
        if (hasActivated) return;

        hasActivated = true;
        LogDebug($"🎉 ACTIVATION TRIGGERED! {requiredOrbCount} orbs collected!");

        // 播放音效
        PlayActivationSound();

        // 播放特效
        PlayActivationEffect();

        // 激活物体
        SetObjectsActive(true);

        // 输出激活的物体信息
        LogActivatedObjects();
    }

    private void SetObjectsActive(bool active)
    {
        if (objectsToActivate == null) return;

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
            {
                obj.SetActive(active);
                LogDebug($"Set {obj.name} active: {active}");
            }
        }
    }

    private void PlayActivationSound()
    {
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
            LogDebug("🔊 Played activation sound");
        }
    }

    private void PlayActivationEffect()
    {
        if (activationEffect != null)
        {
            // 实例化特效
            GameObject effect = Instantiate(activationEffect, transform.position, transform.rotation);

            // 自动销毁特效
            if (effectDuration > 0)
            {
                Destroy(effect, effectDuration);
            }

            LogDebug($"✨ Played activation effect at {transform.position}");
        }
    }

    private void LogActivatedObjects()
    {
        LogDebug("📋 Activated objects:");
        for (int i = 0; i < objectsToActivate.Length; i++)
        {
            if (objectsToActivate[i] != null)
            {
                LogDebug($"  {i + 1}. {objectsToActivate[i].name}");
            }
        }
    }

    private void ValidateSetup()
    {
        // 检查BodySocketInventory
        if (inventory == null)
        {
            inventory = FindObjectOfType<BodySocketInventory>();
            if (inventory == null)
            {
                Debug.LogError("❌ OrbCollectionManager: No BodySocketInventory found! Please assign it in the inspector.");
            }
            else
            {
                LogDebug("✅ Auto-found BodySocketInventory");
            }
        }

        // 检查要激活的物体
        if (objectsToActivate == null || objectsToActivate.Length == 0)
        {
            Debug.LogWarning("⚠️ OrbCollectionManager: No objects to activate assigned!");
        }
        else
        {
            int validObjects = 0;
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null) validObjects++;
            }
            LogDebug($"✅ Found {validObjects}/{objectsToActivate.Length} valid objects to activate");
        }
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[OrbCollectionManager] {message}");
        }
    }

    // 公共方法：手动检查状态
    [ContextMenu("Check Current Status")]
    public void CheckCurrentStatus()
    {
        if (inventory != null)
        {
            int currentCount = inventory.GetAvailableOrbCount();
            Debug.Log($"=== Orb Collection Status ===");
            Debug.Log($"Current orbs: {currentCount}/{requiredOrbCount}");
            Debug.Log($"Has activated: {hasActivated}");
            Debug.Log($"Objects to activate: {(objectsToActivate != null ? objectsToActivate.Length : 0)}");
        }
        else
        {
            Debug.LogError("No inventory reference!");
        }
    }

    // 公共方法：强制激活（测试用）
    [ContextMenu("Force Activate (Test)")]
    public void ForceActivate()
    {
        Debug.Log("🧪 Force activating objects for testing...");
        ActivateObjects();
    }

    // 公共方法：重置状态
    [ContextMenu("Reset State")]
    public void ResetState()
    {
        hasActivated = false;
        lastOrbCount = 0;
        SetObjectsActive(false);
        Debug.Log("🔄 OrbCollectionManager state reset");
    }

    // 公共方法：设置需要的小球数量
    public void SetRequiredOrbCount(int count)
    {
        requiredOrbCount = count;
        LogDebug($"Required orb count set to: {count}");
    }

    // 公共方法：添加要激活的物体
    public void AddObjectToActivate(GameObject obj)
    {
        if (obj == null) return;

        // 扩展数组
        System.Array.Resize(ref objectsToActivate, objectsToActivate.Length + 1);
        objectsToActivate[objectsToActivate.Length - 1] = obj;

        LogDebug($"Added object to activate: {obj.name}");
    }

    // 属性：获取当前状态
    public bool HasActivated => hasActivated;
    public int CurrentOrbCount => inventory != null ? inventory.GetAvailableOrbCount() : 0;
    public int RequiredOrbCount => requiredOrbCount;
}