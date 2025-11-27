Shader "Custom/MaskBlend"
{
    Properties
    {
        _MainTex ("Content (A)", 2D) = "white" {}      // content 이미지
        _StyleTex ("Style (B)", 2D) = "white" {}       // ST output
        _MaskTex ("Mask", 2D) = "black" {}             // 0=A, 1=B
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _StyleTex;
            sampler2D _MaskTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 colA = tex2D(_MainTex,  i.uv); // content
                fixed4 colB = tex2D(_StyleTex, i.uv); // style
                fixed  m    = tex2D(_MaskTex,  i.uv).r; // 0~1

                // 마스크 0이면 A, 1이면 B
                return lerp(colA, colB, m);
            }
            ENDCG
        }
    }
}
