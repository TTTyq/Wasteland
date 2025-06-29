using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(OrbEffect))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class OrbMaterialDragger : MonoBehaviour
{
    private OrbEffect orbEffect;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Transform originalParent;
    private bool isPainting = false;
    private PaintableTexture currentPaintable;

    private void Awake()
    {
        orbEffect = GetComponent<OrbEffect>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
        originalParent = transform.parent;

        if (grabInteractable != null)
        {
            grabInteractable.selectExited.AddListener(OnReleased);
            grabInteractable.activated.AddListener(OnActivated);
            grabInteractable.deactivated.AddListener(OnDeactivated);
        }
    }

    private void Update()
    {
        if (isPainting && currentPaintable != null)
        {
            // 持续绘制
            currentPaintable.PaintAtPosition(transform.position, orbEffect.CurrentMaterial);
        }
    }

    private void OnActivated(ActivateEventArgs args)
    {
        // 检查是否在可绘制物体上
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            PaintableTexture paintable = hit.collider.GetComponent<PaintableTexture>();
            if (paintable != null)
            {
                isPainting = true;
                currentPaintable = paintable;
            }
        }
    }

    private void OnDeactivated(DeactivateEventArgs args)
    {
        isPainting = false;
        currentPaintable = null;
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // 如果不是在绘制状态，则检查是否需要转移材质
        if (!isPainting)
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            {
                Renderer targetRenderer = hit.collider.GetComponent<Renderer>();
                if (targetRenderer != null && orbEffect != null)
                {
                    targetRenderer.material = orbEffect.CurrentMaterial;
                }
            }
        }

        // 回到原本的圆周位置
        ReturnToOriginalPosition();
    }

    private void ReturnToOriginalPosition()
    {
        // 取消物理影响
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        // 恢复父物体和本地位置
        transform.SetParent(originalParent);
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;

        // 重新激活物理
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
} 