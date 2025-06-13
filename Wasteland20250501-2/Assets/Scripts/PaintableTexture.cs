using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(MeshRenderer))]
public class PaintableTexture : MonoBehaviour
{
    [Header("Paint Settings")]
    public float brushSize = 0.1f; // 笔刷大小
    public float paintStrength = 1f; // 绘制强度
    public LayerMask paintableLayer; // 可绘制的层

    private MeshRenderer meshRenderer;
    private Material paintMaterial;
    private Texture2D paintTexture;
    private Vector2 textureSize = new Vector2(512, 512); // 纹理大小
    private bool isCompatible = false;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        
        // 检查材质兼容性
        CheckMaterialCompatibility();
        
        if (isCompatible)
        {
            // 创建可绘制的纹理
            CreatePaintTexture();
        }
    }

    private void CheckMaterialCompatibility()
    {
        if (meshRenderer == null || meshRenderer.sharedMaterial == null)
        {
            Debug.LogError($"{gameObject.name} 缺少材质或MeshRenderer组件！");
            return;
        }

        Material mat = meshRenderer.sharedMaterial;
        string shaderName = mat.shader.name.ToLower();

        // 检查是否是支持纹理的Shader
        if (shaderName.Contains("standard") || 
            shaderName.Contains("lit") || 
            shaderName.Contains("unlit") ||
            shaderName.Contains("pbr"))
        {
            // 检查是否有主纹理属性
            if (mat.HasProperty("_MainTex") || 
                mat.HasProperty("_BaseMap") || 
                mat.HasProperty("_BaseColorMap"))
            {
                isCompatible = true;
                Debug.Log($"{gameObject.name} 的材质支持纹理绘制！");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} 的Shader不支持纹理属性！");
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 使用了不支持的Shader类型：{shaderName}");
        }
    }

    private void CreatePaintTexture()
    {
        if (!isCompatible) return;

        // 创建新的纹理
        paintTexture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        Color[] colors = new Color[(int)(textureSize.x * textureSize.y)];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white; // 初始为白色
        }
        paintTexture.SetPixels(colors);
        paintTexture.Apply();

        // 创建新的材质
        paintMaterial = new Material(meshRenderer.sharedMaterial);
        
        // 设置主纹理
        if (paintMaterial.HasProperty("_MainTex"))
            paintMaterial.SetTexture("_MainTex", paintTexture);
        else if (paintMaterial.HasProperty("_BaseMap"))
            paintMaterial.SetTexture("_BaseMap", paintTexture);
        else if (paintMaterial.HasProperty("_BaseColorMap"))
            paintMaterial.SetTexture("_BaseColorMap", paintTexture);

        meshRenderer.material = paintMaterial;
    }

    public void PaintAtPosition(Vector3 worldPosition, Material paintMaterial)
    {
        if (!isCompatible) return;

        // 将世界坐标转换为UV坐标
        Vector2 uv = WorldToUV(worldPosition);
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
            return;

        // 计算笔刷范围
        int brushPixelSize = Mathf.RoundToInt(brushSize * textureSize.x);
        int centerX = Mathf.RoundToInt(uv.x * textureSize.x);
        int centerY = Mathf.RoundToInt(uv.y * textureSize.y);

        // 在笔刷范围内绘制
        for (int x = -brushPixelSize; x <= brushPixelSize; x++)
        {
            for (int y = -brushPixelSize; y <= brushPixelSize; y++)
            {
                int pixelX = centerX + x;
                int pixelY = centerY + y;

                // 检查是否在纹理范围内
                if (pixelX >= 0 && pixelX < textureSize.x && pixelY >= 0 && pixelY < textureSize.y)
                {
                    // 计算到笔刷中心的距离
                    float distance = Mathf.Sqrt(x * x + y * y) / brushPixelSize;
                    if (distance <= 1)
                    {
                        // 使用平滑的笔刷效果
                        float strength = Mathf.Pow(1 - distance, 2) * paintStrength;
                        
                        // 获取当前像素颜色
                        Color currentColor = paintTexture.GetPixel(pixelX, pixelY);
                        
                        // 获取材质的主纹理颜色
                        Color materialColor = Color.white;
                        if (paintMaterial.HasProperty("_BaseColor"))
                            materialColor = paintMaterial.GetColor("_BaseColor");
                        else if (paintMaterial.HasProperty("_Color"))
                            materialColor = paintMaterial.GetColor("_Color");
                        
                        // 混合颜色
                        Color newColor = Color.Lerp(currentColor, materialColor, strength);
                        paintTexture.SetPixel(pixelX, pixelY, newColor);
                    }
                }
            }
        }

        // 应用更改
        paintTexture.Apply();
    }

    private Vector2 WorldToUV(Vector3 worldPosition)
    {
        // 将世界坐标转换为局部坐标
        Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
        
        // 假设这是一个平面，使用X和Z坐标作为UV坐标
        // 注意：这里假设物体是水平放置的，可能需要根据实际旋转调整
        float u = (localPosition.x + 0.5f);
        float v = (localPosition.z + 0.5f);
        
        return new Vector2(u, v);
    }

    // 用于VR交互的方法
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (!isCompatible)
        {
            // 可以在这里添加视觉反馈，表明物体不支持绘制
            Debug.Log("此物体不支持纹理绘制！");
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        // 可以在这里添加视觉反馈
    }

    public void OnActivate(ActivateEventArgs args)
    {
        if (!isCompatible) return;

        // 当激活时（比如按下扳机键）进行绘制
        if (args.interactorObject != null)
        {
            Vector3 position = args.interactorObject.transform.position;
            // 获取当前选中的orb材质
            OrbEffect orbEffect = FindObjectOfType<OrbEffect>();
            if (orbEffect != null)
            {
                PaintAtPosition(position, orbEffect.CurrentMaterial);
            }
        }
    }
} 