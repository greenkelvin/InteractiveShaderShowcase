// ProceduralMarble.shader
// A custom URP-compatible shader that procedurally generates a marble texture.
// Uses layered sine and noise patterns to simulate marble veins without any textures.

Shader "Custom/ProceduralMarble"
{
    Properties
    {
        // The main base color of the marble
        _Color("Base Color", Color) = (1,1,1,1)

        // The color used for the marble veins
        _VeinColor("Vein Color", Color) = (0,0,0,1)

        // Controls the tiling of the pattern
        _Scale("Scale", Float) = 5.0

        // Controls the amount of noise distortion
        _Intensity("Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Vertex input structure
            struct Attributes
            {
                float4 positionOS : POSITION; // Object-space position
                float2 uv : TEXCOORD0;        // UV coordinates
            };

            // Vertex to fragment interpolators
            struct Varyings
            {
                float4 positionCS : SV_POSITION; // Clip-space position
                float2 uv : TEXCOORD0;           // UV coordinates
            };

            // Vertex Shader: Transforms vertex position to clip space and passes UV
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            // Helper function: Generates pseudo-random float from 2D position
            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Gradient noise function based on grid interpolation
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f); // Smoothstep interpolation

                // Bilinear interpolation of random values
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            // Material properties
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float4 _VeinColor;
            float _Scale;
            float _Intensity;
            CBUFFER_END

            // Fragment Shader: Computes marble pattern using sine and noise
            float4 frag(Varyings IN) : SV_Target
            {
                // Scale UVs to tile the pattern
                float2 uv = IN.uv * _Scale;

                // Generate noise for warping the pattern
                float noiseValue = noise(uv);

                // Apply a sine wave to create the "marble" streaks
                float marble = sin(uv.x + uv.y + noiseValue * _Intensity);

                // Use smoothstep to control the threshold for vein visibility
                marble = smoothstep(0.3, 0.7, marble);

                // Blend between base color and vein color based on pattern
                return lerp(_Color, _VeinColor, marble);
            }

            ENDHLSL
        }
    }
}
