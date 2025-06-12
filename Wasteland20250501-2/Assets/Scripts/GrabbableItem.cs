using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbableItem : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Collider itemCollider;
    public bool isAttached { get; set; }

    private void Start()
    {
        // 获取或添加必要的组件
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        itemCollider = GetComponent<Collider>();
        if (itemCollider == null)
        {
            Debug.LogWarning($"{gameObject.name} 缺少 Collider 组件，请添加适当的碰撞体");
        }

        // 设置抓取交互
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        // 当物体被抓起时
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        // 当物体被释放时
        if (rb != null)
        {
            rb.isKinematic = false;
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