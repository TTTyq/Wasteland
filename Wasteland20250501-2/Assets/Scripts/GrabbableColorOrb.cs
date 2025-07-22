using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;

public class GrabbableColorOrb : MonoBehaviour
{
    [Header("Basic Setting")]
    public int orbIndex;
    public Material orbMaterial;
    public BodySocketInventory inventory;

    [Header("Visual Setting")]
    public float handScale = 0.5f;

    [Header("Interaction Setting")]
    public LayerMask colorableLayerMask = -1;
    public float raycastDistance = 10f;

    [Header("Return Settings")]
    public float returnDelay = 2f;

    [Header("Physics Settings")]
    public float throwForce = 15f;
    public float orbRadius = 0.1f;

    // 基础状态
    private bool isInHand = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Transform originalParent;
    private HandColorHolder handHolder;
    private XRGrabInteractable grabInteractable;
    private bool isReturning = false;
    private bool hasBeenSetup = false;

    // 染色控制状态 - 简化版本
    private bool hasBeenThrown = false;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalParent = transform.parent;

        SetupGrabbing();
        // 移除自动添加碰撞体的代码，使用现有的碰撞体设置
    }

    // 移除了自动添加碰撞体的代码，使用你已经设置的碰撞体

    void SetupGrabbing()
    {
        if (hasBeenSetup) return;
        hasBeenSetup = true;

        // 修复：确保小球有合适的碰撞体
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = orbRadius;
            sphereCol.isTrigger = false; // 关键：不是trigger
            Debug.Log($"Added SphereCollider to orb {orbIndex}");
        }
        else
        {
            col.isTrigger = false;
            if (col is SphereCollider sphere)
            {
                sphere.radius = orbRadius;
            }
            Debug.Log($"Orb {orbIndex} collider settings: isTrigger={col.isTrigger}, type={col.GetType().Name}");
        }

        // 修复：刚体设置
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.mass = 0.1f; // 轻一点
        rb.drag = 0.5f;  // 添加阻力，让小球不会飞太快
        rb.angularDrag = 0.5f;

        // 设置XR抓取组件
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        // 设置XRI 3.1.2的属性
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.throwOnDetach = false;
        grabInteractable.attachEaseInTime = 0.15f;

        // 清除并重新设置事件监听
        grabInteractable.selectEntered.RemoveAllListeners();
        grabInteractable.selectExited.RemoveAllListeners();
        grabInteractable.activated.RemoveAllListeners();

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
        grabInteractable.activated.AddListener(OnActivated);

        Debug.Log($"Setup grabbing for orb {orbIndex} - activated event connected");
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (isReturning) return;

        Debug.Log($"Grabbed orb {orbIndex}");

        // 获取交互器的Transform
        Transform interactorTransform = args.interactorObject.transform;

        // 重要修复：使用手柄的父级Transform，而不是Near-Far Interactor本身
        Transform handControllerTransform = interactorTransform.parent;
        if (handControllerTransform != null && handControllerTransform.name.ToLower().Contains("controller"))
        {
            Debug.Log($"Using hand controller transform: {handControllerTransform.name}");
            interactorTransform = handControllerTransform;
        }
        else
        {
            Debug.Log($"Using interactor transform: {interactorTransform.name}");
        }

        // 查找或创建HandColorHolder
        handHolder = interactorTransform.GetComponent<HandColorHolder>();
        if (handHolder == null)
        {
            handHolder = interactorTransform.gameObject.AddComponent<HandColorHolder>();
            Debug.Log($"Added HandColorHolder to {interactorTransform.name}");
        }

        if (handHolder.currentOrb != null && handHolder.currentOrb != this)
        {
            handHolder.ReleaseOrb();
        }

        AttachToHand();
        // 移除grip监听，简化为直接可用模式
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (isReturning) return;

        Debug.Log($"Released orb {orbIndex}");

        // 简化：松开时自动投掷
        if (isInHand)
        {
            Debug.Log($"Auto-throwing orb {orbIndex} on release");
            ThrowOrb();
        }
    }

    void OnActivated(ActivateEventArgs args)
    {
        Debug.Log($"XRGrabInteractable activated! isInHand: {isInHand}, isReturning: {isReturning}");

        if (isInHand && !isReturning)
        {
            if (grabInteractable.isSelected)
            {
                Debug.Log($"Throwing orb {orbIndex} from activated event");
                ThrowOrb();
            }
        }
    }

    void AttachToHand()
    {
        isInHand = true;
        if (handHolder != null)
        {
            handHolder.AttachOrb(this);
            transform.localScale = originalScale * handScale;

            // 禁用物理，确保小球不会掉落
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Debug.Log($"Orb {orbIndex} attached to hand with physics disabled");
        }
    }

    void ThrowOrb()
    {
        if (!isInHand || handHolder == null)
        {
            Debug.Log("Cannot throw - not in hand or no hand holder");
            return;
        }

        Debug.Log($"🎯 Throwing orb {orbIndex} with physics");

        // 释放小球从手柄
        if (handHolder != null)
        {
            handHolder.ReleaseOrb();
        }

        // 启用物理
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        // 投掷方向计算
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        Vector3 throwDirection = mainCamera.transform.forward;

        // 投掷小球
        rb.velocity = throwDirection * throwForce;
        rb.angularVelocity = Random.insideUnitSphere * 2f;

        Debug.Log($"Orb thrown with velocity: {rb.velocity}");

        // 标记已投掷，启动返回计时器
        hasBeenThrown = true;
        isInHand = false;
        Invoke(nameof(ReturnToOriginalPosition), returnDelay);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"🔥 COLLISION DETECTED! Orb {orbIndex} hit {collision.gameObject.name}");
        Debug.Log($"   - Is returning: {isReturning}");
        Debug.Log($"   - Contact points: {collision.contacts.Length}");

        // 只要不在返回过程中就可以染色
        if (isReturning)
        {
            Debug.Log($"❌ Collision ignored - orb is returning");
            return;
        }

        GameObject target = collision.gameObject;
        Debug.Log($"🎯 Orb {orbIndex} collided with: {target.name}");

        // 跳过自己
        if (target == gameObject)
        {
            Debug.Log($"⏭️ Skipping self collision");
            return;
        }

        // 修复：针对你的结构，先检查碰撞物体，然后检查其父物体
        ColorableObject colorable = FindColorableObjectInHierarchy(target);

        if (colorable == null)
        {
            Debug.Log($"⏭️ Skipping - no ColorableObject found on {target.name} or its parents");
            return;
        }

        Debug.Log($"✅ Found ColorableObject on {colorable.gameObject.name}, proceeding to color");

        // 尝试染色（注意：染色目标是有ColorableObject的物体，不是碰撞的子物体）
        bool success = TryColorObject(colorable.gameObject);

        if (success)
        {
            Debug.Log($"🎉 SUCCESS! Colored {colorable.gameObject.name} on collision");
            // 染色成功后延迟返回，让用户看到效果
            Invoke(nameof(StartReturnProcess), 1f);
        }
        else
        {
            Debug.Log($"❌ Failed to color {colorable.gameObject.name} on collision");
        }
    }

    // 新方法：在物体层级中查找ColorableObject组件
    private ColorableObject FindColorableObjectInHierarchy(GameObject target)
    {
        // 1. 先检查碰撞的物体本身
        ColorableObject colorable = target.GetComponent<ColorableObject>();
        if (colorable != null)
        {
            Debug.Log($"🔍 Found ColorableObject on collision target: {target.name}");
            return colorable;
        }

        // 2. 检查子物体（如果碰撞的是父物体）
        colorable = target.GetComponentInChildren<ColorableObject>();
        if (colorable != null)
        {
            Debug.Log($"🔍 Found ColorableObject in children of: {target.name}");
            return colorable;
        }

        // 3. 关键修复：检查父物体（针对你的结构：default子物体碰撞，但ColorableObject在父物体上）
        Transform parent = target.transform.parent;
        while (parent != null)
        {
            colorable = parent.GetComponent<ColorableObject>();
            if (colorable != null)
            {
                Debug.Log($"🔍 Found ColorableObject on parent: {parent.name} (collision was with child: {target.name})");
                return colorable;
            }
            parent = parent.parent; // 继续向上查找
        }

        // 4. 特殊处理：如果碰撞物体名为"default"，直接检查父物体是否是tripo_convert
        if (target.name == "default" && target.transform.parent != null)
        {
            GameObject parentObj = target.transform.parent.gameObject;
            if (parentObj.name.StartsWith("tripo_convert_"))
            {
                colorable = parentObj.GetComponent<ColorableObject>();
                if (colorable != null)
                {
                    Debug.Log($"🔍 Special case: Found ColorableObject on tripo_convert parent: {parentObj.name}");
                    return colorable;
                }
                else
                {
                    Debug.LogWarning($"⚠️ tripo_convert parent {parentObj.name} missing ColorableObject component!");
                }
            }
        }

        return null;
    }

    // 添加trigger检测作为备用
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🔸 TRIGGER DETECTED! Orb {orbIndex} triggered with {other.gameObject.name}");

        if (isReturning) return;

        GameObject target = other.gameObject;

        // 跳过自己
        if (target == gameObject) return;

        // 使用相同的层级查找逻辑
        ColorableObject colorable = FindColorableObjectInHierarchy(target);

        if (colorable == null) return;

        bool success = TryColorObject(colorable.gameObject);
        if (success)
        {
            Debug.Log($"🎉 SUCCESS via trigger! Colored {colorable.gameObject.name}");
            Invoke(nameof(StartReturnProcess), 1f);
        }
    }

    private bool TryColorObject(GameObject target)
    {
        Debug.Log($"🔧 TryColorObject called for: {target.name}");

        if (orbMaterial == null)
        {
            Debug.LogError("❌ No orb material available!");
            return false;
        }

        // 检查是否有渲染器
        Renderer targetRenderer = target.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            targetRenderer = target.GetComponentInChildren<Renderer>();
        }

        if (targetRenderer == null)
        {
            Debug.Log($"⚠️ {target.name} has no renderer, skipping");
            return false;
        }

        Debug.Log($"✅ Found renderer on {target.name}: {targetRenderer.GetType().Name}");

        // 获取或添加ColorableObject
        ColorableObject colorable = target.GetComponent<ColorableObject>();
        if (colorable == null)
        {
            Debug.Log($"➕ Adding ColorableObject to {target.name}");
            colorable = target.AddComponent<ColorableObject>();

            // 强制初始化ColorableObject
            try
            {
                colorable.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
                Debug.Log($"✅ ColorableObject initialized on {target.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to initialize ColorableObject: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"✅ Found existing ColorableObject on {target.name}");
        }

        // 检查ColorableObject是否可以染色
        if (colorable != null && colorable.CanBeColored())
        {
            Debug.Log($"🎨 Attempting to apply color to {target.name}");
            bool result = colorable.ApplyColor(orbMaterial);
            Debug.Log($"🎨 ApplyColor result: {result}");
            return result;
        }
        else
        {
            Debug.LogWarning($"❌ ColorableObject on {target.name} cannot be colored");

            // 备用方案：直接修改渲染器材质
            try
            {
                Debug.Log($"🔄 Trying direct material application to {target.name}");
                targetRenderer.material = new Material(orbMaterial);
                Debug.Log($"✅ Direct material application succeeded on {target.name}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Direct material application failed: {e.Message}");
                return false;
            }
        }
    }

    void StartReturnProcess()
    {
        isReturning = true;
        isInHand = false;
        hasBeenThrown = false;

        if (handHolder != null)
        {
            handHolder.ReleaseOrb();
        }

        Invoke(nameof(ReturnToOriginalPosition), returnDelay);

        Debug.Log($"Orb {orbIndex} will return in {returnDelay} seconds");
    }

    void ReturnToOriginalPosition()
    {
        if (originalParent == null)
        {
            Debug.LogError($"Orb {orbIndex}: Original parent is null!");
            return;
        }

        isInHand = false;
        isReturning = false;
        hasBeenThrown = false;

        transform.parent = originalParent;
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;

        // 重置物理状态
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (handHolder != null)
        {
            handHolder.ReleaseOrb();
        }

        Debug.Log($"Orb {orbIndex} returned to original position");
    }

    void Update()
    {
        // 测试用键盘输入
        if (Input.GetKeyDown(KeyCode.T) && isInHand && !isReturning)
        {
            Debug.Log("Test activation with T key");
            ThrowOrb();
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
            grabInteractable.activated.RemoveListener(OnActivated);
        }
    }

    // 调试方法
    [ContextMenu("Debug ColorableObjects")]
    void DebugColorableObjects()
    {
        Debug.Log("=== ColorableObjects Analysis ===");

        ColorableObject[] allColorables = FindObjectsOfType<ColorableObject>();
        int validColorables = 0;

        foreach (ColorableObject colorable in allColorables)
        {
            GameObject obj = colorable.gameObject;
            Renderer renderer = obj.GetComponent<Renderer>();
            Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
            Collider collider = obj.GetComponent<Collider>();

            Debug.Log($"ColorableObject: {obj.name}");
            Debug.Log($"  - Has Renderer: {renderer != null}");
            Debug.Log($"  - Child Renderers: {childRenderers.Length}");
            Debug.Log($"  - Has Collider: {collider != null}");
            Debug.Log($"  - Can be colored: {colorable.CanBeColored()}");
            Debug.Log($"  - Position: {obj.transform.position}");

            if (colorable.CanBeColored()) validColorables++;
        }

        Debug.Log($"=== Summary: {validColorables}/{allColorables.Length} valid colorable objects ===");
    }
}