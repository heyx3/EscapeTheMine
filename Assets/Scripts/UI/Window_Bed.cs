using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class Window_Bed : Window<GameLogic.Units.Bed>
	{
		public Transform ContentParent;
		public GameObject ElementButtonPrefab;
		public string ElementButtonPrefab_ButtonName_SelectUnit = "\"Select Unit\" Button",
					  ElementButtonPrefab_ButtonName_StopSleeping = "\"Stop Sleeping\" Button";

		private Dictionary<ulong, GameObject> idToContent = new Dictionary<ulong, GameObject>();


		private void Start()
		{
			//Track units being added to/removed from the bed.
			Target.SleepingUnitsByID.OnElementAdded += Callback_AddSleeper;
			Target.SleepingUnitsByID.OnElementRemoved += Callback_RemoveSleeper;

			//Watch out for units getting destroyed while sleeping in the bed.
			Target.TheMap.OnUnitRemoved += Callback_UnitRemoved;

			foreach (ulong id in Target.SleepingUnitsByID)
				Callback_AddSleeper(Target.SleepingUnitsByID, id);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			Target.SleepingUnitsByID.OnElementAdded -= Callback_AddSleeper;
			Target.SleepingUnitsByID.OnElementRemoved -= Callback_RemoveSleeper;
			Target.TheMap.OnUnitRemoved -= Callback_UnitRemoved;
		}

		public void Callback_ZoomToBed()
		{
			switch (UnityLogic.Options.ViewMode)
			{
				case UnityLogic.ViewModes.TwoD:
					Rendering.TwoD.Content2D.Instance.ZoomToSee(Enumerable.Repeat(Target.Pos.Value, 1));
					break;

				case UnityLogic.ViewModes.ThreeD: throw new NotImplementedException();

				default: throw new NotImplementedException(UnityLogic.Options.ViewMode.ToString());
			}
		}

		private void Callback_AddSleeper(LockedSet<ulong> sleepers, ulong unitID)
		{
			GameObject elementButton = Instantiate(ElementButtonPrefab);
			Transform elementButtonTr = elementButton.transform;

			Transform selectUnitButton =
				elementButtonTr.Find(ElementButtonPrefab_ButtonName_SelectUnit);
			selectUnitButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(
					() =>
					{
						ContentUI.Instance.CreateUnitWindow(Target.TheMap.GetUnit(unitID));
					});
			selectUnitButton.GetComponentInChildren<UnityEngine.UI.Text>().text =
				Target.TheMap.GetUnit(unitID).DisplayName;

			elementButtonTr.Find(ElementButtonPrefab_ButtonName_StopSleeping)
				.GetComponent<UnityEngine.UI.Button>()
				.onClick.AddListener(
					() =>
					{
						//End the sleeper's "Sleep" job.
						var sleepingUnit = UnityLogic.EtMGame.Instance.Map.GetUnit(unitID);
						((GameLogic.Units.PlayerChar)sleepingUnit).StopDoingJob(null, true);
					});

			elementButton.transform.SetParent(ContentParent, false);
		}
		private void Callback_RemoveSleeper(LockedSet<ulong> sleepers, ulong unitID)
		{
			Destroy(idToContent[unitID]);
			idToContent.Remove(unitID);
		}

		private void Callback_UnitRemoved(GameLogic.Map map, GameLogic.Unit unit)
		{
			if (idToContent.ContainsKey(unit.ID))
				Callback_RemoveSleeper(Target.SleepingUnitsByID, unit.ID);
		}
	}
}
