Shader "Portal/PortalShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
		_Cutout("Cutout Texture", 2D) = "clear" {}
		_Overlay("Overlay Texture", 2D) = "black" {}
		[MaterialToggle] _Crop("Crop", Float) = 0
		_Top("Top UVs", Vector) = (0, 1, 1, 1)
		_Bottom("Bottom UVs", Vector) = (0, 0, 1, 0)
		_Attenuation("Attenuation", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Portal" "Queue" = "Transparent" }
        LOD 100
		Cull Off

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
			sampler2D _Cutout;
			sampler2D _Overlay;
			float4 _MainTex_ST;
			float _Crop;
			float4 _Top;
			float4 _Bottom;
			float _Attenuation;

            v2f vert (appdata v)
            {
                v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float a = tex2D(_Cutout, i.uv).a;
				fixed4 c = tex2D(_Overlay, i.uv);
				c = fixed4(c.rgb * 2, c.a);
				if (_Crop) {
					float2 top = _Top.xy + i.uv.x * (_Top.zw - _Top.xy);
					float2 bottom = _Bottom.xy + i.uv.x * (_Bottom.zw - _Bottom.xy);
					float2 uv = top + (bottom - top) * i.uv.y;
					c = (c.a) * c + (1 - c.a) * tex2D(_MainTex, uv) * _Attenuation;
				}
				else {
					c = (c.a) * c + (1 - c.a) * tex2D(_MainTex, i.screenPos.xy / i.screenPos.w);
				}
				c.a = a;
				return c;
            }
            ENDCG
        }
    }
}
