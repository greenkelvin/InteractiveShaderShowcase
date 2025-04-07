// IridescentShader.shader
// A custom URP-compatible shader that simulates iridescence (color shifting effects)
// based on the viewing angle using a Fresnel approximation and procedural hue shifting.

Shader "Custom/IridescentShader"
{
    Properties
    {
        // The base color of the material (used as the underlying tone)
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        // Strength of the iridescent effect (0 = none, 1 = full effect)
        _IridescenceStrength ("Iridescence Strength", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Vertex input structure (object space)
            struct Attributes
            {
                float4 positionOS : POSITION;  // Vertex position in object space
                float3 normalOS : NORMAL;      // Vertex normal in object space
            };

            // Data interpolated from vertex to fragment shader
            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // Homogeneous clip space position
                float3 normalWS : TEXCOORD0;      // Normal in world space
                float3 viewDirWS : TEXCOORD1;     // View direction in world space
            };

            // Material properties (auto-populated by Unity)
            float4 _BaseColor;
            float _IridescenceStrength;

            // Vertex Shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);                          // Transform vertex to clip space
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);                         // Convert normal to world space
                OUT.viewDirWS = GetWorldSpaceViewDir(TransformObjectToWorld(IN.positionOS));      // Compute world-space view direction
                return OUT;
            }

            // Fragment Shader
            float4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);     // Normalize interpolated normal
                float3 viewDir = normalize(IN.viewDirWS);   // Normalize interpolated view direction

                // Fresnel factor: stronger at grazing angles
                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), 2.0);

                // Procedural rainbow colors based on Fresnel
                // The sin-based function approximates hue shifts
                float hue = frac(fresnel * 1.5);  // fractional part to wrap the rainbow
                float3 iridescentColor = saturate(abs(sin(float3(5.0, 3.0, 1.0) * hue * 6.283)));

                // Mix between base color and iridescent color
                float3 finalColor = lerp(_BaseColor.rgb, iridescentColor, _IridescenceStrength);

                return float4(finalColor, _BaseColor.a);  // Output final color with base alpha
            }

            ENDHLSL
        }
    }
}
