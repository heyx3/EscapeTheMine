using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MyData;
using System.Collections;

namespace GameLogic.Units
{
	//TODO: Next step of "Adding a new Unit" text doc: make renderer prefab.

	/// <summary>
	/// An aggressive monster.
	/// </summary>
	public class LizardMan : Unit
	{
		public Stat<float, LizardMan> Health { get; private set; }
		public Stat<float, LizardMan> Strength { get; private set; }

		public override string DisplayName { get { return Localization.Get("NAME_LIZARDMAN"); } }
		public override bool BlocksMovement { get { return false; } }
		public override bool BlocksStructures { get { return true; } }


		public LizardMan(Map theMap, ulong groupID, float health, float strength)
			: base(theMap, groupID)
		{
			Health = new Stat<float, LizardMan>(this, health);
			Strength = new Stat<float, LizardMan>(this, strength);
		}
		public LizardMan(Map theMap) : this(theMap, ulong.MaxValue, 0.0f, 0.0f) { }


		public override IEnumerable TakeTurn()
		{
			//TODO: Logic. Follow the design doc.
			yield break;
		}


		#region Serialization

		public override Types MyType { get { return Types.LizardMan; } }
		public override void WriteData(Writer writer)
		{
			base.WriteData(writer);
			writer.Float(Health, "health");
			writer.Float(Strength, "strength");
		}
		public override void ReadData(Reader reader)
		{
			base.ReadData(reader);
			Health.Value = reader.Float("health");
			Strength.Value = reader.Float("strength");
		}

		#endregion
	}
}
