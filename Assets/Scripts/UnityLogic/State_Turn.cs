using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnityLogic
{
	/// <summary>
	/// Takes a specific team's turn.
	/// The turns are spread out over multiple frames.
	/// </summary>
	public class State_Turn : GameFSM.State
	{
		private GameLogic.Unit.Teams team;

		private List<GameLogic.Unit> unitsToUpdate = new List<GameLogic.Unit>();
		private int currentUnit;
		private float timeSinceLastUnit;


		public State_Turn(GameLogic.Unit.Teams _team)
		{
			team = _team;
			Init();
		}

		private void Init()
		{
			FSM.Map.Units.OnElementAdded += Callback_AddUnit;
			FSM.Map.Units.OnElementRemoved += Callback_RemoveUnit;
			FSM.Map.Units.OnUnitTeamChanged += Callback_UnitTeamChanged;
		}
		private void DeInit()
		{
			FSM.Map.Units.OnElementAdded -= Callback_AddUnit;
			FSM.Map.Units.OnElementRemoved -= Callback_RemoveUnit;
			FSM.Map.Units.OnUnitTeamChanged -= Callback_UnitTeamChanged;
		}


		public override GameFSM.State Start(GameFSM.State previousState)
		{
			unitsToUpdate.Clear();
			unitsToUpdate.AddRange(FSM.Map.Units.Where(u => u.Team == team));

			currentUnit = 0;
			timeSinceLastUnit = 0.0f;

			return base.Start(previousState);
		}
		public override GameFSM.State Update()
		{
			timeSinceLastUnit += Time.deltaTime;

			//Keep letting units take turns until we've caught up or gone through every unit.
			while (timeSinceLastUnit >= Options.UnitTurnInterval && currentUnit < unitsToUpdate.Count)
			{
				unitsToUpdate[currentUnit].TakeTurn();

				timeSinceLastUnit -= Options.UnitTurnInterval;
				currentUnit += 1;
			}

			//If we've gone through every unit, switch to the next turn.
			if (currentUnit >= unitsToUpdate.Count)
			{
				switch (team)
				{
					case GameLogic.Unit.Teams.Player:
						team = GameLogic.Unit.Teams.Environment;
						return this;
					case GameLogic.Unit.Teams.Environment:
						team = GameLogic.Unit.Teams.Monsters;
						return this;
					case GameLogic.Unit.Teams.Monsters:
						team = GameLogic.Unit.Teams.Player;
						return this;
					default: throw new NotImplementedException(team.ToString());
				}
			}

			return base.Update();
		}
		public override GameFSM.State End(GameFSM.State nextState)
		{
			//During normal operation, the next state is actually just this one
			//    after changing some fields.
			//However, this can get interrupted by e.x. the player ending the level.
			if (nextState != this)
				DeInit();

			return base.End(nextState);
		}

		private void Callback_AddUnit(LockedSet<GameLogic.Unit> mapUnits, GameLogic.Unit unit)
		{
			if (unit.Team == team)
				unitsToUpdate.Add(unit);
		}
		private void Callback_RemoveUnit(LockedSet<GameLogic.Unit> mapUnits, GameLogic.Unit unit)
		{
			int index = unitsToUpdate.IndexOf(unit);
			if (index > -1)
			{
				unitsToUpdate.RemoveAt(index);
				if (currentUnit >= index)
					currentUnit -= 1;
			}
		}
		private void Callback_UnitTeamChanged(GameLogic.UnitSet set, GameLogic.Unit unit,
											  GameLogic.Unit.Teams oldTeam, GameLogic.Unit.Teams newTeam)
		{
			//Remove the unit if it used to be on this team.
			if (oldTeam == team && newTeam != team)
			{
				UnityEngine.Assertions.Assert.IsTrue(unitsToUpdate.Contains(unit));
				Callback_RemoveUnit(set, unit);
			}
			//Add the unit if it just got put onto this team.
			else if (oldTeam != team && newTeam == team)
			{
				UnityEngine.Assertions.Assert.IsFalse(unitsToUpdate.Contains(unit));
				Callback_AddUnit(set, unit);
			}
		}
	}
}