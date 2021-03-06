﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnityLogic.MapGen
{
	/// <summary>
	/// Environmental information for a single tile as part of a larger biome.
	/// </summary>
	public struct BiomeTile
	{
		/// <summary>
		/// How smooth caverns are in this area, from 0 to 1.
		/// </summary>
		public float CaveSmoothness;

		public BiomeTile(float caveSmoothness)
		{
			CaveSmoothness = caveSmoothness;
		}
		public BiomeTile(BiomeTile a, BiomeTile b, float t)
		{
			CaveSmoothness = Mathf.Lerp(a.CaveSmoothness, b.CaveSmoothness, t);
		}
	}

	[Serializable]
	public class BiomeGenSettings : MyData.IReadWritable
	{
		public NoiseOctaves Noise = new NoiseOctaves(3, 0.1f, 0.5f);

		public BiomeTile[,] Generate(int sizeX, int sizeY, int nThreads, int seed)
		{
			BiomeTile[,] tiles = new BiomeTile[sizeX, sizeY];

			Func<Vector3, float, float> sampler = (pos, scale) =>
				NoiseAlgos3D.LinearNoise(pos * scale);

			float seedF = (float)seed;
			ThreadedRunner.Run(nThreads, sizeY,
				(startY, endY) =>
				{
					for (int y = startY; y <= endY; ++y)
					{
						float yF = (float)y;
						for (int x = 0; x < sizeX; ++x)
						{
							float xF = (float)x;

							tiles[x, y] = new BiomeTile();
							tiles[x, y].CaveSmoothness = Noise.Sample(new Vector3(xF, yF, seedF),
																	  sampler);
						}
					}
				});

			return tiles;
		}

		//Serialization stuff:
		public void ReadData(MyData.Reader reader)
		{
			reader.Structure(Noise, "noise");
		}
		public void WriteData(MyData.Writer writer)
		{
			writer.Structure(Noise, "noise");
		}
	}
}
