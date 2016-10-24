using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
	public class Graph : Pathfinding.IGraph<Vector2i>
	{
		/// <summary>
		/// Does the basic edge length/heuristic calculation
		/// that all units will likely use for their pathing.
		/// </summary>
		public static void AStarEdgeCalc(Pathfinding.Goal<Vector2i> goal,
										 Pathfinding.Edge<Vector2i> edge,
										 out float edgeLength, out float heuristic)
		{
			//For performance (and because they'll generally be adjacent),
			//    use manhattan distance between nodes.
			edgeLength = Math.Abs(edge.Start.x - edge.End.x) + Math.Abs(edge.Start.y - edge.End.y);


			heuristic = 0.0f;

			//Manhattan distance to the goal.
			if (goal.SpecificGoal.HasValue)
			{
				Vector2i specificGoal = goal.SpecificGoal.Value;
				float dist = Math.Abs(edge.End.x - specificGoal.x) +
							 Math.Abs(edge.End.y - specificGoal.y);

				//Square it to make it more important.
				heuristic += dist * dist;
			}
		}


		public Map Owner { get; private set; }


		public Graph(Map owner) { Owner = owner; }


		public void GetConnections(Vector2i starting, HashSet<Pathfinding.Edge<Vector2i>> outEdgeList)
		{
			//Grab all of the four adjacent spaces that are on the map and don't block movement.

			Vector2i lessX = starting.LessX,
					 moreX = starting.MoreX,
					 lessY = starting.LessY,
					 moreY = starting.MoreY;
			
			if (Owner.Tiles.IsValid(lessX) && !Owner.Tiles[lessX].BlocksMovement())
				outEdgeList.Add(new Pathfinding.Edge<Vector2i>(starting, lessX));
			if (Owner.Tiles.IsValid(lessY) && !Owner.Tiles[lessY].BlocksMovement())
				outEdgeList.Add(new Pathfinding.Edge<Vector2i>(starting, lessY));
			if (Owner.Tiles.IsValid(moreX) && !Owner.Tiles[moreX].BlocksMovement())
				outEdgeList.Add(new Pathfinding.Edge<Vector2i>(starting, moreX));
			if (Owner.Tiles.IsValid(moreY) && !Owner.Tiles[moreY].BlocksMovement())
				outEdgeList.Add(new Pathfinding.Edge<Vector2i>(starting, moreY));
		}
	}
}
