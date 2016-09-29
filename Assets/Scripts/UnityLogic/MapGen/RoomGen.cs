using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnityLogic.MapGen
{
	public class Room
	{
		public HashSet<Vector2i> Spaces = new HashSet<Vector2i>();
		public RectI OriginalBounds;
		public BiomeTile Biome;

		public Room(BiomeTile biome, RectI bounds)
		{
			Biome = biome;
			OriginalBounds = bounds;
		}
	}

	[Serializable]
	public class RoomGenSettings : MyData.IReadWritable
	{
		/// <summary>
		/// The target number of rooms to initially generate along each axis.
		/// If the map is too small, fewer rooms will be generated.
		/// </summary>
		public int NRooms = 5;
		/// <summary>
		/// The spacing between the rooms when first generated.
		/// </summary>
		public int RoomSpacing = 10;
		/// <summary>
		/// The space taken up by the rooms when first generated,
		///		from 0 to 1.
		/// </summary>
		public float RoomSize = 0.5f;

		/// <summary>
		/// The number of cellular automata iterations to run.
		/// </summary>
		public int NIterations = 10;

		/// <summary>
		/// How much varation there is between tiles in a room.
		/// Should be between 0 and 1.
		/// </summary>
		public float TileVariation = 0.5f;

		/// <summary>
		/// The chance of a tile changing from its current state to the opposite state
		///     given the number of similar tiles surrounding it.
		/// </summary>
		public float[] TileChangeChances = new float[9]
		{
			0.95f, // 0 similar tiles
			0.85f, // 1 similar tiles
			0.75f, // 2 similar tiles
			0.65f, // 3 similar tiles
			0.55f, // 4 similar tiles
			0.35f, // 5 similar tiles
			0.25f, // 6 similar tiles
			0.15f, // 7 similar tiles
			0.05f, // 8 similar tiles
		};


		public List<Room> Generate(BiomeTile[,] tiles)
		{
			PRNG prng = new PRNG(UnityEngine.Random.Range(0, int.MaxValue));
			List<Room> outRooms = new List<Room>();

			//Generate the initial rooms as rectangles,
			//    then modify them from there with cellular automata.

			int sizeX = tiles.GetLength(0),
				sizeY = tiles.GetLength(1);

			//Calculate the size of each initial room.
			int oneRoomFullX = sizeX / NRooms,
				oneRoomFullY = sizeY / NRooms;
			int roomSizeX = (int)((oneRoomFullX - RoomSpacing) * RoomSize),
				roomSizeY = (int)((oneRoomFullY - RoomSpacing) * RoomSize);

			//Create the initial rooms.
			for (int yStart = 0; (yStart + roomSizeY) <= sizeY; yStart += roomSizeY)
				for (int xStart = 0; (xStart + roomSizeX) <= NRooms; xStart += roomSizeX)
				{
					//For the room's biome, get the biome of its center tile.

					int centerX = xStart + (roomSizeX / 2),
						centerY = yStart + (roomSizeY / 2);

					outRooms.Add(new Room(tiles[centerX, centerY],
										  new RectI(xStart, yStart, roomSizeX, roomSizeY)));

					for (int roomX = xStart; roomX < (xStart + roomSizeX); ++roomX)
						for (int roomY = yStart; roomY < (yStart + roomSizeY); ++roomY)
							outRooms[outRooms.Count - 1].Spaces.Add(new Vector2i(roomX, roomY));
				}

			//Mutate the rooms using several iterations of a cellular automata.
			for (int iterI = 0; iterI < NIterations; ++iterI)
			{
				foreach (Room r in outRooms)
				{
					//Get a copy of the tiles currently in the room
					//    so that changes in this iteration don't apply until the next iteration.
					HashSet<Vector2i> currentSpaces = new HashSet<Vector2i>(r.Spaces);

					for (int tileY = r.OriginalBounds.MinY; tileY <= r.OriginalBounds.MaxY; ++tileY)
					{
						for (int tileX = r.OriginalBounds.MinX; tileX <= r.OriginalBounds.MaxX; ++tileX)
						{
							Vector2i tilePos = new Vector2i(tileX, tileY);

							//Use a blend between the room's biome and the tile's own biome.
							BiomeTile tileBiome = tiles[tileX, tileY];
							tileBiome = new BiomeTile(r.Biome, tileBiome, TileVariation);

							//Get the number of similar tiles nearby.
							bool minXEdge = (tileX == 0),
								 minYEdge = (tileY == 0),
								 maxXEdge = (tileX == (tiles.GetLength(0) - 1)),
								 maxYEdge = (tileY == (tiles.GetLength(1) - 1));
							bool isInRoom = currentSpaces.Contains(tilePos);
							int similarTiles = 0;
							if (minXEdge || (isInRoom == currentSpaces.Contains(tilePos.LessX)))
								similarTiles += 1;
							if (minYEdge || (isInRoom == currentSpaces.Contains(tilePos.LessY)))
								similarTiles += 1;
							if (maxXEdge || (isInRoom == currentSpaces.Contains(tilePos.MoreX)))
								similarTiles += 1;
							if (maxYEdge || (isInRoom == currentSpaces.Contains(tilePos.MoreY)))
								similarTiles += 1;
							if (minXEdge || minYEdge || (isInRoom == currentSpaces.Contains(tilePos.LessX.LessY)))
								similarTiles += 1;
							if (minXEdge || maxYEdge || (isInRoom == currentSpaces.Contains(tilePos.LessX.MoreY)))
								similarTiles += 1;
							if (maxXEdge || minYEdge || (isInRoom == currentSpaces.Contains(tilePos.MoreX.LessY)))
								similarTiles += 1;
							if (maxXEdge || maxYEdge || (isInRoom == currentSpaces.Contains(tilePos.MoreX.MoreY)))
								similarTiles += 1;

							//Get the odds of this tile switching its membership in the room.
							//Modify the chance based on the biome.
							float chanceOfSwitch = TileChangeChances[similarTiles];
							chanceOfSwitch = Mathf.Lerp(chanceOfSwitch,
														(similarTiles > 4 ? 0.0f : 1.0f),
														tileBiome.CaveSmoothness);

							if (prng.NextFloat() < chanceOfSwitch)
								if (isInRoom)
									r.Spaces.Remove(tilePos);
								else
									r.Spaces.Add(tilePos);
						}
					}
				}
			}

			return outRooms;
		}

		//Serialization stuff:
		public void ReadData(MyData.Reader reader)
		{
			NRooms = reader.Int("nRooms");
			RoomSpacing = reader.Int("roomSpacing");
			RoomSize = reader.Float("roomSize");

			NIterations = reader.Int("nIterations");

			TileVariation = reader.Float("tileVariation");

			TileChangeChances = reader.Collection(
				"tileChangeChances",
				(MyData.Reader r, ref float outVal, string name) =>
					{ outVal = r.Float(name); },
				(size) => new float[size]);
		}
		public void WriteData(MyData.Writer writer)
		{
			writer.Int(NRooms, "nRooms");
			writer.Int(RoomSpacing, "roomSpacing");
			writer.Float(RoomSize, "roomSize");

			writer.Int(NIterations, "nIterations");

			writer.Float(TileVariation, "tileVariation");

			writer.Collection(TileChangeChances, "tileChangeChances",
							  (MyData.Writer w, float val, string name) =>
								  { w.Float(val, name); });
		}
	}
}