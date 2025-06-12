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

    [Header("VR Controller References")]
    public Transform leftController;
    public Transform rightController;
    
    [Header("Orb Settings")]
    public GameObject orbPrefab;
    public float orbRotationSpeed = 30f;
    public float orbRadius = 0.2f; // 减小半径，让光球更靠近手柄
    public float orbHeight = 0.1f; // 减小高度偏移
    public float orbDistanceFromController = 0.3f; // 光球距离手柄的距离

    private List<SocketPoint> socketPoints = new List<SocketPoint>();
    private Transform activeController; // 当前激活的手柄

    private void Start()
    {
        // 为每个手柄创建socket点
        CreateControllerSocketPoints();
        
        // 初始化所有socket点
        foreach (var socket in socketPoints)
        {
            socket.isOccupied = false;
            socket.currentItem = null;
            CreateOrbEffect(socket);
        }
    }

    private void CreateControllerSocketPoints()
    {
        // 为左手柄创建socket点
        if (leftController != null)
        {
            CreateSocketPoint("LeftController", leftController);
        }

        // 为右手柄创建socket点
        if (rightController != null)
        {
            CreateSocketPoint("RightController", rightController);
        }
    }

    private void CreateSocketPoint(string name, Transform controller)
    {
        GameObject socketObj = new GameObject(name);
        socketObj.transform.parent = controller;
        socketObj.transform.localPosition = Vector3.zero;
        socketObj.transform.localRotation = Quaternion.identity;

        SocketPoint socket = new SocketPoint
        {
            socketName = name,
            socketTransform = socketObj.transform,
            isOccupied = false,
            currentItem = null,
            orbEffect = null
        };

        socketPoints.Add(socket);
    }

    private void Update()
    {
        UpdateOrbPositions();
    }

    private void UpdateOrbPositions()
    {
        for (int i = 0; i < socketPoints.Count; i++)
        {
            if (socketPoints[i].orbEffect != null && socketPoints[i].orbEffect.activeSelf)
            {
                Transform controller = socketPoints[i].socketTransform.parent;
                if (controller != null)
                {
                    // 计算光球位置
                    float angle = Time.time * orbRotationSpeed + (i * (360f / socketPoints.Count));
                    Vector3 offset = new Vector3(
                        Mathf.Cos(angle * Mathf.Deg2Rad) * orbRadius,
                        orbHeight,
                        Mathf.Sin(angle * Mathf.Deg2Rad) * orbRadius
                    );

                    // 将光球放置在手柄前方
                    Vector3 forward = controller.forward;
                    Vector3 right = controller.right;
                    Vector3 up = controller.up;

                    Vector3 position = controller.position + 
                                     forward * orbDistanceFromController + 
                                     right * offset.x + 
                                     up * offset.y;

                    socketPoints[i].orbEffect.transform.position = position;
                    socketPoints[i].orbEffect.transform.rotation = controller.rotation;
                }
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
        SocketPoint closestSocket = null;
        float closestDistance = float.MaxValue;

        foreach (var socket in socketPoints)
        {
            if (!socket.isOccupied)
            {
                float distance = Vector3.Distance(handTransform.position, socket.socketTransform.position);
                if (distance < orbDistanceFromController && distance < closestDistance)
                {
                    closestSocket = socket;
                    closestDistance = distance;
                }
            }
        }

        if (closestSocket != null)
        {
            item.transform.position = closestSocket.socketTransform.position;
            item.transform.rotation = closestSocket.socketTransform.rotation;
            item.transform.parent = closestSocket.socketTransform;
            
            closestSocket.isOccupied = true;
            closestSocket.currentItem = item;
            item.isAttached = true;
            
            return true;
        }

        return false;
    }

    public void DetachItem(GrabbableItem item)
    {
        foreach (var socket in socketPoints)
        {
            if (socket.currentItem == item)
            {
                socket.isOccupied = false;
                socket.currentItem = null;
                item.isAttached = false;
                break;
            }
        }
    }
}