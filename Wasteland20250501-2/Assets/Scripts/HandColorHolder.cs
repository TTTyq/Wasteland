using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Attachment;

public class HandColorHolder : MonoBehaviour
{
    public GrabbableColorOrb currentOrb;
    public Vector3 attachOffset = new Vector3(0.05f, 0, 0.1f);

    private Transform attachPoint;

    void Awake()
    {
        CreateAttachPoint();
    }

    void CreateAttachPoint()
    {
        // 检查是否已经有附着点
        Transform existingAttach = transform.Find("OrbAttachPoint");
        if (existingAttach != null)
        {
            attachPoint = existingAttach;
            Debug.Log($"Found existing attach point on {gameObject.name}");
            return;
        }

        // 创建新的附着点
        GameObject attach = new GameObject("OrbAttachPoint");
        attach.transform.parent = transform;
        attach.transform.localPosition = attachOffset;
        attach.transform.localRotation = Quaternion.identity;
        attachPoint = attach.transform;

        Debug.Log($"Created new attach point for {gameObject.name}");
    }

    public void AttachOrb(GrabbableColorOrb orb)
    {
        if (currentOrb != null && currentOrb != orb)
        {
            ReleaseOrb();
        }

        currentOrb = orb;

        // 优先尝试使用Interaction Attach Controller的Transform
        var attachController = GetComponent<InteractionAttachController>();
        if (attachController != null && attachController.transformToFollow != null)
        {
            // 将小球附着到attach controller的transform
            orb.transform.parent = attachController.transformToFollow;
            orb.transform.localPosition = Vector3.zero;
            orb.transform.localRotation = Quaternion.identity;
            Debug.Log($"Attached orb {orb.orbIndex} to Interaction Attach Controller transform: {attachController.transformToFollow.name}");
        }
        else if (attachPoint != null)
        {
            // 回退到手动创建的附着点
            orb.transform.parent = attachPoint;
            orb.transform.localPosition = Vector3.zero;
            orb.transform.localRotation = Quaternion.identity;
            Debug.Log($"Attached orb {orb.orbIndex} to manual attach point: {attachPoint.name}");
        }
        else
        {
            Debug.LogError($"No valid attach point found on {gameObject.name}");
            CreateAttachPoint(); // 尝试重新创建

            if (attachPoint != null)
            {
                orb.transform.parent = attachPoint;
                orb.transform.localPosition = Vector3.zero;
                orb.transform.localRotation = Quaternion.identity;
                Debug.Log($"Attached orb {orb.orbIndex} to newly created attach point");
            }
        }
    }

    public void ReleaseOrb()
    {
        if (currentOrb != null)
        {
            Debug.Log($"Released orb {currentOrb.orbIndex} from {gameObject.name}");
        }
        currentOrb = null;
    }
}