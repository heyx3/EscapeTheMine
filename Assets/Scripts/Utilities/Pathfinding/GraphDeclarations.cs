using System;
using System.Collections.Generic;

namespace Pathfinding
{
	//A "Graph" is defined as a set of "Nodes" connected by "Edges".
	//"Nodes" can be whatever you want as long as they implement IEquatable<>.


	/// <summary>
	/// A connection between two nodes.
	/// </summary>
	/// <typeparam name="NodeType">
	/// The "nodes" in a graph.
	/// It's highly recommended to override GetHashCode() for this type.
	/// </typeparam>
	public struct Edge<NodeType> : IEquatable<Edge<NodeType>>
		where NodeType : IEquatable<NodeType>
	{
		public NodeType Start { get; private set; }
		public NodeType End { get; private set; }
		
		public Edge(NodeType start, NodeType end)
		{
			Start = start;
			End = end;
		}

		public override int GetHashCode()
		{
			//Hash the start/end nodes' hash codes together.
			return new PRNG(Start.GetHashCode(), End.GetHashCode()).GetHashCode();
		}
		public override bool Equals(object obj)
		{
			return (obj is Edge<NodeType>) &&
				   Equals((Edge<NodeType>)obj);
		}
		public bool Equals(Edge<NodeType> e)
		{
			return e.Start.Equals(Start) && e.End.Equals(End);
		}
	}


	/// <summary>
	/// A collection of nodes connected via edges.
	/// </summary>
	/// <typeparam name="NodeType">
	/// The "nodes" in a graph.
	/// It's highly recommended to override GetHashCode() for this type.
	/// </typeparam>
	public interface IGraph<NodeType>
		where NodeType : IEquatable<NodeType>
	{
		/// <summary>
		/// Get all edges that start at the given Node,
		///     and put them into the given empty collection.
		/// </summary>
		void GetConnections(NodeType starting, HashSet<Edge<NodeType>> outEdgeList);
	}
}