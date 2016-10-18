using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GameLogic.Units
{
	/// <summary>
	/// A unit that belongs to the Environment. Does the following every turn:
	///     1. Takes food if standing on a TestStructure unit.
	///     2. Loses food. If food runs out, the unit dies.
	///     3. Randomly moves to an adjacent space is one is open.
	/// </summary>
	public class TestChar : Unit
	{
		/// <summary>
		/// Raised when this character's food levels change.
		/// The parameters are this character, the old food amount,
		///     and the new food amount, respectively.
		/// </summary>
		public event Action<TestChar, float, float> OnFoodChanged;

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
		private float food = 200.0f;
        private float foodLossPerTurn = 0.1f;


		protected override Types MyType { get { return Types.TestChar; } }

		
		public TestChar(Map map, Vector2i pos) : base(map, Teams.Environment, pos) { }
		public TestChar(Map map) : base(map, Teams.Environment) { }

		private TestChar(Map map, TestChar copyFrom) : base(map, copyFrom)
		{
			food = copyFrom.food;
		}


		public override Unit Clone(Map newOwner)
		{
			return new TestChar(newOwner, this);
		}

		public override void TakeTurn()
		{
			//If standing on a TestStructure, gain its food.
			TestStructure ts = Owner.GetUnitsAt(Pos).FirstOrDefault(u => u is TestStructure) as TestStructure;
			if (ts != null)
			{
				Food += ts.Food;
				ts.Food = 0.0f;
			}

            //Subtract one from food. If there's no food left, die.
            Food -= foodLossPerTurn;
			if (Food <= 0.0f)
			{
				Owner.Units.Remove(this);
				return;
			}


			//Move to a random space.

			List<Vector2i> validPoses = new List<Vector2i>();

			Vector2i pos = Pos;
			if (Owner.Tiles.IsValid(pos.LessX) && CanMoveTo(Owner.Tiles[pos.LessX]))
				validPoses.Add(pos.LessX);
			if (Owner.Tiles.IsValid(pos.MoreX) && CanMoveTo(Owner.Tiles[pos.MoreX]))
				validPoses.Add(pos.MoreX);
			if (Owner.Tiles.IsValid(pos.LessY) && CanMoveTo(Owner.Tiles[pos.LessY]))
				validPoses.Add(pos.LessY);
			if (Owner.Tiles.IsValid(pos.MoreY) && CanMoveTo(Owner.Tiles[pos.MoreY]))
				validPoses.Add(pos.MoreY);
			
			if (validPoses.Count > 0)
				Pos.Value = validPoses[UnityEngine.Random.Range(0, validPoses.Count)];
		}
		private bool CanMoveTo(TileTypes tile)
		{
			switch (tile)
			{
				case TileTypes.Empty:
				case TileTypes.Exit:
				case TileTypes.Entrance:
					return true;
				case TileTypes.Wall:
				case TileTypes.Bedrock:
					return false;
				default: throw new NotImplementedException(tile.ToString());
			}
		}


		public override void WriteData(MyData.Writer writer)
		{
			base.WriteData(writer);
			writer.Float(food, "Food");
		}
		public override void ReadData(MyData.Reader reader)
		{
			base.ReadData(reader);
			Food = reader.Float("Food");
		}
	}
}