using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
	/// <summary>
	/// A piece of data owned by a unit.
	/// </summary>
	public class Stat<T>
	{
		/// <summary>
		/// Raised when this state changes.
		/// The first parameter is the unit whose stat this is.
		/// The second and third arguments are the old and new value, respectively.
		/// </summary>
		public Action<Unit, T, T> OnChanged;

		public T Value
		{
			get { return val; }
			set
			{
				T oldVal = val;
				val = value;

				if (OnChanged != null)
					OnChanged(Owner, oldVal, val);
			}
		}
		private T val;

		public Unit Owner { get; private set; }


		public Stat(Unit owner, T value)
		{
			Owner = owner;
			val = value;
		}


		/// <summary>
		/// You can implicitly cast this class to the value it contains.
		/// </summary>
		public static implicit operator T(Stat<T> s)
		{
			return s.val;
		}
	}
}
