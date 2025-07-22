using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRGameViewCamera : MonoBehaviour
{
    [Header("References")]
    public XROrigin xrOrigin;
    public Camera vrMainCamera;

    [Header("Game View Camera Settings")]
    public Camera gameViewCamera;
    public bool enableGameViewCamera = true;

    void Start()
    {
        SetupGameViewCamera();
    }

    void SetupGameViewCamera()
    {
        if (vrMainCamera == null)
            vrMainCamera = Camera.main;

        // 创建或设置Game View摄像头
        if (gameViewCamera == null)
        {
            GameObject gameViewCameraGO = new GameObject("GameViewCamera");
            gameViewCamera = gameViewCameraGO.AddComponent<Camera>();
        }

        // 复制VR摄像头设置
        gameViewCamera.fieldOfView = vrMainCamera.fieldOfView;
        gameViewCamera.nearClipPlane = vrMainCamera.nearClipPlane;
        gameViewCamera.farClipPlane = vrMainCamera.farClipPlane;
        gameViewCamera.cullingMask = vrMainCamera.cullingMask;
        gameViewCamera.backgroundColor = vrMainCamera.backgroundColor;
        gameViewCamera.clearFlags = vrMainCamera.clearFlags;

        // 设置更高的Depth，让它在Game窗口中显示
        gameViewCamera.depth = vrMainCamera.depth + 1;

        // 只在编辑器中启用
        gameViewCamera.enabled = enableGameViewCamera;

#if !UNITY_EDITOR
        // 在构建版本中禁用Game View摄像头
        gameViewCamera.enabled = false;
#endif
    }

    void Update()
    {
        if (gameViewCamera != null && gameViewCamera.enabled)
        {
            // 跟随VR主摄像头的位置和旋转，但使用居中位置
            gameViewCamera.transform.position = vrMainCamera.transform.position;
            gameViewCamera.transform.rotation = vrMainCamera.transform.rotation;
        }
    }
}