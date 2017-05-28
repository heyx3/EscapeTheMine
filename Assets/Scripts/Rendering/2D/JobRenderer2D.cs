using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using GameLogic.Units.Player_Char;
using GameLogic.Groups;
using GameLogic.Groups.Player_Group;


namespace Rendering.TwoD
{
	public class JobRenderer2D : Singleton<JobRenderer2D>
	{
		[SerializeField]
		private Color jobColor_Pending = Color.yellow,
					  jobColor_Active = Color.green,
					  jobColor_Stopped = Color.red;
		[SerializeField]
		private Dict.SpritesByJobType spritesByJobType = new Dict.SpritesByJobType(true);
		
		private Dictionary<Job, List<ulong>> highlightsPerJob = new Dictionary<Job, List<ulong>>();
		private List<List<ulong>> extraHighlightLists = new List<List<ulong>>();

		private PlayerGroup playerGroup = null;


		private void Start()
		{
			var game = UnityLogic.EtMGame.Instance;
			game.OnStart += Callback_MapStart;
		}

		private void Callback_MapStart()
		{
			//Clean up the previous map's callbacks.
			if (playerGroup != null)
			{
				playerGroup.JobQueries.OnJobCreated -= Callback_JobCreated;
				playerGroup.JobQueries.OnJobDestroyed -= Callback_JobDestroyed;
				playerGroup.JobQueries.OnJobStarted -= Callback_JobStarted;
				playerGroup.JobQueries.OnJobStopped -= Callback_JobStopped;
			}

			//Set up callbacks for the new player group.
			playerGroup = UnityLogic.EtMGame.Instance.Map.FindGroup<PlayerGroup>();
			playerGroup.JobQueries.OnJobCreated += Callback_JobCreated;
			playerGroup.JobQueries.OnJobDestroyed += Callback_JobDestroyed;
			playerGroup.JobQueries.OnJobStarted += Callback_JobStarted;
			playerGroup.JobQueries.OnJobStopped += Callback_JobStopped;
		}

		private void Callback_JobCreated(PlayerGroup playerGroup, Job job)
		{
			//If there's no "affected" positions, then no highlights are needed.
			if (!playerGroup.JobQueries.AffectsAnyPoses(job))
				return;

			//Create the list of jobs.
			//Use an old discarded list if one exists to reduce garbage.
			List<ulong> jobHighlights;
			if (extraHighlightLists.Count == 0)
			{
				jobHighlights = new List<ulong>();
			}
			else
			{
				jobHighlights = extraHighlightLists[extraHighlightLists.Count - 1];
				extraHighlightLists.RemoveAt(extraHighlightLists.Count - 1);
			}
			highlightsPerJob.Add(job, jobHighlights);

			//Create one hghlight for each affected job.
			foreach (Vector2i affectedPos in playerGroup.JobQueries.GetAffectedPoses(job))
			{
				jobHighlights.Add(MyUI.TileHighlighter.Instance.CreateHighlight(affectedPos,
																				jobColor_Pending));
				MyUI.TileHighlighter.Instance.SetSprite(jobHighlights[jobHighlights.Count - 1],
														spritesByJobType[job.ThisType]);
			}
		}
		private void Callback_JobDestroyed(PlayerGroup playerGroup, Job job)
		{
			//If the job doesn't affect any tiles, there's no highlights to modify.
			if (!highlightsPerJob.ContainsKey(job))
				return;

			//Remove the highlights.
			foreach (ulong highlightID in highlightsPerJob[job])
				MyUI.TileHighlighter.Instance.DestroyHighlight(highlightID);

			//Remove the highlight list.
			//Save the actual allocated list for later use to reduce garbage.
			highlightsPerJob[job].Clear();
			extraHighlightLists.Add(highlightsPerJob[job]);
			highlightsPerJob.Remove(job);
		}
		private void Callback_JobStarted(PlayerGroup playerGroup, Job job)
		{
			//If the job doesn't affect any tiles, there's no highlights to modify.
			if (!highlightsPerJob.ContainsKey(job))
				return;

			//Update the color of the job highlights.
			foreach (ulong highlightID in highlightsPerJob[job])
				MyUI.TileHighlighter.Instance.SetColor(highlightID, jobColor_Active);
		}
		private void Callback_JobStopped(PlayerGroup playerGroup, Job job)
		{
			//If the job doesn't affect any tiles, or was just destroyed,
			//    then there's no highlights to modify.
			if (!highlightsPerJob.ContainsKey(job))
				return;

			//Update the color of the job highlights.
			foreach (ulong highlightID in highlightsPerJob[job])
				MyUI.TileHighlighter.Instance.SetColor(highlightID, jobColor_Stopped);
		}
	}
}
