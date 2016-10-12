using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace UnityLogic
{
	public class MenuController : Singleton<MenuController>
	{
		public GameObject Menu_Main, Menu_NewWorld, Menu_LoadWorld, Menu_WorldGenSettings;


		public void Callback_SetupNewGame()
		{
			Activate(Menu_NewWorld);
		}
		public void Callback_LoadGame()
		{
			Activate(Menu_LoadWorld);
		}
		public void Callback_Quit()
		{
			Application.Quit();
		}

		public void Callback_ToMainMenu()
		{
			Activate(Menu_Main);
		}

		public void Callback_ToggleViewMode(UnityEngine.UI.Image viewModeImage)
		{
			switch (Options.ViewMode)
			{
				case ViewModes.TwoD:
					Options.ViewMode = ViewModes.ThreeD;
					viewModeImage.sprite = MenuConsts.Instance.ViewMode_3D;
					break;

				case ViewModes.ThreeD:
					Options.ViewMode = ViewModes.TwoD;
					viewModeImage.sprite = MenuConsts.Instance.ViewMode_2D;
					break;

				default: throw new NotImplementedException(Options.ViewMode.ToString());
			}
		}

		
		public void Activate(GameObject menu)
		{
			Menu_Main.SetActive(Menu_Main == menu);
			Menu_NewWorld.SetActive(Menu_NewWorld == menu);
			Menu_LoadWorld.SetActive(Menu_LoadWorld == menu);
			Menu_WorldGenSettings.SetActive(Menu_WorldGenSettings == menu);
		}


		private void Start()
		{
			Activate(Menu_Main);
		}
	}
}
