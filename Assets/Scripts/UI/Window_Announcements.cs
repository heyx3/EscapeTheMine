using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace MyUI
{
	/// <summary>
	/// A singleton Window that should always be open,
	///     and displays any announcements about events in the game.
	/// Periodically hides itself if no announcements have happened recently.
	/// </summary>
	public class Window_Announcements : Window_Global
	{
		public static Window_Announcements Instance { get; private set; }


		/// <summary>
		/// All messages are parented to this object.
		/// </summary>
		public Transform ContentParent;
		/// <summary>
		/// The template for an announcement label.
		/// </summary>
		public GameObject LabelPrefab;
		/// <summary>
		/// The amount of time since the last interaction before this window hides itself.
		/// </summary>
		public float TimeTillHide = 6.0f;
		/// <summary>
		/// The maximum number of announcements allowed in this window at once.
		/// </summary>
		public int MaxAnnouncements = 3;

		private float timeSinceLastInteraction;


		public void Announce(string msg)
		{
			//Make sure this window is visible.
			gameObject.SetActive(true);

			//Reset the clock on hiding this window.
			timeSinceLastInteraction = TimeTillHide;

			//If we're at the max number of announcements, clear out the oldest one.
			if (ContentParent.childCount >= MaxAnnouncements)
				Destroy(ContentParent.GetChild(0).gameObject);

			//Make the label.
			GameObject newLabel = Instantiate(LabelPrefab);
			newLabel.GetComponentInChildren<UnityEngine.UI.Text>().text = msg;
			newLabel.transform.SetParent(ContentParent, false);
		}

		protected override void Awake()
		{
			base.Awake();

			if (Instance != null)
				Debug.LogError("More than one Window_Announcements at once");
			Instance = this;

			timeSinceLastInteraction = 0.0f;
		}
		private void Start()
		{
			Game.Map.Groups.OnElementAdded += Callback_NewGroup;
			foreach (GameLogic.Group g in Game.Map.Groups)
				Callback_NewGroup(Game.Map.Groups, g);

			Game.OnStart += () => gameObject.SetActive(true);
			Game.OnEnd += () => gameObject.SetActive(false);
		}
		private void Update()
		{
			timeSinceLastInteraction -= Time.deltaTime;
			if (timeSinceLastInteraction <= 0.0f)
				gameObject.SetActive(false);
		}

		protected override void Callback_MapChanging()
		{
			//Just don't close like windows usually do.
			//Note that in the Start() method,
			//    this window adds callbacks to hide/show itself when a map ends/starts.
		}

		public override void Callback_DragButton_Titlebar()
		{
			base.Callback_DragButton_Titlebar();
			
			//Reset the clock on hiding this window.
			timeSinceLastInteraction = TimeTillHide;
		}

		private void Callback_NewGroup(LockedSet<GameLogic.Group> groups, GameLogic.Group newGroup)
		{
			newGroup.UnitsByID.OnElementAdded += Callback_NewUnit;
			foreach (ulong unitID in newGroup.UnitsByID)
				Callback_NewUnit(newGroup.UnitsByID, unitID);

			if (newGroup is GameLogic.Groups.PlayerGroup)
			{
				var pGroup = (GameLogic.Groups.PlayerGroup)newGroup;

				pGroup.OnNewJob += Callback_NewJob;
				foreach (GameLogic.Units.Player_Char.Job j in pGroup.NormalJobs.Concat(pGroup.EmergencyJobs))
					Callback_NewJob(pGroup, j);
			}
		}
		private void Callback_NewUnit(LockedSet<ulong> unitsByID, ulong id)
		{
			GameLogic.Unit unit = Game.Map.GetUnit(id);

			if (unit is GameLogic.Units.PlayerChar)
			{
				var pChar = (GameLogic.Units.PlayerChar)unit;

				pChar.OnAddCustomJob += Callback_NewCustomJob;
				foreach (GameLogic.Units.Player_Char.Job j in pChar.CustomJobs)
					Callback_NewCustomJob(pChar, j);
			}
		}
		private void Callback_NewJob(GameLogic.Groups.PlayerGroup group,
									 GameLogic.Units.Player_Char.Job newJob)
		{
			newJob.OnJobFinished += Callback_JobFinished;
		}
		private void Callback_NewCustomJob(GameLogic.Units.PlayerChar pChar,
										   GameLogic.Units.Player_Char.Job newJob)
		{
			newJob.OnJobFinished += Callback_JobFinished;
		}
		private void Callback_JobFinished(GameLogic.Units.Player_Char.Job job,
										  bool wasSuccessful, string message)
		{
			if (message != null)
				Announce(message);
		}
	}
}