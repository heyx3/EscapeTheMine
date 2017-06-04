using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;


namespace GameLogic.Units
{
	[Serializable]
	public class Consts : MyData.IReadWritable
	{
		private static Consts instance = new Consts();
		private Consts()
		{
            instance = this;

			//Try to read the constants from a file.
			const string fileName = "UnitConsts.consts";
			string filePath = Path.Combine(Application.dataPath, fileName);
			if (File.Exists(filePath))
			{
				try
				{
					MyData.JSONReader reader = new MyData.JSONReader(filePath);
					reader.Structure(this, "consts");
				}
				catch (MyData.Reader.ReadException e)
				{
					Debug.LogError("Error reading data file \"" + filePath + "\": " +
								   e.Message + "||" + e.StackTrace);
				}
			}
			//If we're in a standalone build and the file doesn't exist, create it.
			else if (!Application.isEditor)
			{
				MyData.JSONWriter writer = null;
				try
				{
					writer = new MyData.JSONWriter(filePath);
					writer.Structure(this, "consts");
				}
				catch (MyData.Writer.WriteException e)
				{
					Debug.LogError("Error writing data file \"" + filePath + "\": " +
									   e.Message + "||" + e.StackTrace);
				}
				finally
				{
					if (writer != null)
						writer.Dispose();
				}
			}
		}


		public static int DefaultMaxEnemyDistSqr { get { return instance.defaultMaxEnemyDistSqr; } }
		public static float EnemyDistanceHeuristicMax { get { return instance.enemyDistHeuristicMax; } }


        #region Private fields

		//The square of the maximum distance an enemy can be while still affecting the A* heuristic.
		private int defaultMaxEnemyDistSqr = 11 * 11;
		//A scale for the effect of an enemy on the A* heuristic.
		private float enemyDistHeuristicMax = 1.0f;

		#endregion


		public void WriteData(MyData.Writer writer)
		{
			writer.Int(defaultMaxEnemyDistSqr, "maxEnemyDistSqr");
			writer.Float(enemyDistHeuristicMax, "enemyDistHeuristicMax");
		}
		public void ReadData(MyData.Reader reader)
		{
			defaultMaxEnemyDistSqr = reader.Int("maxEnemyDistSqr");
			enemyDistHeuristicMax = reader.Float("enemyDistHeuristicMax");
		}
	}
}
