using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Rendering
{
	public class RendererController : Singleton<RendererController>
	{
		[Serializable]
		private class DictEntry
		{
			public UnityLogic.ViewModes Mode;
			public GameObject Renderer;

			public DictEntry(UnityLogic.ViewModes mode) { Mode = mode; Renderer = null; }
		}


		[SerializeField]
		private DictEntry[] renderers = new DictEntry[2]
		{
			new DictEntry(UnityLogic.ViewModes.TwoD),
			new DictEntry(UnityLogic.ViewModes.ThreeD),
		};

		private Dictionary<UnityLogic.ViewModes, GameObject> viewModeToRenderer =
			new Dictionary<UnityLogic.ViewModes, GameObject>();


		protected override void Awake()
		{
			base.Awake();

			foreach (DictEntry entry in renderers)
				viewModeToRenderer.Add(entry.Mode, entry.Renderer);

			UnityLogic.Options.OnChanged_ViewMode += Callback_ChangeViewMode;
		}
		private void Start()
		{
			Callback_ChangeViewMode(UnityLogic.ViewModes.TwoD, UnityLogic.Options.ViewMode);
		}

		private void Callback_ChangeViewMode(UnityLogic.ViewModes oldMode, UnityLogic.ViewModes newMode)
		{
			foreach (KeyValuePair<UnityLogic.ViewModes, GameObject> kvp in viewModeToRenderer)
				if (kvp.Value != null)
					kvp.Value.SetActive(kvp.Key == newMode);
		}
	}
}