// PostProcessingCombined.shader
// Combines color tinting and edge detection in a single fullscreen post-processing pass.
// Used in Unity URP as a hidden post-processing effect.

Shader "Hidden/PostProcessingCombined"
{
    SubShader
    {
        // Disable depth writing and backface culling since this is a fullscreen effect
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        Cull Off

        Pass
        {
            Name "PostProcessingCombinedPass"

            HLSLPROGRAM
            // Include common URP rendering helpers and fullscreen blit utilities
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            // Shader properties passed from C#
            float4 _TintColor;            // RGB tint color
            float _TintIntensity;         // Blend factor for applying tint (0-1)
            float _EdgeThreshold;         // Edge sensitivity for Sobel filter
            float _EnableEdgeDetection;   // Whether edge detection is enabled (1 = on, 0 = off)
            float4 _EdgeColor;            // Color used for detected edges

            // Convert RGB color to grayscale luminance (unused but available)
            float luminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            // Fragment shader
            float4 Frag(Varyings input) : SV_Target0
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                // Calculate size of one pixel in UV space
                float2 texelSize = 1.0 / _BlitTexture_TexelSize.zw;

                // Sample a 3x3 grid of pixels around the current UV
                float3 sample[9];
                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        sample[(y + 1) * 3 + (x + 1)] = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + offset).rgb;
                    }
                }

                // Sobel edge detection kernel (applied separately in X and Y)
                float3 sobelX = sample[2] + 2 * sample[5] + sample[8] - sample[0] - 2 * sample[3] - sample[6];
                float3 sobelY = sample[0] + 2 * sample[1] + sample[2] - sample[6] - 2 * sample[7] - sample[8];

                // Compute edge magnitude from both directions
                float edgeMagnitude = length(sobelX + sobelY);

                // Convert edge magnitude into a hard edge mask
                float edge = smoothstep(_EdgeThreshold - 0.05, _EdgeThreshold + 0.05, edgeMagnitude);

                // Sample the original scene color
                float3 originalColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;

                // Apply a uniform color tint
                float3 tinted = originalColor * lerp(1.0, _TintColor.rgb, _TintIntensity);

                // If edge detection is enabled, overlay the selected edge color
                float3 edgeColor = _EdgeColor.rgb * edge;
                float3 finalColor = (_EnableEdgeDetection > 0.5) ? tinted + edgeColor : tinted;

                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }
}
