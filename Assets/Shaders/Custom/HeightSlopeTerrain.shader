Shader "Custom/HeightSlopeTerrain"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _SlopeTex ("Slope Texture", 2D) = "white" {}
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _BottomColor ("Bottom Color", Color) = (0,1,0,1)
        _MinHeight ("Min Height", Float) = 0
        _MaxHeight ("Max Height", Float) = 10
        _SlopeThreshold ("Slope Threshold", Float) = 0.7
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SlopeTex;
            float4 _TopColor;
            float4 _BottomColor;
            float _MinHeight;
            float _MaxHeight;
            float _SlopeThreshold;
            // Add these 👇
            float4 _MainTex_ST;
            float4 _SlopeTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = worldPos.xyz;
                o.worldNormal = normalize(mul(v.normal, (float3x3)unity_ObjectToWorld));
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float inverseLerp(float a, float b, float v)
            {
                return saturate((v - a) / (b - a));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float height = i.worldPos.y;
                float slope = 1.0 - dot(i.worldNormal, float3(0,1,0)); // 0 = flat, 1 = vertical

                // Blend by height
                float heightFactor = inverseLerp(_MinHeight, _MaxHeight, height);
                float4 heightColor = lerp(_BottomColor, _TopColor, heightFactor);

                // Blend by slope
                float slopeBlend = smoothstep(_SlopeThreshold, 1.0, slope);
                float2 baseUV = i.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                float2 slopeUV = i.uv * _SlopeTex_ST.xy + _SlopeTex_ST.zw;

                float4 baseTex = tex2D(_MainTex, baseUV);
                float4 slopeTex = tex2D(_SlopeTex, slopeUV);

                // Mix textures and color
                float4 texBlend = lerp(baseTex, slopeTex, slopeBlend);
                return texBlend * heightColor;
            }
            ENDCG
        }
    }
}
