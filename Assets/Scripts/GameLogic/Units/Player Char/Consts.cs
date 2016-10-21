using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic.Units.Player_Char
{
	//TODO: Get all this from a file. Maybe "PlayerPrefs" file?

	public static class Consts
	{
		public static readonly float MinStart_Health = 18.0f,
									 MaxStart_Health = 30.0f,
									 MinStart_Food = 200.0f,
									 MaxStart_Food = 300.0f,
									 MinStart_Energy = 100.0f,
									 MaxStart_Energy = 200.0f,
									 MinStart_Strength = 1.0f,
									 MaxStart_Strength = 1.5f;

		public static readonly float BaseLossPerTurn_Food = 0.2f;
		public static readonly float LossPerTurn_Health = 0.2f;

		public static readonly int MovesPerTurn = 5;
	}
}
