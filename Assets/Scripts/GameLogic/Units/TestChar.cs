using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace GameLogic.Units
{
	/// <summary>
	/// A unit that belongs to the Environment. Does the following every turn:
	///     1. Gains food if standing on a TestStructure unit.
	///     2. Loses 1 food. If food runs out, the unit dies.
	///     3. Randomly moves to an adjacent space is one is open.
	/// </summary>
	public class TestChar : Unit
	{
		/// <summary>
		/// Raised when this character's food levels change.
		/// The parameters are this character, the old food amount,
		///     and the new food amount, respectively.
		/// </summary>
		public event Action<TestChar, int, int> OnFoodChanged;

		public int Food
		{
			get { return food; }
			set
			{
				int oldFood = food;
				food = value;

				if (OnFoodChanged != null)
					OnFoodChanged(this, oldFood, food);
			}
		}
		private int food = 20;


		protected override Types MyType { get { return Types.TestChar; } }

		
		public TestChar(Map map, Vector2i pos) : base(map, Teams.Environment, pos) { }
		public TestChar(Map map) : base(map, Teams.Environment) { }


		public void TakeTurn()
		{
			//If standing on a TestStructure, gain its food.
			TestStructure ts = Owner.GetUnitsAt(Pos).FirstOrDefault(u => u is TestStructure) as TestStructure;
			if (ts != null)
			{
				Food += ts.Food;
				ts.Food = 0;
			}

			//Subtract one from food. If there's no food left, die.
			Food -= 1;
			if (Food <= 0)
			{
				Owner.Units.Remove(this);
				return;
			}


			//Move to a random space.

			List<Vector2i> validPoses = new List<Vector2i>();

			if (Owner.Tiles.IsValid(Pos.LessX) && CanMoveTo(Owner.Tiles[Pos.LessX]))
				validPoses.Add(Pos.LessX);
			if (Owner.Tiles.IsValid(Pos.MoreX) && CanMoveTo(Owner.Tiles[Pos.MoreX]))
				validPoses.Add(Pos.MoreX);
			if (Owner.Tiles.IsValid(Pos.LessY) && CanMoveTo(Owner.Tiles[Pos.LessY]))
				validPoses.Add(Pos.LessY);
			if (Owner.Tiles.IsValid(Pos.MoreY) && CanMoveTo(Owner.Tiles[Pos.MoreY]))
				validPoses.Add(Pos.MoreY);
			
			if (validPoses.Count > 0)
				Pos = validPoses[UnityEngine.Random.Range(0, validPoses.Count)];
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
					return false;
				default: throw new NotImplementedException(tile.ToString());
			}
		}


		public override void WriteData(MyData.Writer writer)
		{
			base.WriteData(writer);
			writer.Int(food, "Food");
		}
		public override void ReadData(MyData.Reader reader)
		{
			base.ReadData(reader);
			Food = reader.Int("Food");
		}
	}
}