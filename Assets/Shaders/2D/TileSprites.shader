// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "EtM 2D/Tile Sprites"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		//_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

        _TextureGridTex ("Tile Grid UV Tex", 2D) = "black" {}
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

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
				//float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				//fixed4 color    : COLOR;
                float2 worldPos : TEXCOORD1; //TODO: Try using half2 instead.
			};
			
			//fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				//OUT.color = IN.color * _Color;

#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
#endif

                OUT.worldPos = mul(unity_ObjectToWorld, float4(IN.vertex, 1.0)).xy;

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
            sampler2D _TextureGridTex;

			fixed4 SampleSpriteTexture(float2 worldPos)
			{
                //TODO: Try using half instead of float.
                //Sample _TextureGridTex to get the UV rectangle to use for sampling the sprite texture atlas.
                float4 uvRect = tex2D(_TextureGridTex, worldPos * _TextureGridTex_TexelSize.xy);
                float2 t = IN.worldPos - floor(IN.worldPos);
				fixed4 color = tex2D(_MainTex, lerp(uvRect.xy, uvRect.zw, t));

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN);// * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
