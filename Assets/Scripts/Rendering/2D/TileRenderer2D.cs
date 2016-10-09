using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic;
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
		private string paramName_TileGridTex = "_TextureGridTex";

		[SerializeField]
		private TileAtlasSlice[] tileAtlases = new TileAtlasSlice[0];
		[SerializeField]
		private Sprite tileAtlas;
		[SerializeField]
		private int tileAtlasSize = 32,
					tileAtlasBorder = 0,
					tileAtlasSpacing = 0;

		[SerializeField]
		private TextureFormat[] tileTexFormatsByPriority = new TextureFormat[]
		{
			TextureFormat.RGBAFloat,
			//TODO: Try to switch these; check whether 16-bit values are acceptable. Make sure the changes propagate to prefabs/scenes.
			TextureFormat.RGBA32, TextureFormat.ARGB32,
			TextureFormat.RGBA4444, TextureFormat.ARGB4444,
		};


		private Texture2D tileGridTex = null;
		private Dictionary<GameLogic.TileTypes, Color> tileTypeToMaterialParam;
		private GameObject tileQuad = null;


		private Camera GameCam { get { return Camera.main; } }


		protected override void OnEnable()
		{
			base.OnEnable();

			//Use the best available format for the tile grid texture when initializing it.
			TextureFormat bestFmt = tileTexFormatsByPriority.First(SystemInfo.SupportsTextureFormat);
			tileGridTex = new Texture2D(1, 1, bestFmt, false);
			tileGridTex.filterMode = FilterMode.Point;

			if (tileQuad == null)
			{
				tileQuad = new GameObject("Tile Quad");
				tileQuad.transform.SetParent(transform, true);
				tileQuad.SetActive(false);
			}

			//Set up sprite renderer.
			SpriteRenderer sr = tileQuad.GetComponent<SpriteRenderer>();
			if (sr == null)
				sr = tileQuad.AddComponent<SpriteRenderer>();
			sr.material = tileRenderMat;
			sr.material.SetTexture(paramName_TileGridTex, tileGridTex);
			sr.sprite = tileAtlas;

			//Calculate UV sub-rects for each tile type.
			tileTypeToMaterialParam = new Dictionary<GameLogic.TileTypes, Color>();
			Vector2 texel = tileAtlas.texture.texelSize;
			Vector2 texel_TileSize = texel * tileAtlasSize,
					texel_Border = texel * tileAtlasBorder,
					texel_Spacing = texel * tileAtlasSpacing;
			int nTilesY = (tileAtlas.texture.height - tileAtlasBorder - tileAtlasBorder) / tileAtlasSize;
			for (int i = 0; i < tileAtlases.Length; ++i)
			{
				//Make sure no duplicate tiles exist in the atlas array.
				for (int j = i + 1; j < tileAtlases.Length; ++j)
					if (tileAtlases[i].TileType == tileAtlases[j].TileType)
						Debug.LogError("Tile atlases " + i + " and " + j + " use the same tile type");

				Rect texR = new Rect(texel_Border.x + (tileAtlases[i].TileX * (texel_Spacing.x + texel_TileSize.x)),
									 texel_Border.y + ((nTilesY - tileAtlases[i].TileY) * (texel_Spacing.y + texel_TileSize.y)),
									 texel_TileSize.x, texel_TileSize.y);
				tileTypeToMaterialParam.Add(tileAtlases[i].TileType,
											new Color(texR.xMin, texR.yMin,
													  texR.xMax, texR.yMax));
			}
		}
		protected override void OnDisable()
		{
			base.OnDisable();

			tileGridTex.Resize(1, 1);
			tileGridTex.Apply();
			tileGridTex = null;
		}


		protected override void StartMap(Map map)
		{
			base.StartMap(map);

			tileQuad.SetActive(true);
			TileGridResized(map.Tiles, Vector2i.Zero, new Vector2i(map.Tiles.Width, map.Tiles.Height));
		}
		protected override void EndMap(Map map)
		{
			base.EndMap(map);
			
			tileQuad.SetActive(false);
		}

		protected override void TileGridResized(GameLogic.TileGrid tiles,
												Vector2i oldSize, Vector2i newSize)
		{
			base.TileGridResized(tiles, oldSize, newSize);

			Transform quadTr = tileQuad.transform;
			quadTr.localScale = new Vector3(Map.Tiles.Width, Map.Tiles.Height, 1.0f);
			quadTr.position = new Vector3(Map.Tiles.Width * 0.5f,
										  Map.Tiles.Height * 0.5f,
										  quadTr.position.z);

			//Update the texture data.

			tileGridTex.Resize(Tiles.Width, Tiles.Height);

			Color[] cols = new Color[tileGridTex.width * tileGridTex.height];
			for (int y = 0; y < tileGridTex.height; ++y)
				for (int x = 0; x < tileGridTex.width; ++x)
					cols[x + (y * tileGridTex.width)] = tileTypeToMaterialParam[Tiles[new Vector2i(x, y)]];

			tileGridTex.SetPixels(cols);
			tileGridTex.Apply();
			
			GetComponentInChildren<SpriteRenderer>().material.SetTexture(paramName_TileGridTex, tileGridTex);
		}
		protected override void TileChanged(GameLogic.TileGrid tiles, Vector2i pos,
										    GameLogic.TileTypes oldVal, GameLogic.TileTypes newVal)
		{
			base.TileChanged(tiles, pos, oldVal, newVal);

			tileGridTex.SetPixel(pos.x, pos.y, tileTypeToMaterialParam[newVal]);
			tileGridTex.Apply();
		}
	}
}
