using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyData;
using Job = GameLogic.Units.Player_Char.Job;


namespace GameLogic.Groups
{
	public class MonstersGroup : Group
	{
		/// <summary>
		/// Gets the MonsterGroup for the given map.
		/// Creates one if it doesn't exist yet.
		/// </summary>
		public static MonstersGroup Get(Map theMap)
		{
			var group = theMap.FindGroup<MonstersGroup>();
			if (group == null)
				group = new MonstersGroup(theMap);
			return group;
		}

		public MonstersGroup(Map theMap)
			: base(theMap, Consts.TurnPriority_Monster, false)
		{
			//This group is enemies with everyone, including itself.

			theMap.Groups.OnElementAdded += (set, group) =>
			{
				EnemiesByID.Add(group.ID);
			};

			foreach (var group in theMap.Groups)
				EnemiesByID.Add(group.ID);

			theMap.Groups.Add(this);
		}

		public override Types MyType { get { return Types.Monsters; } }
	}
}
