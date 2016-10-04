using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Rendering
{
	public class RendererController : Singleton<RendererController>
	{
		public void UseRenderer(UnityLogic.GameFSM.ViewModes mode)
		{
			foreach (KeyValuePair<UnityLogic.GameFSM.ViewModes, GameObject> kvp in viewModeToRenderer)
				if (kvp.Value != null)
					kvp.Value.SetActive(kvp.Key == mode);
		}


		[Serializable]
		private class DictEntry
		{
			public UnityLogic.GameFSM.ViewModes Mode;
			public GameObject Renderer;

			public DictEntry(UnityLogic.GameFSM.ViewModes mode) { Mode = mode;  Renderer = null; }
		}


		[SerializeField]
		private DictEntry[] renderers = new DictEntry[2]
		{
			new DictEntry(UnityLogic.GameFSM.ViewModes.TwoD),
			new DictEntry(UnityLogic.GameFSM.ViewModes.ThreeD),
		};

		private Dictionary<UnityLogic.GameFSM.ViewModes, GameObject> viewModeToRenderer =
			new Dictionary<UnityLogic.GameFSM.ViewModes, GameObject>();


		protected override void Awake()
		{
			base.Awake();

			foreach (DictEntry entry in renderers)
				viewModeToRenderer.Add(entry.Mode, entry.Renderer);
		}
	}
}