using System;
using System.Collections;
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
		private List<GameLogic.Unit> unitsToUpdate = new List<GameLogic.Unit>();
		private int currentUnit = 0;
		private float timeSinceLastUnit;


		public State_Turn()
		{
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


		public override void Start(GameFSM.State previousState)
		{
			unitsToUpdate.Clear();
			unitsToUpdate.AddRange(FSM.Map.Units.Where(u => u.Team == GameFSM.Instance.CurrentTurn));
			
			timeSinceLastUnit = 0.0f;
		}
		public override IEnumerable Update()
		{
			timeSinceLastUnit += Time.deltaTime;

			//Keep letting units take turns until we've caught up or gone through every unit.
			while (timeSinceLastUnit >= Options.UnitTurnInterval && currentUnit < unitsToUpdate.Count)
			{
				//Note that some time may elapse during this unit's turn,
				//    but we don't want to count that towards the time till the next unit.
				foreach (object o in unitsToUpdate[currentUnit].TakeTurn())
					yield return o;

				timeSinceLastUnit -= Options.UnitTurnInterval;
				currentUnit += 1;
			}

			//If we've gone through every unit, switch to the next turn.
			//Do this by restarting the current state.
			if (currentUnit >= unitsToUpdate.Count)
			{
				currentUnit = 0;
				
				switch (FSM.CurrentTurn)
				{
					case GameLogic.Unit.Teams.Player:
						FSM.CurrentTurn = GameLogic.Unit.Teams.Environment;
						break;

					case GameLogic.Unit.Teams.Environment:
						FSM.CurrentTurn = GameLogic.Unit.Teams.Monsters;
						break;

					case GameLogic.Unit.Teams.Monsters:
						FSM.CurrentTurn = GameLogic.Unit.Teams.Player;
						break;

					default: throw new NotImplementedException(FSM.CurrentTurn.ToString());
				}

				FSM.CurrentState = this;
			}
		}
		public override void End(GameFSM.State nextState)
		{
			//During normal operation, the next state is actually just this one
			//    after changing some fields.
			//However, this might get interrupted by e.x. the player ending the level.
			if (nextState != this)
				DeInit();
		}

		private void Callback_AddUnit(LockedSet<GameLogic.Unit> mapUnits, GameLogic.Unit unit)
		{
			if (unit.Team == FSM.CurrentTurn)
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
			GameLogic.Unit.Teams currentTeam = FSM.CurrentTurn;

			//Remove the unit if it used to be on this team.
			if (oldTeam == currentTeam && newTeam != currentTeam)
			{
				UnityEngine.Assertions.Assert.IsTrue(unitsToUpdate.Contains(unit));
				Callback_RemoveUnit(set, unit);
			}
			//Add the unit if it just got put onto this team.
			else if (oldTeam != currentTeam && newTeam == currentTeam)
			{
				UnityEngine.Assertions.Assert.IsFalse(unitsToUpdate.Contains(unit));
				Callback_AddUnit(set, unit);
			}
		}
	}
}