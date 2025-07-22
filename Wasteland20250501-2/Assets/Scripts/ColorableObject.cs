using UnityEngine;

public class ColorableObject : MonoBehaviour
{
    private Renderer[] objectRenderers;
    private Material[] originalMaterials;
    private bool isInitialized = false;

    void Start()
    {
        InitializeColorableObject();
    }

    void Awake()
    {
        // 确保在Awake中也初始化，防止Start未调用的情况
        if (!isInitialized)
        {
            InitializeColorableObject();
        }
    }

    private void InitializeColorableObject()
    {
        if (isInitialized) return;

        Debug.Log($"🎨 Initializing ColorableObject on {gameObject.name}");

        // 首先检查当前物体的Renderer
        Renderer mainRenderer = GetComponent<Renderer>();

        if (mainRenderer != null)
        {
            // 使用当前物体的Renderer
            objectRenderers = new Renderer[] { mainRenderer };
            originalMaterials = new Material[] { mainRenderer.material };
            Debug.Log($"✅ ColorableObject initialized on {gameObject.name} (main renderer: {mainRenderer.GetType().Name})");
        }
        else
        {
            // 修复：针对你的结构，在子物体中查找Renderer（特别是"default"子物体）
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();

            if (childRenderers != null && childRenderers.Length > 0)
            {
                // 过滤掉不应该染色的Renderer
                System.Collections.Generic.List<Renderer> validRenderers = new System.Collections.Generic.List<Renderer>();
                System.Collections.Generic.List<Material> validMaterials = new System.Collections.Generic.List<Material>();

                foreach (Renderer renderer in childRenderers)
                {
                    // 跳过粒子系统和UI元素
                    if (renderer.GetComponent<ParticleSystem>() != null) continue;
                    if (renderer.gameObject.layer == LayerMask.NameToLayer("UI")) continue;
                    if (renderer.name.ToLower().Contains("ui")) continue;
                    if (renderer.name.ToLower().Contains("particle")) continue;
                    if (renderer.name.ToLower().Contains("effect")) continue;

                    // 特殊处理：优先选择名为"default"的子物体的Renderer
                    bool isDefaultChild = renderer.name == "default" && renderer.transform.parent == this.transform;

                    // 确保Renderer有有效的材质
                    if (renderer.material != null)
                    {
                        validRenderers.Add(renderer);
                        validMaterials.Add(renderer.material);

                        if (isDefaultChild)
                        {
                            Debug.Log($"  📦 Found DEFAULT child renderer: {renderer.name} ({renderer.GetType().Name}) ⭐");
                        }
                        else
                        {
                            Debug.Log($"  📦 Added renderer: {renderer.name} ({renderer.GetType().Name})");
                        }
                    }
                }

                if (validRenderers.Count > 0)
                {
                    objectRenderers = validRenderers.ToArray();
                    originalMaterials = validMaterials.ToArray();
                    Debug.Log($"✅ ColorableObject initialized on {gameObject.name} (found {objectRenderers.Length} valid child renderers)");
                }
                else
                {
                    Debug.LogWarning($"⚠️ ColorableObject on {gameObject.name}: Found {childRenderers.Length} renderers but none are valid for coloring");
                    objectRenderers = null;
                    originalMaterials = null;
                }
            }
            else
            {
                Debug.LogError($"❌ ColorableObject on {gameObject.name}: No Renderer found in object or children!");
                objectRenderers = null;
                originalMaterials = null;
            }
        }

        isInitialized = true;

        // 调试信息
        Debug.Log($"🔍 ColorableObject final status for {gameObject.name}:");
        Debug.Log($"   - Initialized: {isInitialized}");
        Debug.Log($"   - Can be colored: {CanBeColored()}");
        Debug.Log($"   - Renderer count: {(objectRenderers != null ? objectRenderers.Length : 0)}");

        // 特别说明结构
        if (objectRenderers != null && objectRenderers.Length > 0)
        {
            for (int i = 0; i < objectRenderers.Length; i++)
            {
                if (objectRenderers[i] != null)
                {
                    bool isChild = objectRenderers[i].transform != this.transform;
                    Debug.Log($"   - Renderer {i}: {objectRenderers[i].name} {(isChild ? "(child)" : "(self)")}");
                }
            }
        }
    }

    public bool ApplyColor(Material colorMaterial)
    {
        Debug.Log($"🎨 ApplyColor called on {gameObject.name}");

        // 确保已初始化
        if (!isInitialized)
        {
            Debug.Log($"🔄 Object not initialized, initializing now...");
            InitializeColorableObject();
        }

        if (objectRenderers == null || objectRenderers.Length == 0)
        {
            Debug.LogError($"❌ ApplyColor failed on {gameObject.name}: No renderers available");
            return false;
        }

        if (colorMaterial == null)
        {
            Debug.LogError($"❌ ApplyColor failed on {gameObject.name}: No material provided");
            return false;
        }

        try
        {
            bool anySuccess = false;
            for (int i = 0; i < objectRenderers.Length; i++)
            {
                if (objectRenderers[i] != null)
                {
                    // 创建材质的新实例，避免共享材质问题
                    Material newMaterial = new Material(colorMaterial);
                    objectRenderers[i].material = newMaterial;
                    anySuccess = true;
                    Debug.Log($"  ✅ Successfully applied material to renderer {i}: {objectRenderers[i].name}");
                }
                else
                {
                    Debug.LogWarning($"  ⚠️ Renderer {i} is null, skipping");
                }
            }

            if (anySuccess)
            {
                Debug.Log($"🎉 Successfully colored {gameObject.name} with {colorMaterial.name} ({objectRenderers.Length} renderers)");
                return true;
            }
            else
            {
                Debug.LogError($"❌ Failed to color any renderers on {gameObject.name}");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Exception while coloring {gameObject.name}: {e.Message}");
            return false;
        }
    }

    public void ResetColor()
    {
        if (objectRenderers != null && originalMaterials != null &&
            objectRenderers.Length == originalMaterials.Length)
        {
            for (int i = 0; i < objectRenderers.Length; i++)
            {
                if (objectRenderers[i] != null && originalMaterials[i] != null)
                {
                    objectRenderers[i].material = originalMaterials[i];
                }
            }
            Debug.Log($"🔄 Reset color on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Cannot reset color on {gameObject.name} - missing renderer or material data");
        }
    }

    // 检查该物体是否可以被染色
    public bool CanBeColored()
    {
        // 确保已初始化
        if (!isInitialized)
        {
            InitializeColorableObject();
        }

        bool canColor = objectRenderers != null && objectRenderers.Length > 0;
        Debug.Log($"🔍 CanBeColored check for {gameObject.name}: {canColor}");
        return canColor;
    }

    // 获取渲染器信息（调试用）
    [ContextMenu("Debug Renderer Info")]
    public void DebugRendererInfo()
    {
        Debug.Log($"=== Renderer Info for {gameObject.name} ===");

        Renderer mainRenderer = GetComponent<Renderer>();
        Debug.Log($"Main Renderer: {(mainRenderer != null ? mainRenderer.name + " (" + mainRenderer.GetType().Name + ")" : "None")}");

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        Debug.Log($"Total child renderers: {allRenderers.Length}");

        for (int i = 0; i < allRenderers.Length; i++)
        {
            string materialName = allRenderers[i].material != null ? allRenderers[i].material.name : "None";
            Debug.Log($"  {i}: {allRenderers[i].name} ({allRenderers[i].GetType().Name}) - Material: {materialName}");
        }

        Debug.Log($"Valid renderers for coloring: {(objectRenderers != null ? objectRenderers.Length : 0)}");
        Debug.Log($"Is initialized: {isInitialized}");
        Debug.Log($"Can be colored: {CanBeColored()}");
    }

    // 强制重新初始化
    [ContextMenu("Force Reinitialize")]
    public void ForceReinitialize()
    {
        isInitialized = false;
        objectRenderers = null;
        originalMaterials = null;
        InitializeColorableObject();
        Debug.Log($"🔄 Force reinitialized ColorableObject on {gameObject.name}");
    }
}