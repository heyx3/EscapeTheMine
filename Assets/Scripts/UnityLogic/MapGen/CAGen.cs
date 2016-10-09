using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnityLogic.MapGen
{
	/// <summary>
	/// Runs a Cellular Automata on the map to finalize the tiles.
	/// </summary>
	[Serializable]
	public class CAGenSettings : MyData.IReadWritable
	{
		/// <summary>
		/// The number of iterations to run.
		/// </summary>
		public int NIterations = 15;

		/// <summary>
		/// How much varation there is between tiles in a room.
		/// Should be between 0 and 1.
		/// </summary>
		public float TileVariation = 0.5f;

		/// <summary>
		/// The chance of a tile changing from its current state to the opposite state
		///     given the number of similar tiles surrounding it.
		/// 0.0 on the X of the curve represents the chance of changing when there are no similar tiles nearby.
		/// 1.0 represents the chance of changing when all tiles nearby are similar.
		/// </summary>
		public AnimationCurve TileChangeChances = new AnimationCurve(
			new Keyframe(0.0f, 0.975f, 0.0f, 0.0f),
			new Keyframe(0.8f, 0.0f, -4.05f, -4.05f));


		/// <summary>
		/// Returns the tiles as booleans indicating whether they're solid.
		/// </summary>
		public bool[,] Generate(BiomeTile[,] biomes, List<Room> rooms, int nThreads, int seed)
		{
			//Use a blend between a room's biome and the tile's own biome.
			BiomeTile[,] oldBiomes = biomes;
			biomes = new BiomeTile[oldBiomes.GetLength(0), oldBiomes.GetLength(1)];
			for (int y = 0; y < biomes.GetLength(1); ++y)
				for (int x = 0; x < biomes.GetLength(0); ++x)
				{
					Room r = rooms.FirstOrDefault(_r => _r.Spaces.Contains(new Vector2i(x, y)));
					if (r == null)
						biomes[x, y] = oldBiomes[x, y];
					else
						biomes[x, y] = new BiomeTile(r.Biome, oldBiomes[x, y], TileVariation);
				}

			//Turn the room data into a grid of tiles.
			bool[,] tiles1 = new bool[biomes.GetLength(0), biomes.GetLength(1)];
			for (int y = 0; y < tiles1.GetLength(1); ++y)
				for (int x = 0; x < tiles1.GetLength(0); ++x)
					tiles1[x, y] = !rooms.Any(r => r.Spaces.Contains(new Vector2i(x, y)));
			//Create a second copy to ping-pong back and forth between iterations.
			bool[,] tiles2 = new bool[tiles1.GetLength(0), tiles1.GetLength(1)];
			for (int y = 0; y < tiles2.GetLength(1); ++y)
				for (int x = 0; x < tiles2.GetLength(0); ++x)
					tiles2[x, y] = tiles1[x, y];

			//Run the cellular automata.
			for (int i = 0; i < NIterations; ++i)
			{
				ThreadedRunner.Run(nThreads, tiles1.GetLength(1),
					(startI, endI) => RunIteration(startI, endI, biomes, seed, tiles1, tiles2));

				//Swap tiles1 and tiles2.
				bool[,] _tiles1 = tiles1;
				tiles1 = tiles2;
				tiles2 = _tiles1;
			}

			return tiles1;
		}
		public void RunIteration(int firstY, int lastY, BiomeTile[,] biomes,
								 int seed, bool[,] inTiles, bool[,] outTiles)
		{
			for (int tileY = firstY; tileY <= lastY; ++tileY)
			{
				for (int tileX = 0; tileX < outTiles.GetLength(0); ++tileX)
				{
					//Get the number of similar tiles nearby.
					bool minXEdge = (tileX == 0),
						 minYEdge = (tileY == 0),
						 maxXEdge = (tileX >= outTiles.GetLength(0)),
						 maxYEdge = (tileY >= outTiles.GetLength(1));
					bool isSolid = inTiles[tileX, tileY];
					int similarTiles = 0;
					if (CheckMinX(tileX, tileY, inTiles, isSolid))
						similarTiles += 1;
					if (CheckMinY(tileX, tileY, inTiles, isSolid))
						similarTiles += 1;
					if (CheckMaxX(tileX, tileY, inTiles, isSolid))
						similarTiles += 1;
					if (CheckMaxY(tileX, tileY, inTiles, isSolid))
						similarTiles += 1;
					if (CheckMinXY(tileX, tileY, inTiles, isSolid))
						similarTiles += 1;
					if (CheckMinXMaxY(tileX, tileY, inTiles, isSolid))
						similarTiles += 1;
					if (CheckMaxXMinY(tileX, tileY, inTiles, isSolid))
						similarTiles += 1;
					if (CheckMaxXY(tileX, tileY, inTiles, isSolid))
						similarTiles += 1;

					//Get the odds of this tile switching its membership in the room.
					//Modify the chance based on the biome.
					float chanceOfSwitch = TileChangeChances.Evaluate(similarTiles / 8.0f);
					chanceOfSwitch = Mathf.Lerp(chanceOfSwitch,
												(similarTiles >= 4 ? 0.0f : 1.0f),
												biomes[tileX, tileY].CaveSmoothness);

					//Either flip the tile or leave it unchanged.
					if (new PRNG(tileX, tileY, seed).NextFloat() < chanceOfSwitch)
						outTiles[tileX, tileY] = !inTiles[tileX, tileY];
					else
						outTiles[tileX, tileY] = inTiles[tileX, tileY];
				}
			}
		}
		private bool CheckMinX(int x, int y, bool[,] tiles, bool isSolid)
		{
			if (x == 0)
				return !isSolid;
			else
				return isSolid == tiles[x - 1, y];
		}
		private bool CheckMinY(int x, int y, bool[,] tiles, bool isSolid)
		{
			if (y == 0)
				return !isSolid;
			else
				return isSolid == tiles[x, y - 1];
		}
		private bool CheckMaxX(int x, int y, bool[,] tiles, bool isSolid)
		{
			if (x == tiles.GetLength(0) - 1)
				return !isSolid;
			else
				return isSolid == tiles[x + 1, y];
		}
		private bool CheckMaxY(int x, int y, bool[,] tiles, bool isSolid)
		{
			if (y == tiles.GetLength(1) - 1)
				return !isSolid;
			else
				return isSolid == tiles[x, y + 1];
		}
		private bool CheckMinXY(int x, int y, bool[,] tiles, bool isSolid)
		{
			if (x == 0 || y == 0)
				return !isSolid;
			else
				return isSolid == tiles[x - 1, y - 1];
		}
		private bool CheckMinXMaxY(int x, int y, bool[,] tiles, bool isSolid)
		{
			if (x == 0 || y == tiles.GetLength(1) - 1)
				return !isSolid;
			else
				return isSolid == tiles[x - 1, y + 1];
		}
		private bool CheckMaxXMinY(int x, int y, bool[,] tiles, bool isSolid)
		{
			if (x == tiles.GetLength(0) - 1 || y == 0)
				return !isSolid;
			else
				return isSolid == tiles[x + 1, y - 1];
		}
		private bool CheckMaxXY(int x, int y, bool[,] tiles, bool isSolid)
		{
			if (x == tiles.GetLength(0) - 1 || y == tiles.GetLength(1) - 1)
				return !isSolid;
			else
				return isSolid == tiles[x + 1, y + 1];
		}


		//Serialization stuff:

		public void WriteData(MyData.Writer writer)
		{
			writer.Int(NIterations, "nIterations");
			writer.Float(TileVariation, "tileVariation");

			KeyframeSerializer kfs = new KeyframeSerializer();
			writer.Collection(
				TileChangeChances.keys, "tileChangeChances",
				(MyData.Writer w, Keyframe k, string name) =>
					{ kfs.Kf = k; w.Structure(kfs, name); });
		}
		public void ReadData(MyData.Reader reader)
		{
			NIterations = reader.Int("nIterations");
			TileVariation = reader.Float("tileVariation");

			KeyframeSerializer kfs = new KeyframeSerializer();
			Keyframe[] keyframes = reader.Collection(
				"tileChangeChances",
				(MyData.Reader r, ref Keyframe outVal, string name) =>
					{ r.Structure(kfs, name); outVal = kfs.Kf; },
				(size) => new Keyframe[size]);
			TileChangeChances.keys = keyframes;
		}

		/// <summary>
		/// Helper class for serializing AnimationCurve keyframes.
		/// </summary>
		private class KeyframeSerializer : MyData.IReadWritable
		{
			public Keyframe Kf;
			public void WriteData(MyData.Writer writer)
			{
				writer.Float(Kf.time, "t");
				writer.Float(Kf.value, "val");
				writer.Float(Kf.inTangent, "inTangent");
				writer.Float(Kf.outTangent, "outTangent");
			}
			public void ReadData(MyData.Reader reader)
			{
				Kf = new Keyframe();
				Kf.time = reader.Float("t");
				Kf.value = reader.Float("val");
				Kf.inTangent = reader.Float("inTangent");
				Kf.outTangent = reader.Float("outTangent");
			}
		}
	}
}
