// RefractionShader.shader
// A transparent shader that simulates refraction by sampling the screen texture and offsetting based on the surface normal.
// Designed for use with the Universal Render Pipeline (URP) and compatible with the _CameraOpaqueTexture.

Shader "Custom/RefractionShader"
{
    Properties
    {
        // Base color tint applied to the surface
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        // Controls the blend between refracted background and the base color
        _Transparency ("Transparency", Range(0, 1)) = 0.5

        // Controls how strongly the normal affects distortion
        _RefractionStrength ("Refraction Strength", Range(0, 0.1)) = 0.02
    }
    
    SubShader
    {
        // Renders in the transparent queue and disables depth writing
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Name "Refraction"
            Tags { "LightMode"="UniversalForward" }

            // Enables alpha blending and disables ZWrite for transparency
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Input structure from mesh
            struct Attributes
            {
                float4 positionOS : POSITION; // Object space position
                float2 uv : TEXCOORD0;        // UVs for optional use
                float3 normalOS : NORMAL;     // Object space normal
            };

            // Data passed to the fragment shader
            struct Varyings
            {
                float4 positionCS : SV_POSITION; // Clip space position
                float2 uv : TEXCOORD0;           // UVs
                float3 normalWS : TEXCOORD1;     // World space normal
                float4 screenPos : TEXCOORD2;    // Screen-space position for sampling _CameraOpaqueTexture
            };

            // Material parameters
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Transparency;
            float _RefractionStrength;
            CBUFFER_END

            // Access to the camera's opaque render texture
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            // Vertex Shader: Transforms mesh data into clip space and calculates screen coordinates
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS); // Required for _CameraOpaqueTexture sampling
                return OUT;
            }

            // Fragment Shader: Computes the refracted color based on screen UVs and the surface normal
            half4 frag(Varyings IN) : SV_Target
            {
                // Convert clip space to normalized screen space
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                // Use the world-space normal's XY as a refraction offset
                float2 refractionOffset = normalize(IN.normalWS.xy) * _RefractionStrength;

                // Sample the scene color using the offset to simulate distortion
                half4 refractedColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + refractionOffset);

                // Blend the distorted background color with the material's base color using transparency
                half4 baseColor = _BaseColor;
                half4 finalColor = lerp(refractedColor, baseColor, _Transparency);

                return finalColor;
            }

            ENDHLSL
        }
    }
}
