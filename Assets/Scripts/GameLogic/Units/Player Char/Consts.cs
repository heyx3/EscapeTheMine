using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;


namespace GameLogic.Units.Player_Char
{
	[Serializable]
	public class Consts : MyData.IReadWritable
	{
		private static Consts instance = new Consts();
		private Consts()
		{
			//Try to read the constants from a file.
			const string fileName = "PlayerConsts.consts";
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

			//Set pre-computed fields.
			One_Over_MaxEnemyDistSqr = 1.0f / MaxEnemyDistSqr;

            instance = this;
		}


		public static float MinStart_Food { get { return instance.minStart_Food; } }
		public static float MaxStart_Food { get { return instance.maxStart_Food; } }

		public static float MinStart_Energy { get { return instance.minStart_Energy; } }
		public static float MaxStart_Energy { get { return instance.maxStart_Energy; } }

		public static float MinStart_Strength { get { return instance.minStart_Strength; } }
		public static float MaxStart_Strength { get { return instance.maxStart_Strength; } }

		public static float Max_Health { get { return instance.max_health; } }

		public static float InitialLowFoodThreshold { get { return instance.initialLowFoodThreshold; } }

		public static float MaxFood(float strengthStat) { return instance.maxFood.Evaluate(strengthStat); }
		public static float MaxEnergy(float strengthStat) { return instance.maxEnergy.Evaluate(strengthStat); }

		public static float FoodLossPerTurn(float strengthStat) { return instance.foodLossPerTurn.Evaluate(strengthStat); }
		public static float StarvationDamagePerTurn { get { return instance.starvationDamagePerTurn; } }

		public static int MovesPerTurn { get { return instance.movesPerTurn; } }

		public static float MaxEnemyDistSqr { get { return instance.maxEnemyDistSqr; } }
		public static float One_Over_MaxEnemyDistSqr { get; private set; }

		public static float EnemyDistHeuristicMax { get { return instance.enemyDistHeuristicMax; } }


		#region Private fields

		private float minStart_Food = 200.0f,
					  maxStart_Food = 300.0f,
					  minStart_Energy = 100.0f,
					  maxStart_Energy = 200.0f,
					  minStart_Strength = 0.0f,
					  maxStart_Strength = 0.5f;

		private float initialLowFoodThreshold = 25.0f;

		private float max_health = 1.0f;

		private float starvationDamagePerTurn = 0.2f;
		
		//Loss in food goes down as strength increases.
		private AsymptoteValue foodLossPerTurn = new AsymptoteValue(0.2f, 0.0f, 2.0f);

		//Maximum-possible energy and food go up as strength increases.
		private ScaledValue maxFood = new ScaledValue(0.5f, 25.0f, 300.0f),
							maxEnergy = new ScaledValue(0.5f, 50.0f, 200.0f);

		private int movesPerTurn = 5;

		private int maxEnemyDistSqr = 11 * 11;
		private float enemyDistHeuristicMax = 1.0f;

		#endregion


		public void WriteData(MyData.Writer writer)
		{
			writer.Float(minStart_Food, "minStart_Food");
			writer.Float(maxStart_Food, "maxStart_Food");
			writer.Float(minStart_Energy, "minStart_Energy");
			writer.Float(maxStart_Energy, "maxStart_Energy");
			writer.Float(minStart_Strength, "minStart_Strength");
			writer.Float(maxStart_Strength, "maxStart_Strength");
			
			writer.Float(initialLowFoodThreshold, "initialLowFoodThreshold");
			
			writer.Float(max_health, "max_health");
			
			writer.Float(starvationDamagePerTurn, "starvationDamagePerTurn");

			writer.Structure(foodLossPerTurn, "foodLossPerTurn");

			writer.Structure(maxFood, "maxFood");
			writer.Structure(maxEnergy, "maxEnergy");

			writer.Int(movesPerTurn, "movesPerTurn");

			writer.Int(maxEnemyDistSqr, "maxEnemyDistSqr");
			writer.Float(enemyDistHeuristicMax, "enemyDistHeuristicMax");
		}
		public void ReadData(MyData.Reader reader)
		{
			minStart_Food = reader.Float("minStart_Food");
			maxStart_Food = reader.Float("maxStart_Food");
			minStart_Energy = reader.Float("minStart_Energy");
			maxStart_Energy = reader.Float("maxStart_Energy");
			minStart_Strength = reader.Float("minStart_Strength");
			maxStart_Strength = reader.Float("maxStart_Strength");
			
			initialLowFoodThreshold = reader.Float("initialLowFoodThreshold");
			
			max_health = reader.Float("max_health");
			
			starvationDamagePerTurn = reader.Float("starvationDamagePerTurn");

			reader.Structure(foodLossPerTurn, "foodLossPerTurn");

			reader.Structure(maxFood, "maxFood");
			reader.Structure(maxEnergy, "maxEnergy");

			movesPerTurn = reader.Int("movesPerTurn");

			maxEnemyDistSqr = reader.Int("maxEnemyDistSqr");
			enemyDistHeuristicMax = reader.Float("enemyDistHeuristicMax");
		}
	}
}
