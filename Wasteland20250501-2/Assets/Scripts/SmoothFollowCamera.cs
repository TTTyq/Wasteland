using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public float smoothTime = 0.2f;
    public float distance = 2.0f;
    public float downwardAngle = 15f; // 向下的角度偏移（度）

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (!gameObject.activeSelf) return;

        // 计算带有向下角度偏移的方向
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        Vector3 up = cameraTransform.up;

        // 围绕右轴旋转，产生向下的角度
        Vector3 offsetDirection = Quaternion.AngleAxis(downwardAngle, right) * forward;

        Vector3 targetPos = cameraTransform.position + offsetDirection * distance;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

        // 让物体朝向相机
        Quaternion targetRot = Quaternion.LookRotation(transform.position - cameraTransform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * (1f / smoothTime));
    }
}