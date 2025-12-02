Shader "Custom/TwoImageMaskFeather"
{
    Properties
    {
        _MainTex    ("Base (Image1)", 2D) = "white" {}
        _OverlayTex ("Overlay (Image2)", 2D) = "white" {}
        _MaskTex    ("Mask", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            sampler2D _MaskTex;
            float4    _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                fixed4 overCol = tex2D(_OverlayTex, i.uv);

                // 마스크 0~1 값 그대로 사용
                fixed  m = tex2D(_MaskTex, i.uv).r;

                return lerp(baseCol, overCol, m);
            }
            ENDCG
        }
    }
}
