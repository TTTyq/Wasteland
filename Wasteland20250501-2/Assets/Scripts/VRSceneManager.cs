using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;

public class VRSceneManager : MonoBehaviour
{
    public static VRSceneManager Instance { get; private set; }
    
    [SerializeField] private GameObject xrOriginPrefab; // XR Origin预制体
    [SerializeField] private Vector3 startPosition = new Vector3(0, 1.6f, 0); // 默认起始位置
    [SerializeField] private Vector3 startRotation = Vector3.zero; // 默认起始旋转

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeVRScene()
    {
        // 初始化XR系统
        var xrManager = XRGeneralSettings.Instance.Manager;
        if (xrManager != null)
        {
            xrManager.InitializeLoaderSync();
            xrManager.StartSubsystems();
        }

        // 实例化XR Origin
        if (xrOriginPrefab != null)
        {
            GameObject xrOrigin = Instantiate(xrOriginPrefab, startPosition, Quaternion.Euler(startRotation));
            DontDestroyOnLoad(xrOrigin);
        }
    }

    public void SetPlayerPosition(Vector3 position, Vector3 rotation)
    {
        if (xrOriginPrefab != null)
        {
            xrOriginPrefab.transform.position = position;
            xrOriginPrefab.transform.rotation = Quaternion.Euler(rotation);
        }
    }
} 