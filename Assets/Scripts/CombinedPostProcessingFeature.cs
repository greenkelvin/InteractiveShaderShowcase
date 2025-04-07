using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CombinedPostProcessingFeature : ScriptableRendererFeature
{
    class CombinedRenderPass : ScriptableRenderPass
    {
        private Material effectMaterial;
        private RTHandle tempTexture;
        private bool isActive;

        public void Setup(Material material, bool active)
        {
            effectMaterial = material;
            isActive = active;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_CombinedEffectTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!isActive || effectMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("CombinedEffectPass");
            RTHandle cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            Blitter.BlitCameraTexture(cmd, cameraTarget, tempTexture, effectMaterial, 0);
            Blitter.BlitCameraTexture(cmd, tempTexture, cameraTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            tempTexture?.Release();
        }
    }

    CombinedRenderPass combinedPass;
    public Material combinedEffectMaterial;
    public bool effectEnabled;

    public override void Create()
    {
        combinedPass = new CombinedRenderPass();
        combinedPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (combinedEffectMaterial != null)
        {
            combinedPass.Setup(combinedEffectMaterial, effectEnabled);
            renderer.EnqueuePass(combinedPass);
        }
    }
}
