using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Underwater : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public Color color = new Color(0, 0.5f, 0.8f, 1);
        public float FogDensity = 1;
        [Range(0, 1)]
        public float alpha = 0.5f;
        public float refraction = 0.1f;
        public Texture normalmap;
        public Vector4 UV = new Vector4(1, 1, 0.2f, 0.1f);
    }

    public Settings settings = new Settings();
    UnderwaterPass m_UnderwaterPass;

    public override void Create()
    {
        m_UnderwaterPass = new UnderwaterPass();
        m_UnderwaterPass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
            return;

        m_UnderwaterPass.Setup(settings);
        renderer.EnqueuePass(m_UnderwaterPass);
    }

    class UnderwaterPass : ScriptableRenderPass
    {
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Underwater");
        private Settings m_Settings;
        private Material m_Material;

        public void Setup(Settings settings)
        {
            m_Settings = settings;
            m_Material = settings.material;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                // 设置材质属性
                m_Material.SetFloat("_FogDensity", m_Settings.FogDensity);
                m_Material.SetFloat("_alpha", m_Settings.alpha);
                m_Material.SetColor("_color", m_Settings.color);
                m_Material.SetTexture("_NormalMap", m_Settings.normalmap);
                m_Material.SetFloat("_refraction", m_Settings.refraction);
                m_Material.SetVector("_normalUV", m_Settings.UV);

                // 使用RenderingUtils.fullscreenMesh绘制全屏四边形
                // 这里不需要设置源目标，因为材质会自动采样_CameraOpaqueTexture
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}