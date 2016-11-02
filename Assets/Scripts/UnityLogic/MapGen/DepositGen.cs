using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace UnityLogic.MapGen
{
	[Serializable]
	public class DepositGenSettings : MyData.IReadWritable
	{
		public NoiseOctaves Noise = new NoiseOctaves(1, 0.1f, 0.5f);
		public float Exponent = 1.25f,
					 Threshold = 0.8f;

		public bool[,] Generate(int sizeX, int sizeY, int nThreads, int seed)
		{
			bool[,] tiles = new bool[sizeX, sizeY];

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

							float val = Noise.Sample(new Vector3(xF, yF, seedF), sampler);
							val = Mathf.Pow(val, Exponent);
							tiles[x, y] = (val > Threshold);
						}
					}
				});

			return tiles;
		}

		//Serialization stuff:
		public void ReadData(MyData.Reader reader)
		{
			reader.Structure(Noise, "noise");
			Exponent = reader.Float("exponent");
			Threshold = reader.Float("threshold");
		}
		public void WriteData(MyData.Writer writer)
		{
			writer.Structure(Noise, "noise");
			writer.Float(Exponent, "exponent");
			writer.Float(Threshold, "threshold");
		}
	}
}
