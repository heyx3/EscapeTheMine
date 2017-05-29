using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	/// <summary>
	/// A singleton Window that should always be open.
	/// Displays any announcements about events in the game.
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
			if (newGroup is GameLogic.Groups.PlayerGroup)
			{
				var pGroup = (GameLogic.Groups.PlayerGroup)newGroup;
				pGroup.JobQueries.OnJobCreated += Callback_NewJob;

				//Call the "New Job" callback on any existing jobs.
				
				foreach (var job in pGroup.NormalJobs.Concat(pGroup.EmergencyJobs))
					Callback_NewJob(pGroup, job);

				//If the map is in the middle of loading, we have to defer this part.
				Game.DoAfterMapLoaded(() =>
				{
					foreach (ulong unitID in pGroup.UnitsByID)
					{
						var unit = Game.Map.GetUnit(unitID);
						if (unit is GameLogic.Units.PlayerChar)
						{
							var pChar = (GameLogic.Units.PlayerChar)unit;

							foreach (var job in pChar.CustomJobs)
								Callback_NewJob(pGroup, job);

							if (pChar.CurrentJob != null)
								Callback_NewJob(pGroup, pChar.CurrentJob);
						}
					}
				});
			}
		}

		private void Callback_NewJob(GameLogic.Groups.PlayerGroup group,
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