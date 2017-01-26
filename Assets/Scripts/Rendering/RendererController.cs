using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Rendering
{
	public class RendererController : Singleton<RendererController>
	{
		public Dict.GameObjectsByViewMode RenderersByViewMode = new Dict.GameObjectsByViewMode(true);

		protected override void Awake()
		{
			base.Awake();
			
			UnityLogic.Options.OnChanged_ViewMode += Callback_ChangeViewMode;
		}
		private void Start()
		{
			Callback_ChangeViewMode(UnityLogic.ViewModes.TwoD, UnityLogic.Options.ViewMode);
		}

		private void Callback_ChangeViewMode(UnityLogic.ViewModes oldMode, UnityLogic.ViewModes newMode)
		{
			foreach (var kvp in RenderersByViewMode)
				if (kvp.Value != null)
					kvp.Value.SetActive(kvp.Key == newMode);
		}
	}
}