using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	public class TileRenderer2D : RendererComponent
	{
		[Serializable]
		public class TileAtlasSlice
		{
			public int TileX = 0,
					   TileY = 0;
			public GameLogic.TileTypes TileType;
		}


		[SerializeField]
		private Material tileRenderMat;
		[SerializeField]
		private Mesh quadMesh;

		[SerializeField]
		private string paramName_TileGridTex = "_tileGridTex";

		[SerializeField]
		private TileAtlasSlice[] tileAtlases = new TileAtlasSlice[0];
		[SerializeField]
		private Texture2D tileAtlasTex;
		[SerializeField]
		private int tileAtlasSize = 32,
					tileAtlasBorder = 0,
					tileAtlasSpacing = 0;

		[SerializeField]
		private TextureFormat[] tileTexFormatsByPriority = new TextureFormat[]
		{
			//TODO: Try to switch these; check whether 16-bit values are acceptable. Make sure the changes propagate to prefabs/scenes.
			TextureFormat.RGBA32, TextureFormat.ARGB32,
			TextureFormat.RGBA4444, TextureFormat.ARGB4444,
		};


		private Texture2D tileGridTex = null;
		private Dictionary<GameLogic.TileTypes, Color> tileTypeToMaterialParam;


		private Camera GameCam { get { return Camera.main; } }


		protected override void Awake()
		{
			base.Awake();

			//Use the best available format for the tile grid texture when initializing it.
			TextureFormat bestFmt = tileTexFormatsByPriority.First(SystemInfo.SupportsTextureFormat);
			tileGridTex = new Texture2D(1, 1, bestFmt, false);
			tileGridTex.filterMode = FilterMode.Point;

			//Set up mesh.
			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf == null)
				mf = gameObject.AddComponent<MeshFilter>();
			mf.sharedMesh = quadMesh;

			//Set up material.
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr == null)
				mr = gameObject.AddComponent<MeshRenderer>();
			mr.material = tileRenderMat;
			mr.material.SetTexture(paramName_TileGridTex, tileGridTex);
			mr.material.mainTexture = tileAtlasTex;

			//Calculate UV sub-rects for each tile type.
			tileTypeToMaterialParam = new Dictionary<GameLogic.TileTypes, Color>();
			Vector2 texel = tileAtlasTex.texelSize;
			Vector2 texel_TileSize = texel * tileAtlasSize,
					texel_Border = texel * tileAtlasBorder,
					texel_Spacing = texel * tileAtlasSpacing;
			for (int i = 0; i < tileAtlases.Length; ++i)
			{
				//Make sure no duplicate tiles exist in the atlas array.
				for (int j = i + 1; j < tileAtlases.Length; ++j)
					if (tileAtlases[i].TileType == tileAtlases[j].TileType)
						Debug.LogError("Tile atlases " + i + " and " + j + " use the same tile type");

				Rect texR = new Rect(texel_Border + (i * (texel_Spacing + texel_TileSize)),
									 texel_TileSize);
				tileTypeToMaterialParam.Add(tileAtlases[i].TileType,
											new Color(texR.xMin, texR.yMin,
													  texR.xMax, texR.yMax));
			}
		}

		public override void StartRendering()
		{
			//Initialize the tile grid texture data.

			tileGridTex.Resize(Tiles.Width, Tiles.Height);
			Color[] cols = new Color[tileGridTex.width * tileGridTex.height];

			for (int y = 0; y < tileGridTex.height; ++y)
				for (int x = 0; x < tileGridTex.width; ++x)
					cols[x + (y * tileGridTex.width)] = tileTypeToMaterialParam[Tiles[new Vector2i(x, y)]];

			tileGridTex.SetPixels(cols);
			tileGridTex.Apply();


			//Set up callbacks.
			Tiles.OnTileChanged += OnTileChanged;
		}
		public override void DestroyRendering()
		{
			//Clean up callbacks.
			Tiles.OnTileChanged -= OnTileChanged;

			base.DestroyRendering();
		}


		private void OnTileChanged(GameLogic.TileGrid tiles, Vector2i pos,
								   GameLogic.TileTypes oldVal, GameLogic.TileTypes newVal)
		{
			tileGridTex.SetPixel(pos.x, pos.y, tileTypeToMaterialParam[newVal]);
			tileGridTex.Apply();
		}
	}
}
