using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
	/// <summary>
	/// A piece of data owned by some object.
	/// </summary>
	public class Stat<StatType, OwnerType>
	{
		/// <summary>
		/// Raised when this state changes.
		/// The first parameter is the owner of this stat.
		/// The second and third arguments are the old and new value, respectively.
		/// </summary>
		public Action<OwnerType, StatType, StatType> OnChanged;

		public StatType Value
		{
			get { return val; }
			set
			{
				StatType oldVal = val;
				val = value;

				if (OnChanged != null)
					OnChanged(Owner, oldVal, val);
			}
		}
		private StatType val;

		public OwnerType Owner { get; private set; }


		public Stat(OwnerType owner, StatType value)
		{
			Owner = owner;
			val = value;
		}


		/// <summary>
		/// You can implicitly cast this class to the value it contains.
		/// </summary>
		public static implicit operator StatType(Stat<StatType, OwnerType> s)
		{
			return s.val;
		}
	}
}
