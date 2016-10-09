//Renders tiles using an atlas and a texture containing the tile grid data.
//The tiles are assumed to take up 1x1 spaces in world space, starting at the origin.

Shader "EtM 2D/Tile Sprites"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

        _TextureGridTex ("Tile Grid UV Tex", 2D) = "black" {}
	}

	SubShader
	{
		Tags
		{
			"IgnoreProjector"="True" 
			"RenderType"="Opaque" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="False"
		}

		Cull Off
		Lighting Off
		ZWrite Off

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
                float2 worldPos : TEXCOORD1; //TODO: Try using half2 instead.
			};	

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);

#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
#endif

                OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex).xy;

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
            sampler2D _TextureGridTex;

            float4 _TextureGridTex_TexelSize,
                   _MainTex_TexelSize;

			fixed4 SampleSpriteTexture(float2 worldPos)
			{
                //TODO: Try using half instead of float.
                //Sample _TextureGridTex to get the UV rectangle to use for sampling the sprite texture atlas.
                float4 uvRect = tex2D(_TextureGridTex, worldPos * _TextureGridTex_TexelSize.xy);
                float2 t = worldPos - floor(worldPos);
                float2 uv = lerp(uvRect.xy, uvRect.zw, t);
				fixed4 color = tex2D(_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.worldPos);
				return c;
			}
		ENDCG
		}
	}
}
