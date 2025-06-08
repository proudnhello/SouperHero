Shader "Cosmetics/ColorReplacement"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [HideInInspector] _color("Color", Color) = (1.0,1.0,1.0,1.0)

        _FilterColor1("Filter Color 1", Color) = (1.0,0,0,1.0)
        _FilterColor2("Filter Color 2", Color) = (0,1.0,0,1.0) 
        _FilterColor3("Filter Color 3", Color) = (0,0,1.0,1.0)
        _FilterColor4("Filter Color 4", Color) = (1.0,1.0,0,1.0) 
        _FilterColor5("Filter Color 5", Color) = (1.0,0,1.0,1.0)
        _FilterColor6("Filter Color 6", Color) = (1.0,0,1.0,1.0) 


        _ColorReplacement1("ColorReplacement1", Color) = (1.0,0,0,1.0) 
        _Threshold1("Threshold1",Range(0,1)) = 0
        _ColorReplacement2("_ColorReplacement2", Color) = (0,1.0,0,1.0)
        _Threshold2("Threshold2",Range(0,1)) = 0
        _ColorReplacement3("_ColorReplacement3", Color) = (0,0,1.0,1.0) 
        _Threshold3("Threshold3",Range(0,1)) = 0
        _ColorReplacement4("_ColorReplacement4", Color) = (1.0,1.0,0,1.0)
        _Threshold4("Threshold4",Range(0,1)) = 0
        _ColorReplacement5("_ColorReplacement5", Color) = (1.0,0,1.0,1.0)
        _Threshold5("Threshold5",Range(0,1)) = 0
        _ColorReplacement6("_ColorReplacement6", Color) = (1.0,0,1.0,1.0)
        _Threshold6("Threshold6",Range(0,1)) = 0

    }

    SubShader
    {
Tags {"Queue" = "Transparent"} ZWrite Off  Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            fixed4 _color;

            //MainColors
            uniform float4 _FilterColor1;
            uniform float4 _FilterColor2;
            uniform float4 _FilterColor3;
            uniform float4 _FilterColor4;
            uniform float4 _FilterColor5;
            uniform float4 _FilterColor6;

            //Colors Replacement
            uniform float4 _ColorReplacement1;
            uniform float4 _ColorReplacement2;
            uniform float4 _ColorReplacement3;
            uniform float4 _ColorReplacement4;
            uniform float4 _ColorReplacement5;
            uniform float4 _ColorReplacement6;


            uniform float _Threshold1;
            uniform float _Threshold2;
            uniform float _Threshold3;
            uniform float _Threshold4;
            uniform float _Threshold5;
            uniform float _Threshold6;


            float _LerpValue;


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };



            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half distance(float4 c){
            half result = sqrt(pow(_color.r - c.r,2.0)+pow(_color.g - c.g,2.0)+pow(_color.b - c.b,2.0));
            return result;
            }

            fixed4 frag (v2f i) : Color
            {
                _color = tex2D(_MainTex, i.uv);

                if(_color.a<=0.15){
                    return half4(0,0,0,0);
                }

                half dis1 = distance(_FilterColor1);
                fixed InThreshold_1 = dis1 <= _Threshold1 ? 1 : 0;
                _color = _color * (1-InThreshold_1) + lerp(_ColorReplacement1, _color, smoothstep(0, _Threshold1, dis1)) * InThreshold_1;

                half dis2 = distance(_FilterColor2);
                fixed InThreshold_2 = dis2 <= _Threshold2 ? 1 : 0;
                _color = _color * (1-InThreshold_2) + lerp(_ColorReplacement2, _color, smoothstep(0, _Threshold2, dis2)) * InThreshold_2;

                half dis3 = distance(_FilterColor3);
                fixed InThreshold_3 = dis3 <= _Threshold3 ? 1 : 0;
                _color = _color * (1-InThreshold_3) + lerp(_ColorReplacement3, _color, smoothstep(0, _Threshold3, dis3)) * InThreshold_3;

                half dis4 = distance(_FilterColor4);
                fixed InThreshold_4 = dis4 <= _Threshold4 ? 1 : 0;
                _color = _color * (1-InThreshold_4) + lerp(_ColorReplacement4, _color, smoothstep(0, _Threshold4, dis4)) * InThreshold_4;

                half dis5 = distance(_FilterColor5);
                fixed InThreshold_5 = dis5 <= _Threshold5 ? 1 : 0;
                _color = _color * (1-InThreshold_5) + lerp(_ColorReplacement5, _color, smoothstep(0, _Threshold5, dis5)) * InThreshold_5;

                half dis6 = distance(_FilterColor6);
                fixed InThreshold_6 = dis6 <= _Threshold6 ? 1 : 0;
                _color = _color * (1-InThreshold_6) + lerp(_ColorReplacement6, _color, smoothstep(0, _Threshold6, dis6)) * InThreshold_6;

                return _color;
            }
            ENDCG
        }
    }
}