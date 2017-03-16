using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameLogic;
using GameLogic.Units.Player_Char;


namespace MyUI
{
	public class Window_PlayerChar : Window<GameLogic.Units.PlayerChar>
	{
		private static string FormatFood(float f)
		{
			return Mathf.RoundToInt(f).ToString();
		}
		private static string FormatHealth(float f)
		{
			float maxHealth = Consts.Max_Health;
			int healthPercent = Mathf.RoundToInt(100.0f * f / maxHealth);
			return healthPercent.ToString() + "%";
		}
		private static string FormatEnergy(float f)
		{
			return Mathf.RoundToInt(f).ToString();
		}
		private static string FormatStrength(float f)
		{
			return string.Format("{0:0.00}", f);
		}
		private static string FormatAdultness(float f)
		{
			int percent = Mathf.RoundToInt(100.0f * f);
			return percent.ToString() + "%";
		}
        

		public Localizer Label_Title,
                         Label_FoodValue, Label_HealthValue,
						 Label_EnergyValue, Label_StrengthValue,
						 Label_AdultMultiplier;
        public UnityEngine.UI.InputField Input_MaxMoveToPosDist,
                                         Input_FindBedBelowEnergy,
                                         Input_FindBedBelowHealth;
        public UnityEngine.UI.Toggle Toggle_TakeMiningJobs, Toggle_TakeBuildBedJobs,
									 Toggle_GrowingUpIsEmergency;

		
		public enum TabTypes
		{
			JobSelection,
			Stats,
            GlobalJobs,
		}
		public Dict.UITabsByTabType TabTypeToObj = new Dict.UITabsByTabType(true);

		[SerializeField]
		private TabTypes firstTab = TabTypes.Stats;


		public void SwitchToTab(TabTypes type)
		{
			foreach (var kvp in TabTypeToObj)
				if (kvp.Key == type)
					kvp.Value.SelectMe();
				else
					kvp.Value.DeselectMe();
		}
		
        private void Start()
        {
            Target.Personality.Name.OnChanged += OnNameChanged;
            Label_Title.Args = new object[] { Target.Personality.Name };
            OnNameChanged(Target.Personality, Target.Personality.Name, Target.Personality.Name);

			Target.Food.OnChanged += OnFoodChanged;
			Label_FoodValue.Args = new object[] { FormatFood(Target.Food) };
			OnFoodChanged(Target, Target.Food, Target.Food);

			Target.Health.OnChanged += OnHealthChanged;
			Label_HealthValue.Args = new object[] { FormatHealth(Target.Health) };
			OnHealthChanged(Target, Target.Health, Target.Health);

			Target.Energy.OnChanged += OnEnergyChanged;
			Label_EnergyValue.Args = new object[] { FormatEnergy(Target.Energy) };
			OnEnergyChanged(Target, Target.Energy, Target.Energy);

			Target.Strength.OnChanged += OnStrengthChanged;
			Label_StrengthValue.Args = new object[] { FormatStrength(Target.Strength) };
			OnStrengthChanged(Target, Target.Strength, Target.Strength);

			Target.AdultMultiplier.OnChanged += OnAdultnessChanged;
			Label_AdultMultiplier.Args = new object[] { FormatAdultness(Target.AdultMultiplier) };
			OnAdultnessChanged(Target, Target.AdultMultiplier, Target.AdultMultiplier);

            Input_MaxMoveToPosDist.text = Target.Career.MoveToPos_MaxDist.Value.ToString();
            Toggle_TakeMiningJobs.isOn = Target.Career.AcceptJob_Mining;
			Toggle_TakeBuildBedJobs.isOn = Target.Career.AcceptJob_BuildBed;
            Input_FindBedBelowEnergy.text = Target.Career.SleepWhen_EnergyBelow.Value.ToString();
            Input_FindBedBelowHealth.text = Target.Career.SleepWhen_HealthBelow.Value.ToString();
            Toggle_GrowingUpIsEmergency.isOn = Target.Career.GrowingUpIsEmergency;

			SwitchToTab(firstTab);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			Target.Food.OnChanged -= OnFoodChanged;
			Target.Health.OnChanged -= OnHealthChanged;
			Target.Energy.OnChanged -= OnEnergyChanged;
			Target.Strength.OnChanged -= OnStrengthChanged;
		}

		private void OnFoodChanged(Unit theChar, float oldVal, float newVal)
		{
			Label_FoodValue.Args[0] = FormatFood(newVal);
			Label_FoodValue.OnValidate();
		}
		private void OnHealthChanged(Unit theChar, float oldVal, float newVal)
		{
			Label_HealthValue.Args[0] = FormatHealth(newVal);
			Label_HealthValue.OnValidate();
		}
		private void OnEnergyChanged(Unit theChar, float oldVal, float newVal)
		{
			Label_EnergyValue.Args[0] = FormatEnergy(newVal);
			Label_EnergyValue.OnValidate();
		}
		private void OnStrengthChanged(Unit theChar, float oldVal, float newVal)
		{
			Label_StrengthValue.Args[0] = FormatStrength(newVal);
			Label_StrengthValue.OnValidate();
		}
		private void OnAdultnessChanged(Unit theChar, float oldVal, float newVal)
		{
			Label_AdultMultiplier.Args[0] = FormatAdultness(newVal);
			Label_AdultMultiplier.OnValidate();

            Toggle_GrowingUpIsEmergency.gameObject.SetActive(!Target.IsAdult);

            //If the PlayerChar is now an adult, we need to tell the "Jobs" tab object
            //    to not activate the "Growing up is emergency?" option when clicked.
            Transform tr = Toggle_GrowingUpIsEmergency.transform;
            var childrenToIgnore = TabTypeToObj[TabTypes.GlobalJobs].ChildrenToIgnore;
            if (Target.IsAdult && !childrenToIgnore.Contains(tr))
                childrenToIgnore.Add(tr);
            else if (!Target.IsAdult && childrenToIgnore.Contains(tr))
                childrenToIgnore.Remove(tr);
		}
        private void OnNameChanged(Personality personality, string oldName, string newName)
        {
            Label_Title.Args[0] = newName;
            Label_Title.OnValidate();
        }
        
		public void Callback_TabClicked(UITab tab)
		{
			//Find the type of tab that was clicked on and select it.
			foreach (var kvp in TabTypeToObj)
				if (kvp.Value == tab)
				{
					SwitchToTab(kvp.Key);
					return;
				}

			throw new ArgumentException(tab.name);
		}
		public void Callback_ZoomToPlayer()
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
        
        public void Callback_NewJob_MoveToPos(bool makeEmergency)
		{
			//Ask the player to select a tile to move to.
			var data = new Window_SelectTile.TileSelectionData(
				(tilePos) =>
				{
					if (tilePos.HasValue)
						Target.AddJob(new Job_MoveToPos(tilePos.Value, makeEmergency, Game.Map));
				},
				(tilePos) => !Game.Map.Tiles[tilePos].BlocksMovement(),
				"WINDOW_MOVETOPOS_TITLE", "WINDOW_MOVETOPOS_MESSAGE");
			ContentUI.Instance.CreateWindow(ContentUI.Instance.Window_SelectTile, data);
		}
        public void Callback_NewJob_Mine(bool makeEmergency)
        {
            var data = new Window_SelectTiles.TilesSelectionData(
                (tilePoses) =>
                {
                    if (tilePoses != null)
                        Target.AddJob(new Job_Mine(tilePoses, makeEmergency, Game.Map));
                },
                (tilePos) => Game.Map.Tiles[tilePos].IsMinable(),
                true, "WINDOW_MINEPOSES_TITLE", "WINDOW_MINEPOSES_MESSAGE");
            ContentUI.Instance.CreateWindow(ContentUI.Instance.Window_SelectTiles, data);
        }
		public void Callback_NewJob_BuildBed(bool makeEmergency)
		{
			//Ask the player to select a tile to move to.
			var data = new Window_SelectTile.TileSelectionData(
				(tilePos) =>
				{
					if (tilePos.HasValue)
					{
						Target.AddJob(new Job_BuildBed(tilePos.Value, Target.MyGroupID,
													   makeEmergency, Game.Map));
					}
				},
				(tilePos) => Game.Map.Tiles[tilePos].IsBuildableOn(),
				"WINDOW_BUILDBED_TITLE", "WINDOW_BUILDBED_MESSAGE");
			ContentUI.Instance.CreateWindow(ContentUI.Instance.Window_SelectTile, data);
		}

		public enum BedSleepTypes
		{
			General = 0,
			GeneralEmergency,
			Specific,
			SpecificEmergency,
		}
		public void Callback_NewJob_SleepBed(int sleepTypeInt)
		{
			bool chooseBed = (sleepTypeInt == (int)BedSleepTypes.Specific ||
							  sleepTypeInt == (int)BedSleepTypes.SpecificEmergency),
				 isEmergency = (sleepTypeInt == (int)BedSleepTypes.GeneralEmergency ||
								sleepTypeInt == (int)BedSleepTypes.SpecificEmergency);

			//If the player wants to choose a specific bed, let him.
			if (chooseBed)
			{
				var data = new Window_SelectTile.TileSelectionData(
					(tilePos) =>
					{
						if (tilePos.HasValue)
						{
							var bed = Job_SleepBed.FirstFriendlyBedAtPos(tilePos.Value, Target);
							Target.AddJob(new Job_SleepBed(isEmergency, Game.Map, bed));
						}
					},
					(tilePos) => (Job_SleepBed.FirstFriendlyBedAtPos(tilePos, Target) != null),
					"WINDOW_SLEEPAT_TITLE", "WINDOW_SLEEPAT_MESSAGE");
				ContentUI.Instance.CreateWindow(ContentUI.Instance.Window_SelectTile, data);
			}
			//Otherwise, just sleep at the closest bed.
			else
			{
				Target.AddJob(new Job_SleepBed(isEmergency, Game.Map));
			}
		}

        public void Callback_ChangeStat_MaxMoveToPosDist(string newVal)
        {
            int i;
            if (int.TryParse(newVal, out i) && i >= 0)
                Target.Career.MoveToPos_MaxDist.Value = i;
        }
        public void Callback_ChangeStat_TakeMiningJobs(bool shouldTake)
        {
            Target.Career.AcceptJob_Mining.Value = shouldTake;
        }
		public void Callback_ChangeStat_TakeBuildBedJobs(bool shouldTake)
		{
			Target.Career.AcceptJob_BuildBed.Value = shouldTake;
		}
        public void Callback_ChangeStat_SleepEnergyThreshold(string newVal)
        {
            float f;
            if (float.TryParse(newVal, out f) && f >= 0.0f)
                Target.Career.SleepWhen_EnergyBelow.Value = f;
        }
        public void Callback_ChangeStat_SleepHealthThreshold(string newVal)
        {
            float f;
            if (float.TryParse(newVal, out f) && f >= 0.0f)
                Target.Career.SleepWhen_HealthBelow.Value = f;
        }
    }
}