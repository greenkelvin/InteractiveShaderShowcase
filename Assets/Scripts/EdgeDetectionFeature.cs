using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EdgeDetectionFeature : ScriptableRendererFeature
{
    class EdgeDetectionPass : ScriptableRenderPass
    {
        private Material edgeMaterial;
        private RTHandle tempColorTexture;
        private bool isActive;

        public void Setup(Material material, bool enable)
        {
            edgeMaterial = material;
            isActive = enable;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempColorTexture, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_EdgeDetectTemp");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!isActive || edgeMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("EdgeDetectionPass");
            RTHandle cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            Blitter.BlitCameraTexture(cmd, cameraTarget, tempColorTexture, edgeMaterial, 0);
            Blitter.BlitCameraTexture(cmd, tempColorTexture, cameraTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            tempColorTexture?.Release();
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material edgeDetectMaterial;
        public bool enableEffect = false;
    }

    public Settings settings = new Settings();
    private EdgeDetectionPass edgePass;

    public override void Create()
    {
        edgePass = new EdgeDetectionPass();
        edgePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        edgePass.Setup(settings.edgeDetectMaterial, settings.enableEffect);
        renderer.EnqueuePass(edgePass);
    }
}
