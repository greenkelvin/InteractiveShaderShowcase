Shader "Custom/PrototypeShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveAmplitude ("Wave Amplitude", Float) = 0.1
        _WaveFrequency ("Wave Frequency", Float) = 2.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _WaveAmplitude;
            float _WaveFrequency;
            float4 _MainTex_ST;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                // Simple sine-wave displacement along the Y-axis
                float wave = sin(v.vertex.x * _WaveFrequency + _Time.y) * _WaveAmplitude;
                v.vertex.y += wave;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
