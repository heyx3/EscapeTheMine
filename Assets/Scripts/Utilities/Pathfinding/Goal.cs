using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Pathfinding
{
	/// <summary>
	/// A value that doesn't have to exist.
	/// </summary>
	public struct OptionalVal<T> : IEquatable<OptionalVal<T>>
		where T : IEquatable<T>
	{
		public bool HasValue { get; private set; }
		private T val;

		public T Value { get { UnityEngine.Assertions.Assert.IsTrue(HasValue); return val; } }

		public OptionalVal(T _val) { HasValue = true; val = _val; }
		
		//You can implicitly cast between an OptionalVal and the value inside it.
		public static implicit operator T(OptionalVal<T> o) { return o.Value; }
		public static implicit operator OptionalVal<T>(T t) { return new OptionalVal<T>(t); }


		public override int GetHashCode()
		{
			if (HasValue)
				return val.GetHashCode();
			else
				return -1;
		}
		public override bool Equals(object obj)
		{
			return (obj is OptionalVal<T>) &&
				   Equals((OptionalVal<T>)obj);
		}
		public bool Equals(OptionalVal<T> o)
		{
			return (o.HasValue == HasValue) &&
				   (!o.HasValue || o.val.Equals(val));
		}

		static OptionalVal()
		{
			UnityEngine.Assertions.Assert.IsFalse(new OptionalVal<int>().HasValue,
												  "Wrong!");
		}
	}


	/// <summary>
	/// The goal for a pathfinding algorithm.
	/// Supports both a single, specific goal
	///     and a general predicate defining which nodes are acceptable goals.
	/// If both are specified, then the specific goal should be used for A* heuristics,
	///     but the predicate goals are acceptable results as well.
	/// </summary>
	/// <typeparam name="NodeType">
	/// The "nodes" in a graph.
	/// It's highly recommended to override GetHashCode() for this type.
	/// </typeparam>
	public struct Goal<NodeType>
		where NodeType : IEquatable<NodeType>
	{
		/// <summary>
		/// A specific node being pathed to.
		/// </summary>
		public OptionalVal<NodeType> SpecificGoal;
		/// <summary>
		/// A function that defines which nodes are a valid pathing goal.
		/// </summary>
		public Predicate<NodeType> GeneralGoal;

		public Goal(NodeType specificGoal) : this(specificGoal, null) { }
		public Goal(Predicate<NodeType> generalGoal)
		{
			SpecificGoal = new OptionalVal<NodeType>();
			GeneralGoal = generalGoal;
		}
		public Goal(NodeType specificGoal, Predicate<NodeType> generalGoal)
		{
			SpecificGoal = specificGoal;
			GeneralGoal = generalGoal;
		}

		/// <summary>
		/// Returns whether the given node qualifies as an end node for this goal.
		/// </summary>
		public bool IsValidEnd(NodeType n)
		{
			return (SpecificGoal.HasValue && n.Equals(SpecificGoal.Value)) ||
				   (GeneralGoal != null && GeneralGoal(n));
		}
	}
}