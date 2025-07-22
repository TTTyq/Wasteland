using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Attachment;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
    public float raycastDistance = 2f;

    [Header("Return Settings")]
    public float returnDelay = 2f;

    private bool isInHand = false;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Transform originalParent;
    private HandColorHolder handHolder;
    private XRGrabInteractable grabInteractable;
    private NearFarInteractor nearFarInteractor;
    private bool isReturning = false;
    private bool hasBeenSetup = false;
    private bool isNearFarConnected = false;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalParent = transform.parent;

        SetupGrabbing();
    }

    void SetupGrabbing()
    {
        if (hasBeenSetup) return;
        hasBeenSetup = true;

        // 确保有正确的碰撞体 - 这是关键修复
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.15f; // 稍微大一点便于抓取
            sphereCol.isTrigger = false; // 关键：必须是false
            Debug.Log($"Added SphereCollider to orb {orbIndex}");
        }
        else
        {
            col.isTrigger = false; // 确保现有碰撞体不是trigger
        }

        // 确保有刚体
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        // 设置XR抓取组件
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        // 重要：设置 XRI 3.1.2 的新属性
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.throwOnDetach = false; // 我们自己处理投掷
        grabInteractable.attachEaseInTime = 0.15f; // XRI 3.1.2要求的最小值

        // 清除并重新设置事件监听
        grabInteractable.selectEntered.RemoveAllListeners();
        grabInteractable.selectExited.RemoveAllListeners();
        grabInteractable.activated.RemoveAllListeners();

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);

        // 重要：连接到Near-Far Interactor用于trigger激活
        ConnectToNearFarInteractor();

        Debug.Log($"Setup grabbing for orb {orbIndex} (XRI 3.1.2)");
    }

    private void ConnectToNearFarInteractor()
    {
        // 查找场景中的Near-Far Interactor
        var allInteractors = FindObjectsOfType<NearFarInteractor>();

        foreach (var interactor in allInteractors)
        {
            // 查找左手的Near-Far Interactor
            if (interactor.name.ToLower().Contains("left") ||
                (interactor.handedness == Handedness.Left))
            {
                nearFarInteractor = interactor;
                break;
            }
        }

        // 如果没找到指定的，使用第一个可用的
        if (nearFarInteractor == null && allInteractors.Length > 0)
        {
            nearFarInteractor = allInteractors[0];
        }

        if (nearFarInteractor != null)
        {
            // 直接监听Near-Far Interactor的activated事件
            nearFarInteractor.activated.RemoveListener(OnNearFarActivated);
            nearFarInteractor.activated.AddListener(OnNearFarActivated);
            isNearFarConnected = true;
            Debug.Log($"Connected to Near-Far Interactor: {nearFarInteractor.name}");
        }
        else
        {
            Debug.LogError("Could not find Near-Far Interactor! Trigger activation will not work.");
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (isReturning) return;

        Debug.Log($"Grabbed orb {orbIndex}");

        // 获取交互器的Transform（适配Near-Far Interactor）
        Transform interactorTransform = args.interactorObject.transform;

        // 尝试获取Interaction Attach Controller
        var attachController = interactorTransform.GetComponent<InteractionAttachController>();
        if (attachController != null)
        {
            // 使用attach controller的transform作为基础
            interactorTransform = attachController.transform;
            Debug.Log($"Found Interaction Attach Controller on {attachController.name}");
        }

        // 查找或创建HandColorHolder
        handHolder = interactorTransform.GetComponent<HandColorHolder>();
        if (handHolder == null)
        {
            handHolder = interactorTransform.gameObject.AddComponent<HandColorHolder>();
            Debug.Log($"Added HandColorHolder to {interactorTransform.name}");
        }

        // 如果手上已经有其他小球，先放下
        if (handHolder.currentOrb != null && handHolder.currentOrb != this)
        {
            handHolder.ReleaseOrb();
        }

        // 小球附着到手上
        AttachToHand();
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (isReturning) return;

        Debug.Log($"Released orb {orbIndex}");

        // 如果不是在投掷状态，就回到原位置
        if (isInHand)
        {
            ReturnToOriginalPosition();
        }
    }

    private void OnNearFarActivated(ActivateEventArgs args)
    {
        Debug.Log($"Near-Far Interactor activated! isInHand: {isInHand}, isReturning: {isReturning}");

        // 检查当前选中的是否是这个小球
        if (isInHand && !isReturning)
        {
            // 额外检查：确保这个小球确实被选中了
            if (grabInteractable.isSelected)
            {
                Debug.Log($"Throwing orb {orbIndex} from Near-Far Interactor activation");
                ThrowOrb();
            }
            else
            {
                Debug.Log($"Orb {orbIndex} is in hand but not selected, ignoring activation");
            }
        }
        else
        {
            Debug.Log($"Orb {orbIndex} cannot be thrown - isInHand: {isInHand}, isReturning: {isReturning}");
        }
    }

    void AttachToHand()
    {
        isInHand = true;
        if (handHolder != null)
        {
            handHolder.AttachOrb(this);
            transform.localScale = originalScale * handScale;
            Debug.Log($"Orb {orbIndex} attached to hand");
        }
    }

    void ThrowOrb()
    {
        if (!isInHand || handHolder == null)
        {
            Debug.Log("Cannot throw - not in hand or no hand holder");
            return;
        }

        // 从手柄位置发射射线
        Ray ray = new Ray(handHolder.transform.position, handHolder.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 2f);
        Debug.Log($"Throwing orb from position: {ray.origin}, direction: {ray.direction}");

        if (Physics.Raycast(ray, out hit, raycastDistance, colorableLayerMask))
        {
            Debug.Log($"Raycast hit: {hit.collider.name}");

            ColorableObject colorable = hit.collider.GetComponent<ColorableObject>();
            if (colorable != null && orbMaterial != null)
            {
                bool success = colorable.ApplyColor(orbMaterial);
                if (success)
                {
                    Debug.Log($"Successfully colored {hit.collider.name}");

                    // 开始返回流程
                    StartReturnProcess();
                    return;
                }
            }
            else
            {
                Debug.Log($"Hit object {hit.collider.name} has no ColorableObject component");
            }
        }
        else
        {
            Debug.Log("No valid target found for coloring");
        }

        // 如果没有命中有效目标，也开始返回流程
        StartReturnProcess();
    }

    void StartReturnProcess()
    {
        isReturning = true;
        isInHand = false;

        // 从手柄分离
        if (handHolder != null)
        {
            handHolder.ReleaseOrb();
        }

        // 延迟后返回原位置
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

        transform.parent = originalParent;
        transform.localPosition = originalPosition;
        transform.localScale = originalScale;

        if (handHolder != null)
        {
            handHolder.ReleaseOrb();
        }

        Debug.Log($"Orb {orbIndex} returned to original position");
    }

    // 临时调试功能 - 用T键测试投掷
    void Update()
    {
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
        }

        if (nearFarInteractor != null && isNearFarConnected)
        {
            nearFarInteractor.activated.RemoveListener(OnNearFarActivated);
        }
    }
}