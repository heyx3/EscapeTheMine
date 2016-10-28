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
		public static Consts Instance = new Consts();
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
		}


		public float MinStart_Food { get { return minStart_Food; } }
		public float MaxStart_Food { get { return maxStart_Food; } }

		public float MinStart_Energy { get { return minStart_Energy; } }
		public float MaxStart_Energy { get { return maxStart_Energy; } }

		public float MinStart_Strength { get { return minStart_Strength; } }
		public float MaxStart_Strength { get { return maxStart_Strength; } }

		public float InitialLowFoodThreshold { get { return initialLowFoodThreshold; } }

		public float Max_Health { get { return max_health; } }

		public float MaxFood(float strengthStat) { return maxFood.Evaluate(strengthStat); }
		public float MaxEnergy(float strengthStat) { return maxEnergy.Evaluate(strengthStat); }

		public float FoodLossPerTurn(float strengthStat) { return foodLossPerTurn.Evaluate(strengthStat); }
		public float StarvationDamagePerTurn { get { return starvationDamagePerTurn; } }

		public int MovesPerTurn { get { return movesPerTurn; } }


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


		/// <summary>
		/// Represents a value that is calculated as ((Stat^exp) * scale) + offset, where:
		/// "Stat" is some stat, like "Strength", whose value strongly influences this value.
		/// "exp" is an exponent that affects how quickly the stat grows.
		/// "scale" is a scale that adjusts how the input stat maps to the output value.
		/// "offset" is the "base" stat that you get when X is 0.
		/// </summary>
		[Serializable]
		public class ScaledValue : MyData.IReadWritable
		{
			public float Exp, Scale, Offset;

			public ScaledValue(float exp, float scale, float offset)
			{
				Exp = exp;
				Scale = scale;
				Offset = offset;
			}

			public float Evaluate(float stat, float? min = null, float? max = null)
			{
				float result = Offset + (Scale * Mathf.Pow(stat, Exp));

				if (min.HasValue)
					result = Math.Max(result, min.Value);
				if (max.HasValue)
					result = Math.Min(result, max.Value);

				return result;
			}
			public void WriteData(MyData.Writer writer)
			{
				writer.Float(Exp, "exp");
				writer.Float(Scale, "scale");
				writer.Float(Offset, "offset");
			}
			public void ReadData(MyData.Reader reader)
			{
				Exp = reader.Float("exp");
				Scale = reader.Float("scale");
				Offset = reader.Float("offset");
			}
		}
		/// <summary>
		/// Represents a value that asymptotically approaches some "boundary" value
		///     from above or below as a stat (e.x. "Strength") increases.
		/// The actual basic function used is (1/(x + 1)) - 1.
		/// </summary>
		[Serializable]
		public class AsymptoteValue : MyData.IReadWritable
		{
			/// <summary>
			/// "Start" is the value when the stat is 0.
			/// "End" is the value when the stat is infinity
			///     (note: if the stat is actually float.PositiveInfinity, it will NOT result in "End").
			/// </summary>
			public float Start, End;
			/// <summary>
			/// The larger the value, the more extreme the initial departure from the starting value.
			/// </summary>
			public float Slope;

			public AsymptoteValue(float start, float end, float slope)
			{
				Start = start;
				End = end;
				Slope = slope;
			}

			public float Evaluate(float stat)
			{
				//Figured out how to do this thanks to Wolfram Alpha.

				//Asymptotic growth from 0 towards 1 based on "stat".
				float t = -((1.0f / ((stat * Slope) + 1.0f)) - 1.0f);
				//Manual lerp to skip Unity's automatic clamp in Mathf.Lerp().
				return Start + ((End - Start) * t);
			}
			public void WriteData(MyData.Writer writer)
			{
				writer.Float(Start, "start");
				writer.Float(End, "end");
				writer.Float(Slope, "slope");
			}
			public void ReadData(MyData.Reader reader)
			{
				Start = reader.Float("start");
				End = reader.Float("end");
				Slope = reader.Float("slope");
			}
		}


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
		}
	}
}
