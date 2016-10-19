using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyData;
using UnityEngine;


namespace GameLogic.Units
{
	public class PlayerChar : Unit
	{
		public Stat<float, PlayerChar> Food, Energy, Health, Strength;


		public PlayerChar(Map newOwner, float food, float energy, float health, float strength)
			: base(newOwner, Teams.Player)
		{
			Food = new Stat<float, PlayerChar>(this, food);
			Energy = new Stat<float, PlayerChar>(this, energy);
			Health = new Stat<float, PlayerChar>(this, health);
			Strength = new Stat<float, PlayerChar>(this, strength);
		}
		public PlayerChar(Map newOwner) : this(newOwner, 0.0f, 0.0f, 0.0f, 0.0f) { }

		protected PlayerChar(Map newOwner, PlayerChar copyFrom)
			: base(newOwner, copyFrom)
		{
			Food = new Stat<float, PlayerChar>(this, copyFrom.Food);
			Energy = new Stat<float, PlayerChar>(this, copyFrom.Energy);
			Health = new Stat<float, PlayerChar>(this, copyFrom.Health);
			Strength = new Stat<float, PlayerChar>(this, copyFrom.Strength);
		}
		public override Unit Clone(Map newOwner)
		{
			return new PlayerChar(newOwner, this);
		}


		public override void TakeTurn()
		{
			//Lose food over time.
			if (Food > 0.0f)
			{
				float newFood = Food - (Player_Char.Consts.BaseLossPerTurn_Food / Strength.Value);
				Food.Value = Mathf.Max(0.0f, newFood);
			}
			//If no food is left, lose health over time (i.e. starvation).
			else
			{
				float newHealth = Health - Player_Char.Consts.LossPerTurn_Health;
				if (newHealth <= 0.0f)
				{
					Health.Value = 0.0f;
					Owner.Units.Remove(this);
					return;
				}
				else
				{
					Health.Value = newHealth;
				}
			}

			//TODO: More features.
		}


		//Serialization.
		protected override Types MyType { get { return Types.PlayerChar; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);
			writer.Float(Food, "food");
			writer.Float(Energy, "energy");
			writer.Float(Health, "health");
			writer.Float(Strength, "strength");
		}
		public override void ReadData(Reader reader)
		{
			base.ReadData(reader);
			Food.Value = reader.Float("food");
			Energy.Value = reader.Float("energy");
			Health.Value = reader.Float("health");
			Strength.Value = reader.Float("strength");
		}
	}
}
