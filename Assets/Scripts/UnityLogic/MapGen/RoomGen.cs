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
		/// The min/max number of circles each room uses to carve out its initial space.
		/// </summary>
		public int MinCirclesPerRoom = 3,
				   MaxCirclesPerRoom = 5;
		/// <summary>
		/// The amount of spread (from 0 to 1) that circles have when being placed in a room.
		/// </summary>
		public float CirclePosVariance = 0.75f;
		/// <summary>
		/// The min/max radius of each circle in a room, as a percentage of the room's size.
		/// </summary>
		public float CircleMinRadius = 0.1f,
					 CircleMaxRadius = 0.35f;


		public List<Room> Generate(BiomeTile[,] tiles, int nThreads, int seed)
		{
			List<Room> outRooms = new List<Room>();

			//Generate the initial rooms as rectangles,
			//    then modify them from there with cellular automata.

			//Calculate the size of each initial room.
			Vector2i roomCellSize = tiles.SizeXY() / NRooms;
			Vector2i roomSize = roomCellSize - RoomSpacing;

			float minCircleLerp = Mathf.Lerp(0.5f, 0.0f, CirclePosVariance),
				  maxCircleLerp = Mathf.Lerp(0.5f, 1.0f, CirclePosVariance);

			//Create the initial rooms.
			Vector2i max = tiles.SizeXY() + 1;
			for (int yStart = 0; (yStart + roomCellSize.y) < max.y; yStart += roomCellSize.y)
				for (int xStart = 0; (xStart + roomCellSize.x) < max.x; xStart += roomCellSize.x)
				{
					Vector2i start = new Vector2i(xStart, yStart);

					//For the room's biome, get the biome of its center tile.
					Vector2i center = start + (roomCellSize / 2);

					PRNG prng = new PRNG(center.x, center.y, seed);

					Vector2i roomMin = center - (roomSize / 2),
							 roomMax = center + (roomSize / 2);

					outRooms.Add(new Room(tiles.Get(center), new RectI(roomMin, roomMax)));

					//Carve circular spaces out of the room.
					int nCircles = MinCirclesPerRoom +
								   (prng.NextInt() % (MaxCirclesPerRoom - MinCirclesPerRoom + 1));
					for (int circI = 0; circI < nCircles; ++circI)
					{
						//Choose a random position for the circle centered around the origin.
						Vector2 posLerp = new Vector2(Mathf.Lerp(minCircleLerp, maxCircleLerp,
																 prng.NextFloat()),
													  Mathf.Lerp(minCircleLerp, maxCircleLerp,
																 prng.NextFloat()));

						Vector2 circlePos = new Vector2(Mathf.Lerp(xStart, xStart + roomCellSize.x,
																   posLerp.x),
														Mathf.Lerp(yStart, yStart + roomCellSize.y,
																   posLerp.y));
						float circleRadius = Mathf.Lerp(CircleMinRadius * roomSize.x,
														CircleMaxRadius * roomSize.y,
														prng.NextFloat());

						CarveCircle(outRooms[outRooms.Count - 1], circlePos, circleRadius);
					}
				}

			return outRooms;
		}

		public void CarveCircle(Room room, Vector2 center, float radius)
		{
			float radiusSqr = radius * radius;

			Vector2i minPos = new Vector2i((int)(center.x - radius),
										   (int)(center.y - radius)),
					 maxPos = new Vector2i((int)(center.x + radius) + 1,
										   (int)(center.y + radius) + 1);
			minPos.x = Math.Max(minPos.x, room.OriginalBounds.MinX);
			minPos.y = Math.Max(minPos.y, room.OriginalBounds.MinY);
			maxPos.x = Math.Min(maxPos.x, room.OriginalBounds.MaxX);
			maxPos.y = Math.Min(maxPos.y, room.OriginalBounds.MaxY);

			for (int y = minPos.y; y <= maxPos.y; ++y)
				for (int x = minPos.x; x <= maxPos.x; ++x)
					if (new Vector2i(x, y).DistanceSqr(center) <= radiusSqr)
						room.Spaces.Add(new Vector2i(x, y));
		}


		//Serialization stuff:
		public void ReadData(MyData.Reader reader)
		{
			NRooms = reader.Int("nRooms");
			RoomSpacing = reader.Int("roomSpacing");

			MinCirclesPerRoom = reader.Int("minCirclesPerRoom");
			MaxCirclesPerRoom = reader.Int("maxCirclesPerRoom");
			CirclePosVariance = reader.Float("circlePosVariance");
			CircleMinRadius = reader.Float("circleMinRadius");
			CircleMaxRadius = reader.Float("circleMaxRadius");
	}
		public void WriteData(MyData.Writer writer)
		{
			writer.Int(NRooms, "nRooms");
			writer.Int(RoomSpacing, "roomSpacing");

			writer.Int(MinCirclesPerRoom, "minCirclesPerRoom");
			writer.Int(MaxCirclesPerRoom, "maxCirclesPerRoom");
			writer.Float(CirclePosVariance, "circlePosVariance");
			writer.Float(CircleMinRadius, "circleMinRadius");
			writer.Float(CircleMaxRadius, "circleMaxRadius");
		}
	}
}