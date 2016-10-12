using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace UnityLogic
{
	public enum ViewModes
	{
		ThreeD,
		TwoD,
	}


	/// <summary>
	/// Stores all options.
	/// Uses Unity's "PlayerPrefs" class so that saving/loading them is automatic.
	/// </summary>
	public static class Options
	{
		public static ViewModes ViewMode
		{
			get { return (ViewModes)PlayerPrefs.GetInt("viewMode", (int)ViewModes.TwoD); }
			set
			{
				ViewModes old = ViewMode;
				PlayerPrefs.SetInt("viewMode", (int)value);

				if (OnChanged_ViewMode != null)
					OnChanged_ViewMode(old, value);
			}
		}
		public static event Action<ViewModes, ViewModes> OnChanged_ViewMode;

		public static float UnitTurnInterval
		{
			get { return PlayerPrefs.GetFloat("unitTurnInterval", 0.1f); }
			set
			{
				float old = UnitTurnInterval;
				PlayerPrefs.SetFloat("unitTurnInterval", value);

				if (OnChanged_ViewMode != null)
					OnChanged_UnitTurnInterval(old, value);
			}
		}
		public static event Action<float, float> OnChanged_UnitTurnInterval;


		/// <summary>
		/// Forces all options to be saved to disk immediately.
		/// This happens automatically when the application exits.
		/// Can be useful in case the game crashes.
		/// </summary>
		public static void Flush()
		{
			PlayerPrefs.Save();
		}
	}
}