Shader "Hidden/Grayscale"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        ZTest Always

        Tags 
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline" = "UniversalPipeline" 
        }

        LOD 100

        Pass
        {
            Name "Downscale"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0


            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            Texture2D _MainTex;
            SamplerState point_clamp_sampler;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.uv = v.uv;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float4 color = _MainTex.Sample(point_clamp_sampler, IN.uv);
                fixed BW = dot(fixed4(.3, .59, .11, 0), color);
                fixed4 output = fixed4(BW, BW, BW, color.a);
                
                return output;
            }
        ENDCG
        }
    }
}