using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class BodySocketInventory : MonoBehaviour
{
    [System.Serializable]
    public class SocketPoint
    {
        public string socketName;
        public Transform socketTransform;
        public bool isOccupied;
        public GrabbableItem currentItem;
        public GameObject orbEffect;
        public Material storedMaterial;
    }

    [Header("Player Camera Reference")]
    public Transform playerCamera; // 拖拽主摄像机
    [Header("Orb Settings")]
    public GameObject orbPrefab;
    public int orbCount = 5; // orb数量
    public float orbSpacing = 0.2f; // orb之间的间距
    public float orbYOffset = -0.5f; // 距离视野下方的偏移
    public float orbForwardOffset = 1.0f; // 距离相机前方的距离
    public float orbRotateSpeed = 50f; // 小球旋转速度

    private List<SocketPoint> socketPoints = new List<SocketPoint>();

    private void Start()
    {
        CreateCameraSocketPoints();
        foreach (var socket in socketPoints)
        {
            socket.isOccupied = false;
            socket.currentItem = null;
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
                currentItem = null,
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
        Vector3 center = playerCamera.position + playerCamera.forward * orbForwardOffset + playerCamera.up * orbYOffset;
        float radius = orbSpacing; // 用orbSpacing作为半径
        float baseAngle = Time.time * orbRotateSpeed; // 旋转速度可调

        for (int i = 0; i < socketPoints.Count; i++)
        {
            if (socketPoints[i].orbEffect != null && socketPoints[i].orbEffect.activeSelf)
            {
                float angle = baseAngle + i * (360f / socketPoints.Count);
                float rad = angle * Mathf.Deg2Rad;
                // 以主相机的right和forward为平面
                Vector3 offset = playerCamera.right * Mathf.Cos(rad) * radius + playerCamera.forward * Mathf.Sin(rad) * radius;
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

            // 确保小球有正确的组件
            if (socket.orbEffect.GetComponent<Renderer>() == null)
            {
                Debug.LogError("Orb prefab must have a Renderer component!");
            }
            if (socket.orbEffect.GetComponent<OrbEffect>() == null)
            {
                Debug.LogError("Orb prefab must have an OrbEffect component!");
            }
        }
        else
        {
            Debug.LogError("Orb prefab is not assigned in BodySocketInventory!");
        }
    }

    public void AddColorOrb(Material material)
    {
        // 找到第一个未使用的socket
        SocketPoint availableSocket = socketPoints.Find(socket => !socket.isOccupied);
        
        if (availableSocket != null)
        {
            availableSocket.storedMaterial = material;
            availableSocket.isOccupied = true;
            
            if (availableSocket.orbEffect != null)
            {
                availableSocket.orbEffect.SetActive(true);
                OrbEffect orbEffect = availableSocket.orbEffect.GetComponent<OrbEffect>();
                if (orbEffect != null)
                {
                    orbEffect.SetMaterial(material);
                    Debug.Log($"Added material {material.name} to orb at socket {availableSocket.socketName}");
                }
                else
                {
                    Debug.LogError($"Orb at socket {availableSocket.socketName} is missing OrbEffect component!");
                }
            }
            else
            {
                Debug.LogError($"No orb effect found at socket {availableSocket.socketName}!");
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

    public bool TryAttachItem(GrabbableItem item, Transform handTransform)
    {
        // 该功能暂时无用，保留接口
        return false;
    }

    public void DetachItem(GrabbableItem item)
    {
        // 该功能暂时无用，保留接口
    }
}