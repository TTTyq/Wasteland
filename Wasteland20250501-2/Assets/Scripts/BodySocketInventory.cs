// ===============================================
// 4. BodySocketInventory.cs - XRI 3.1.2优化版本 (增加左右角度)
// ===============================================
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

public class BodySocketInventory : MonoBehaviour
{
    [System.Serializable]
    public class SocketPoint
    {
        public string socketName;
        public Transform socketTransform;
        public bool isOccupied;
        public GameObject orbEffect;
        public Material storedMaterial;
    }

    [Header("Player Camera Reference")]
    public Transform playerCamera;

    [Header("Orb Settings")]
    public GameObject orbPrefab;
    public int orbCount = 7;
    public float orbSpacing = 0.2f;
    public float orbYOffset = -0.2f;
    public float orbForwardOffset = 1.0f;
    public float orbLeftRightOffset = -0.5f;
    public float orbRotateSpeed = 50f;

    [Header("Interaction Settings")]
    public LayerMask colorableLayerMask = -1;

    private List<SocketPoint> socketPoints = new List<SocketPoint>();

    private void Start()
    {
        CreateCameraSocketPoints();
        foreach (var socket in socketPoints)
        {
            socket.isOccupied = false;
            CreateOrbEffect(socket);
        }
    }

    private void CreateCameraSocketPoints()
    {
        for (int i = 0; i < orbCount; i++)
        {
            GameObject socketObj = new GameObject($"CameraSocket_{i}");
            socketObj.transform.parent = playerCamera;
            socketObj.transform.localPosition = Vector3.zero;
            socketObj.transform.localRotation = Quaternion.identity;

            SocketPoint socket = new SocketPoint
            {
                socketName = $"CameraSocket_{i}",
                socketTransform = socketObj.transform,
                isOccupied = false,
                orbEffect = null
            };

            socketPoints.Add(socket);
        }
    }

    private void Update()
    {
        UpdateOrbPositions();
    }

    private void UpdateOrbPositions()
    {
        if (playerCamera == null) return;

        // 修改这里的偏移值来调整球的整体位置
        float adjustedYOffset = -0.2f;
        float adjustedForwardOffset = 0.9f;

        Vector3 center = playerCamera.position +
                        playerCamera.forward * adjustedForwardOffset +
                        playerCamera.up * adjustedYOffset;

        float radius = orbSpacing;
        float baseAngle = Time.time * orbRotateSpeed;

        for (int i = 0; i < socketPoints.Count; i++)
        {
            if (socketPoints[i].orbEffect != null && socketPoints[i].orbEffect.activeSelf)
            {
                float angle = baseAngle + i * (360f / socketPoints.Count);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 offset = playerCamera.right * Mathf.Cos(rad) * radius +
                               playerCamera.forward * Mathf.Sin(rad) * radius;
                Vector3 orbPos = center + offset;
                socketPoints[i].orbEffect.transform.position = orbPos;
                socketPoints[i].orbEffect.transform.rotation = Quaternion.LookRotation(playerCamera.forward, playerCamera.up);
            }
        }
    }



    private void CreateOrbEffect(SocketPoint socket)
    {
        if (orbPrefab != null)
        {
            socket.orbEffect = Instantiate(orbPrefab, socket.socketTransform.position, Quaternion.identity);
            socket.orbEffect.transform.parent = socket.socketTransform;
            socket.orbEffect.SetActive(false);

            if (socket.orbEffect.GetComponent<Renderer>() == null)
            {
                Debug.LogError("Orb prefab must have a Renderer component!");
            }
        }
        else
        {
            Debug.LogError("Orb prefab is not assigned in BodySocketInventory!");
        }
    }

    public void AddColorOrb(Material material)
    {
        SocketPoint availableSocket = socketPoints.Find(socket => !socket.isOccupied);

        if (availableSocket != null)
        {
            availableSocket.storedMaterial = material;
            availableSocket.isOccupied = true;

            if (availableSocket.orbEffect != null)
            {
                availableSocket.orbEffect.SetActive(true);

                Renderer orbRenderer = availableSocket.orbEffect.GetComponent<Renderer>();
                if (orbRenderer != null)
                {
                    orbRenderer.material = material;
                }

                OrbEffect orbEffect = availableSocket.orbEffect.GetComponent<OrbEffect>();
                if (orbEffect != null)
                {
                    orbEffect.SetMaterial(material);
                }

                SetupOrbForGrabbing(availableSocket.orbEffect, availableSocket);

                Debug.Log($"Added material {material.name} to orb at socket {availableSocket.socketName}");
            }
        }
        else
        {
            Debug.LogWarning("No available socket found for new color orb!");
        }
    }

    public void RemoveColorOrb(Material material)
    {
        SocketPoint socket = socketPoints.Find(s => s.isOccupied && s.storedMaterial == material);
        if (socket != null)
        {
            socket.isOccupied = false;
            socket.storedMaterial = null;
            if (socket.orbEffect != null)
            {
                socket.orbEffect.SetActive(false);
            }
        }
    }

    public void DestroyUsedOrb(Material material)
    {
        SocketPoint socket = socketPoints.Find(s => s.isOccupied && s.storedMaterial == material);
        if (socket != null)
        {
            socket.isOccupied = false;
            socket.storedMaterial = null;
            if (socket.orbEffect != null)
            {
                Destroy(socket.orbEffect);
                socket.orbEffect = null;
            }
            Debug.Log($"Destroyed used orb with material {material.name}");
        }
    }

    // XRI 3.1.2优化的设置方法
    private void SetupOrbForGrabbing(GameObject orbObject, SocketPoint socket)
    {
        if (orbObject == null)
        {
            Debug.LogError("SetupOrbForGrabbing: orbObject is null!");
            return;
        }

        // XRI 3.1.2中查找XRInteractionManager的方式
        XRInteractionManager interactionManager = FindObjectOfType<XRInteractionManager>();
        if (interactionManager == null)
        {
            Debug.LogError("找不到XR Interaction Manager!");
            return;
        }

        // 设置GrabbableColorOrb组件
        GrabbableColorOrb grabbableOrb = orbObject.GetComponent<GrabbableColorOrb>();
        if (grabbableOrb == null)
        {
            grabbableOrb = orbObject.AddComponent<GrabbableColorOrb>();
        }

        int orbIndex = socketPoints.IndexOf(socket);
        grabbableOrb.orbIndex = orbIndex;
        grabbableOrb.orbMaterial = socket.storedMaterial;
        grabbableOrb.inventory = this;
        grabbableOrb.colorableLayerMask = colorableLayerMask;

        // 设置XR Grab Interactable - XRI 3.1.2版本
        XRGrabInteractable grabInteractable = orbObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = orbObject.AddComponent<XRGrabInteractable>();
        }

        // 关键：设置XRI 3.1.2的新属性
        grabInteractable.interactionManager = interactionManager;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.smoothPosition = true;
        grabInteractable.smoothRotation = true;

        // XRI 3.1.2 新的默认值
        grabInteractable.smoothPositionAmount = 8f; // 从5改为8
        grabInteractable.smoothRotationAmount = 8f; // 从5改为8
        grabInteractable.tightenPosition = 0.1f;    // 从0.5改为0.1
        grabInteractable.tightenRotation = 0.1f;    // 从0.5改为0.1
        grabInteractable.attachEaseInTime = 0.15f;  // XRI 3.1.2要求的最小值

        // 确保有正确的碰撞体 - 这是最关键的修复
        Collider col = orbObject.GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = orbObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.15f; // 稍微大一点便于抓取
            sphereCol.isTrigger = false; // 关键：必须是false才能被抓取
            Debug.Log($"Added collider to orb {orbIndex}");
        }
        else
        {
            col.isTrigger = false; // 确保现有碰撞体不是trigger
        }

        // 确保有刚体
        Rigidbody rb = orbObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = orbObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        // 确保在正确的图层上
        orbObject.layer = LayerMask.NameToLayer("Default");

        Debug.Log($"Successfully setup orb {orbIndex} for XRI 3.1.2 interaction");

        // 验证设置
        ValidateOrbSetup(orbObject, orbIndex);
    }

    private void ValidateOrbSetup(GameObject orbObject, int orbIndex)
    {
        bool hasGrabbable = orbObject.GetComponent<GrabbableColorOrb>() != null;
        bool hasXRGrab = orbObject.GetComponent<XRGrabInteractable>() != null;
        Collider col = orbObject.GetComponent<Collider>();
        bool hasCollider = col != null;
        bool hasRigidbody = orbObject.GetComponent<Rigidbody>() != null;
        bool correctColliderSettings = hasCollider && !col.isTrigger;

        if (hasGrabbable && hasXRGrab && hasCollider && hasRigidbody && correctColliderSettings)
        {
            Debug.Log($"✓ Orb {orbIndex} setup validation passed (XRI 3.1.2)");
        }
        else
        {
            Debug.LogError($"✗ Orb {orbIndex} setup validation failed: " +
                          $"Grabbable={hasGrabbable}, XRGrab={hasXRGrab}, " +
                          $"Collider={hasCollider}, Rigidbody={hasRigidbody}, " +
                          $"CorrectCollider={correctColliderSettings}");
        }
    }

    // 获取相关信息的方法
    public GameObject GetOrbByIndex(int index)
    {
        if (index >= 0 && index < socketPoints.Count && socketPoints[index].isOccupied)
        {
            return socketPoints[index].orbEffect;
        }
        return null;
    }

    public Material GetOrbMaterial(int index)
    {
        if (index >= 0 && index < socketPoints.Count && socketPoints[index].isOccupied)
        {
            return socketPoints[index].storedMaterial;
        }
        return null;
    }

    public bool IsOrbAvailable(int index)
    {
        return index >= 0 && index < socketPoints.Count && socketPoints[index].isOccupied;
    }

    public int GetAvailableOrbCount()
    {
        int count = 0;
        foreach (var socket in socketPoints)
        {
            if (socket.isOccupied && socket.storedMaterial != null)
            {
                count++;
            }
        }
        return count;
    }

    [ContextMenu("验证所有Orb设置")]
    public void ValidateAllOrbs()
    {
        Debug.Log("=== 开始验证Orb设置 ===");

        int validOrbs = 0;
        int totalOrbs = 0;

        for (int i = 0; i < socketPoints.Count; i++)
        {
            var socket = socketPoints[i];
            if (socket.isOccupied && socket.orbEffect != null)
            {
                totalOrbs++;

                GrabbableColorOrb grabbable = socket.orbEffect.GetComponent<GrabbableColorOrb>();
                XRGrabInteractable grabInteractable = socket.orbEffect.GetComponent<XRGrabInteractable>();
                Collider col = socket.orbEffect.GetComponent<Collider>();

                bool isValid = true;
                if (grabbable == null)
                {
                    Debug.LogError($"Orb {i}: 缺少 GrabbableColorOrb 组件");
                    isValid = false;
                }
                if (grabInteractable == null)
                {
                    Debug.LogError($"Orb {i}: 缺少 XRGrabInteractable 组件");
                    isValid = false;
                }
                if (col == null)
                {
                    Debug.LogError($"Orb {i}: 缺少 Collider 组件");
                    isValid = false;
                }
                else if (col.isTrigger)
                {
                    Debug.LogError($"Orb {i}: Collider不应该是Trigger！");
                    isValid = false;
                }

                if (isValid)
                {
                    validOrbs++;
                    Debug.Log($"Orb {i}: 设置正确 ✓");
                }
            }
        }

        Debug.Log($"=== 验证完成: {validOrbs}/{totalOrbs} 个orb设置正确 ===");
    }

    [ContextMenu("验证XRI 3.1.2兼容性")]
    public void ValidateXRI312Compatibility()
    {
        Debug.Log("=== 验证XRI 3.1.2兼容性 ===");

        // 检查XRInteractionManager
        XRInteractionManager manager = FindObjectOfType<XRInteractionManager>();
        if (manager != null)
        {
            Debug.Log($"✓ 找到XRInteractionManager: {manager.name}");
        }
        else
        {
            Debug.LogError("✗ 未找到XRInteractionManager - 请确保场景中有XR Origin!");
        }

        // 验证所有orb设置
        ValidateAllOrbs();
    }
}