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
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // 射线检测orb当前位置下方的物体
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            Renderer targetRenderer = hit.collider.GetComponent<Renderer>();
            if (targetRenderer != null && orbEffect != null)
            {
                targetRenderer.material = orbEffect.CurrentMaterial;
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