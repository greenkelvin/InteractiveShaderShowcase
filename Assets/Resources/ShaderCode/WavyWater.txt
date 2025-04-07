// SimpleWaterShader.shader
// A stylized water shader that simulates surface waves using vertex displacement
// and adds Fresnel-based edge highlighting for a subtle reflective effect.

Shader "Custom/SimpleWaterShader"
{
    Properties
    {
        // Color tint of the water surface
        _BaseColor ("Base Color", Color) = (0, 0.5, 1, 1)

        // Normal map used to simulate small-scale water surface detail
        _NormalMap ("Normal Map", 2D) = "bump" {}

        // Controls the speed of animated wave motion
        _WaveSpeed ("Wave Speed", Float) = 1.0

        // Controls the height of the surface wave displacement
        _WaveStrength ("Wave Strength", Float) = 0.1

        // Controls intensity of the Fresnel effect (edge highlights based on view angle)
        _FresnelPower ("Fresnel Power", Float) = 2.0
    }

    SubShader
    {
        // Transparent rendering with blending enabled
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Vertex input structure
            struct Attributes
            {
                float4 positionOS : POSITION;   // Object space vertex position
                float2 uv : TEXCOORD0;          // Texture coordinates
                float3 normalOS : NORMAL;       // Object space normal
            };

            // Data passed from vertex to fragment shader
            struct Varyings
            {
                float4 positionHCS : SV_POSITION; // Homogeneous clip space position
                float2 uv : TEXCOORD0;            // UVs for sampling textures
                float3 normalWS : TEXCOORD1;      // World space normal
                float3 viewDirWS : TEXCOORD2;     // World space view direction
            };

            // Material properties
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            sampler2D _NormalMap;
            float _WaveSpeed;
            float _WaveStrength;
            float _FresnelPower;
            CBUFFER_END

            // Vertex shader: applies animated wave displacement to the surface
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Simulate wave motion over time
                float time = _Time.y * _WaveSpeed;
                float waveOffset = sin(IN.positionOS.x * 2.0 + time) * cos(IN.positionOS.z * 2.0 + time) * _WaveStrength;

                // Displace the vertex vertically
                IN.positionOS.y += waveOffset;

                // Convert position to clip space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);

                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - TransformObjectToWorld(IN.positionOS).xyz);

                return OUT;
            }

            // Fragment shader: applies color blending and Fresnel edge enhancement
            half4 frag(Varyings IN) : SV_Target
            {
                // Sample the normal map in tangent space (optional visual detail)
                float3 normalTS = UnpackNormal(tex2D(_NormalMap, IN.uv));

                // Calculate Fresnel effect based on view direction and surface normal
                float fresnel = pow(1.0 - saturate(dot(IN.viewDirWS, IN.normalWS)), _FresnelPower);

                // Final color blends base color with Fresnel edge highlight
                float3 finalColor = _BaseColor.rgb + fresnel * 0.3;

                return half4(finalColor, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
