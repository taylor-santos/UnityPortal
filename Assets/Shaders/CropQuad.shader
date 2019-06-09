Shader "Unlit/CropQuad"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Top     ("Top UVs", Vector)    = (0, 1, 1, 1)
		_Bottom  ("Bottom UVs", Vector) = (0, 0, 1, 0)
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Top;
			float4 _Bottom;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				float2 top = _Top.xy + i.uv.x * (_Top.zw - _Top.xy);
				float2 bottom = _Bottom.xy + i.uv.x * (_Bottom.zw - _Bottom.xy);
				float2 uv = bottom + (top - bottom) * i.uv.y;
                fixed4 col = tex2D(_MainTex, uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
