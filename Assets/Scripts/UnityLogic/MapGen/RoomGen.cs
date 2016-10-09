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

			int sizeX = tiles.GetLength(0),
				sizeY = tiles.GetLength(1);

			//Calculate the size of each initial room.
			int roomCellSizeX = sizeX / NRooms,
				roomCellSizeY = sizeY / NRooms;
			int roomSizeX = roomCellSizeX - RoomSpacing,
				roomSizeY = roomCellSizeY - RoomSpacing;
			
			float minCircleLerp = Mathf.Lerp(0.5f, 0.0f, CirclePosVariance),
				  maxCircleLerp = Mathf.Lerp(0.5f, 1.0f, CirclePosVariance);

			//Create the initial rooms.
			for (int yStart = 0; (yStart + roomCellSizeY) <= sizeY; yStart += roomCellSizeY)
				for (int xStart = 0; (xStart + roomCellSizeX) <= sizeX; xStart += roomCellSizeX)
				{
					//For the room's biome, get the biome of its center tile.

					int centerX = xStart + (roomCellSizeX / 2),
						centerY = yStart + (roomCellSizeY / 2);

					PRNG prng = new PRNG(centerX, centerY, seed);

					int roomMinX = centerX - (roomSizeX / 2),
						roomMinY = centerY - (roomSizeY / 2),
						roomMaxX = centerX + (roomSizeX / 2),
						roomMaxY = centerY + (roomSizeY / 2);

					outRooms.Add(new Room(tiles[centerX, centerY],
										  new RectI(roomMinX, roomMinY,
													(roomMaxX - roomMinX),
													(roomMaxY - roomMinY))));

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

						Vector2 circlePos = new Vector2(Mathf.Lerp(xStart, xStart + roomCellSizeX,
																   posLerp.x),
														Mathf.Lerp(yStart, yStart + roomCellSizeY,
																   posLerp.y));
						float circleRadius = Mathf.Lerp(CircleMinRadius * roomSizeX,
														CircleMaxRadius * roomSizeY,
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