using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Experimental.Rendering;

public class PostProcessingPass : ScriptableRenderPass
{
    const string m_PassName = "PostProcessingPass";
    private Material m_PostProcessMaterial;

    public void Setup(Material mat)
    {
        m_PostProcessMaterial = mat;
        requiresIntermediateTexture = true;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // Debug.Log("🔥 RecordRenderGraph executed for PostProcessingPass!");

        UniversalResourceData resourceData;
        try
        {
            resourceData = frameData.Get<UniversalResourceData>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Failed to retrieve UniversalResourceData: {e.Message}");
            return;
        }

        TextureHandle source = resourceData.activeColorTexture;
        if (!source.IsValid())
        {
            Debug.LogError("❌ PostProcessingPass: Source texture is invalid.");
            return;
        }

        TextureDesc sourceDesc = renderGraph.GetTextureDesc(source);
        TextureDesc destinationDesc = new TextureDesc(sourceDesc.width, sourceDesc.height)
        {
            colorFormat = GraphicsFormat.R8G8B8A8_UNorm, // Standard color format
            clearBuffer = false,
            enableRandomWrite = false
        };

        TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

        RenderGraphUtils.BlitMaterialParameters para = new(source, destination, m_PostProcessMaterial, 0);
        renderGraph.AddBlitPass(para, passName: m_PassName);

        resourceData.cameraColor = destination;
    }
}
