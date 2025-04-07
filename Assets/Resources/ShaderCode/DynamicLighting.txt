// DynamicLightingShader.shader
// Custom surface shader that demonstrates Lambertian diffuse and Blinn-Phong specular lighting
// using a manually defined directional light source and camera view direction.
// Compatible with the Universal Render Pipeline (URP).

Shader "Custom/DynamicLighting"
{
    Properties
    {
        // Base color of the material
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        // Strength of the specular highlight (0 = no specular, 1 = full specular)
        _SpecularStrength ("Specular Strength", Range(0,1)) = 0.5

        // Controls the sharpness of the specular highlight (higher = shinier surface)
        _Shininess ("Shininess", Range(1,100)) = 20
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Vertex input structure
            struct Attributes
            {
                float4 positionOS : POSITION;   // Object-space vertex position
                float3 normalOS : NORMAL;       // Object-space normal vector
            };

            // Vertex-to-fragment interpolated data
            struct Varyings
            {
                float4 positionCS : SV_POSITION; // Clip-space position for rasterization
                float3 normalWS : TEXCOORD0;     // World-space normal
                float3 viewDirWS : TEXCOORD1;    // World-space view direction (camera to surface)
                float3 lightDirWS : TEXCOORD2;   // World-space light direction
            };

            // Material parameters (auto-populated from the Properties block)
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _SpecularStrength;
            float _Shininess;
            CBUFFER_END

            // Vertex shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Transform vertex position to clip space
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);

                // Transform normal to world space
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // Calculate view direction from camera to vertex (in world space)
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - TransformObjectToWorld(IN.positionOS).xyz);

                // Use a fixed directional light direction (world space)
                OUT.lightDirWS = normalize(float3(0.5, 1, 0.2));

                return OUT;
            }

            // Fragment shader
            half4 frag(Varyings IN) : SV_Target
            {
                // Normalize interpolated vectors
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(IN.viewDirWS);
                float3 lightDir = normalize(IN.lightDirWS);

                // Lambertian diffuse lighting
                float diff = max(dot(normal, lightDir), 0.0);
                float3 baseColor = _BaseColor.rgb;
                float3 diffuse = diff * baseColor;

                // Blinn-Phong specular reflection
                float3 halfwayDir = normalize(lightDir + viewDir);
                float spec = pow(max(dot(normal, halfwayDir), 0.0), _Shininess) * _SpecularStrength;

                // Combine diffuse and specular with base color
                float3 finalColor = (diffuse + spec) * baseColor;

                return half4(finalColor, 1.0); // Output final color with full opacity
            }

            ENDHLSL
        }
    }
}
