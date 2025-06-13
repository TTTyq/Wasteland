using UnityEngine;

public class OrbEffect : MonoBehaviour
{
    public float pulseSpeed = 1f;
    public float pulseAmount = 0.2f;
    public float rotationSpeed = 50f;
    
    private Vector3 originalScale;
    private Material orbMaterial;
    private Renderer orbRenderer;

    private void Awake()
    {
        // 确保有必要的组件
        if (GetComponent<Renderer>() == null)
        {
            // 添加MeshRenderer和MeshFilter
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            orbRenderer = gameObject.AddComponent<MeshRenderer>();
            Debug.Log("Added MeshRenderer and MeshFilter to orb");
        }
        else
        {
            orbRenderer = GetComponent<Renderer>();
        }
    }

    private void Start()
    {
        originalScale = transform.localScale;
        
        // 获取渲染器组件
        if (orbRenderer == null)
        {
            orbRenderer = GetComponent<Renderer>();
            if (orbRenderer == null)
            {
                Debug.LogError("No renderer found on orb!");
                return;
            }
        }

        // 创建新的材质实例
        if (orbRenderer.sharedMaterial == null)
        {
            // 如果没有材质，创建一个默认的发光材质
            orbMaterial = new Material(Shader.Find("Standard"));
            orbMaterial.EnableKeyword("_EMISSION");
            orbMaterial.SetColor("_EmissionColor", Color.white);
            orbRenderer.material = orbMaterial;
            Debug.Log("Created default material for orb");
        }
        else
        {
            orbMaterial = new Material(orbRenderer.sharedMaterial);
            orbRenderer.material = orbMaterial;
        }
    }

    private void Update()
    {
        // 脉冲效果
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
        
        // 旋转效果
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    public void SetMaterial(Material material)
    {
        if (orbRenderer == null)
        {
            orbRenderer = GetComponent<Renderer>();
            if (orbRenderer == null)
            {
                Debug.LogError("Failed to get Renderer component!");
                return;
            }
        }

        if (material != null)
        {
            // 直接使用传入的材质
            orbRenderer.material = material;
            
            // 确保发光效果
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                Color emissionColor = material.GetColor("_EmissionColor");
                material.SetColor("_EmissionColor", emissionColor * 2f);
            }
            
            Debug.Log($"Orb material set: {material.name}");
        }
        else
        {
            Debug.LogWarning("No material provided to set on orb");
        }
    }

    private void OnDestroy()
    {
        if (orbMaterial != null)
        {
            Destroy(orbMaterial);
        }
    }

    public Material CurrentMaterial => orbRenderer != null ? orbRenderer.material : null;
} 