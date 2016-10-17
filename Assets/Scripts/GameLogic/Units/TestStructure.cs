using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GameLogic.Units
{
	/// <summary>
	/// Belongs to the environment, gives itself some Food every turn.
	/// </summary>
	public class TestStructure : Unit
	{
		/// <summary>
		/// Raised when this structure's food levels change.
		/// The parameters are this structure, the old food amount,
		///     and the new food amount, respectively.
		/// </summary>
		public event Action<TestStructure, float, float> OnFoodChanged;

		public float Food
		{
			get { return food; }
			set
			{
                float oldFood = food;
				food = value;

				if (OnFoodChanged != null)
					OnFoodChanged(this, oldFood, food);
			}
		}
		private float food = 0.0f;
        private float foodGainPerTurn = 0.05f;

		
		protected override Types MyType { get { return Types.TestStructure; } }


		public TestStructure(Map map) : base(map, Teams.Environment) { }
		public TestStructure(Map map, Vector2i pos) : base(map, Teams.Environment, pos) { }

		private TestStructure(Map map, TestStructure copyFrom) : base(map, copyFrom)
		{
			food = copyFrom.food;
		}


		public override Unit Clone(Map newOwner)
		{
			return new TestStructure(newOwner, this);
		}

		public override void TakeTurn()
		{
			Food += foodGainPerTurn;
		}
	}
}