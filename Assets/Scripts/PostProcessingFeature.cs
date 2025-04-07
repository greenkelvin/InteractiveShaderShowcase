using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Custom ScriptableRendererFeature for injecting a post-processing material into the URP render pipeline.
/// This feature applies full-screen effects (e.g., tint, edge detection) after the main rendering is done.
/// </summary>
public class PostProcessingFeature : ScriptableRendererFeature
{
    // The material used for the post-processing effect (uses a shader like Hidden/PostProcessingCombined)
    public Material postProcessingMaterial;

    // Controls when the post-processing pass is executed during the render pipeline
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

    // Internal reference to the render pass instance
    PostProcessingPass m_Pass;

    /// <summary>
    /// Called once when the renderer feature is initialized.
    /// Instantiates the post-processing pass and sets its render timing.
    /// </summary>
    public override void Create()
    {
        m_Pass = new PostProcessingPass();
        m_Pass.renderPassEvent = renderPassEvent;
    }

    /// <summary>
    /// Adds the custom post-processing pass to the renderer if a valid material is assigned.
    /// </summary>
    /// <param name="renderer">The ScriptableRenderer that handles the camera rendering.</param>
    /// <param name="renderingData">The current rendering context for the camera.</param>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (postProcessingMaterial == null)
        {
            Debug.LogWarning("PostProcessingFeature: Material is null, skipping effect.");
            return;
        }

        // Set up and enqueue the custom render pass
        m_Pass.Setup(postProcessingMaterial);
        renderer.EnqueuePass(m_Pass);
    }
}
