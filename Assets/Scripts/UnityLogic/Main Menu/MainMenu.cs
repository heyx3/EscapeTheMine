using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace UnityLogic
{
	public class MainMenu : Singleton<MainMenu>
	{
		public GameObject Menu_Main, Menu_NewWorld, Menu_LoadWorld;


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

		public void Callback_StartNewGame()
		{
			GameFSM.Instance.GenerateWorld();
		}
		public void Callback_StartLoadedGame()
		{
			GameFSM.Instance.LoadWorld(Path.Combine(LoadWorldMenu.Instance.SaveFolderPath,
													LoadWorldMenu.Instance.SelectedWorld));
		}

		
		private void Activate(GameObject menu)
		{
			Menu_Main.SetActive(Menu_Main == menu);
			Menu_NewWorld.SetActive(Menu_NewWorld == menu);
			Menu_LoadWorld.SetActive(Menu_LoadWorld == menu);
		}


		private void Start()
		{
			Activate(Menu_Main);
		}
	}
}
