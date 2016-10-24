using System;
using System.Collections.Generic;
using UnityEngine;


namespace Pathfinding
{
	/// <summary>
	/// Runs A*/Djikstra to find a path from a start to a goal.
	/// Instances of this class are not thread-safe;
	///     collections are re-used between calls to "FindPath()" to reduce garbage.
	/// </summary>
	/// <typeparam name="NodeType">
	/// The "nodes" in the graph.
	/// It's highly recommended to override GetHashCode() for this type.
	/// </typeparam>
	public class PathFinder<NodeType>
		where NodeType : IEquatable<NodeType>
	{
		public IGraph<NodeType> Graph;

		/// <summary>
		/// Outputs the length of the edge,
		///     as well as an optional A* heuristic to be added to that length.
		/// </summary>
		public delegate void CostCalculator(Goal<NodeType> goal, Edge<NodeType> edge,
											out float edgeLength, out float heuristic);
		public CostCalculator CalcCosts;


		/// <summary>
		/// Creates a new PathFinder.
		/// </summary>
		/// <param name="graph">The graph to search.</param>
		/// <param name="makeEdge">Constructs an Edge from the given start and end nodes.</param>
		public PathFinder(IGraph<NodeType> graph, CostCalculator calcCosts)
		{
			Graph = graph;
			CalcCosts = calcCosts;
		}


		/// <summary>
		/// Finds the shortest path from the given start to a node that satisfies the given goal.
		/// </summary>
		/// <param name="outPath">
		/// After this method is called, this list contains the path from start to end,
		///     NOT including the start node itself.
		/// </param>
		/// <param name="maxPathLength">
		/// The maximum path length this method can search from the start node.
		/// "Path length" is the sum of the lengths of each edge in a path.
		/// </param>
		/// <param name="tryMyBest">
		/// If true, and a path to an actual end can't be found,
		///     this method will attempt to find the closest path possible.
		/// </param>
		public bool FindPath(NodeType start, Goal<NodeType> goal, float maxPathLength,
							 bool tryMyBest, List<NodeType> outPath)
		{
			//Note: edge length is referred to as "search" cost,
			//    while edge length + A* heuristics is referred to as "traversal" cost.
			
			//Clear out the collections for this run.
			pathTree.Clear();
			nodesToSearch.Clear();
			cost_traversal.Clear();
			cost_search.Clear();
			considered.Clear();
			connections.Clear();

			//The final stopping place of this path.
			OptionalVal<NodeType> finalDestination = new OptionalVal<NodeType>();


			//Start searching from the source node.
			considered.Add(start);
			pathTree.Add(start, new OptionalVal<NodeType>());
			cost_traversal.Add(start, 0.0f);
			cost_search.Add(start, 0.0f);
			//Start the search frontier with a phony edge that "ends" at the start node.
			nodesToSearch.Push(new Edge<NodeType>(default(NodeType), start), 0.0f);

			//This is used in case we can't find the actual end node.
			NodeType lastNodeChecked = default(NodeType);

			//As long as there are more edges to search, keep checking them out.
			while (!nodesToSearch.IsEmpty)
			{
				//Get the edge/node being checked out.
				KeyValuePair<float, Edge<NodeType>> edgeToCheck = nodesToSearch.Pop();
				NodeType edgeDestination = edgeToCheck.Value.End;
				lastNodeChecked = edgeDestination;

				float totalTraversalCost = edgeToCheck.Key;
				float totalSearchCost = cost_search[edgeDestination];


				//Put this edge into the path tree.
				//Note that if it was already in the path,
				//    then a shorter route to it has already been found.
				if (!pathTree.ContainsKey(edgeDestination))
					pathTree.Add(edgeDestination, edgeToCheck.Value.Start);

				//If a goal has been found, stop here.
				if (goal.IsValidEnd(edgeDestination))
				{
					finalDestination = edgeDestination;
					break;
				}

				//If we've searched too far, discard this edge.
				if (totalSearchCost >= maxPathLength)
					continue;


				//Now process all the edges coming out of this node.

				connections.Clear();
				Graph.GetConnections(edgeDestination, connections);
				foreach (Edge<NodeType> connection in connections)
				{
					//Get the total cost of traversing/searching to this node.
					float edgeLength, heuristic;
					CalcCosts(goal, connection, out edgeLength, out heuristic);
					float costToEdgeEnd_search = edgeLength + totalSearchCost,
						  costToEdgeEnd_traversal = edgeLength + heuristic + totalTraversalCost;

					//Only check out this node if it's not too far away.
					if (costToEdgeEnd_search <= maxPathLength)
					{
						//If this node hasn't been found yet, add it to the list of nodes to search.
						if (!considered.Contains(connection.End))
						{
							//Add the edge to the search space.
							considered.Add(connection.End);
							cost_traversal.Add(connection.End, costToEdgeEnd_traversal);
							cost_search.Add(connection.End, costToEdgeEnd_search);

							nodesToSearch.Push(connection, costToEdgeEnd_traversal);
						}
						//If it HAS been found already, this must be a longer path.
						else //if (traversalCostToEdgeEnd < costToMoveToNode[connection.End])
						{
							UnityEngine.Assertions.Assert.IsTrue(costToEdgeEnd_traversal >=
																	 cost_traversal[connection.End]);
							//Update the path tree.
							//CostToMoveToNode[connections[i].End] = tempCost;
							//getToNodeSearchCost[connections[i].End] = tempSearchCost;
							//PathTree[connections[i].End] = connections[i].Start;
						}
					}
				}
			}


			//Now generate the actual path.

			//If we didn't find a valid goal node, pick the closest thing to it.
			if (!finalDestination.HasValue)
			{
				if (!tryMyBest || pathTree.Count == 0)
					return false;

				//If we can use heuristics to search towards a goal,
				//    we can abuse those heuristics to find something close to the goal.
				if (goal.SpecificGoal.HasValue)
				{
					OptionalVal<NodeType> bestEnd = new OptionalVal<NodeType>();
					float bestHeuristic = float.PositiveInfinity;

					foreach (NodeType n in pathTree.Values)
					{
						float edgeLength, heuristic;
						CalcCosts(goal, new Edge<NodeType>(n, goal.SpecificGoal.Value),
								  out edgeLength, out heuristic);
						if (heuristic < bestHeuristic)
						{
							bestEnd = n;
							bestHeuristic = heuristic;
						}
					}
					
					finalDestination = bestEnd.Value;
				}
				//Otherwise, there's no way to know how close we are,
				//    so just use the last node we checked out.
				else
				{
					finalDestination = lastNodeChecked;
				}
			}

			//Build the path using the path tree.
			outPath.Clear();
			NodeType counter = finalDestination.Value;
			while (!counter.Equals(start))
			{
				outPath.Add(counter);
				counter = pathTree[counter];
			}
			outPath.Reverse();

			return goal.IsValidEnd(finalDestination.Value);
		}

		#region Reused collections
		
		//Note: edge length is referred to as "search" cost,
		//    while edge length + A* heuristics is referred to as "traversal" cost.

		/// <summary>
		/// Indexes each node to the very next node in a path back to the start node.
		/// </summary>
		private Dictionary<NodeType, OptionalVal<NodeType>> pathTree =
			new Dictionary<NodeType, OptionalVal<NodeType>>();

		/// <summary>
		/// The most pressing edges to search, sorted so that lowest-cost edges are at the front.
		/// </summary>
		private IndexedPriorityQueue<Edge<NodeType>> nodesToSearch =
			new IndexedPriorityQueue<Edge<NodeType>>(true);

		/// <summary>
		/// Stores the cost to traverse/search from the start to the given node.
		/// </summary>
		private Dictionary<NodeType, float> cost_traversal = new Dictionary<NodeType, float>(),
											cost_search = new Dictionary<NodeType, float>();

		/// <summary>
		/// Nodes that have already been considered.
		/// </summary>
		private HashSet<NodeType> considered = new HashSet<NodeType>();

		/// <summary>
		/// Temp list used to hold all edges coming out from a given node.
		/// </summary>
		private HashSet<Edge<NodeType>> connections = new HashSet<Edge<NodeType>>();

		#endregion
	}
}