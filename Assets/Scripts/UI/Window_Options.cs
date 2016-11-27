using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class Window_Options : Window_Global
	{
		public void Callback_Button_Save()
		{
			Game.SaveWorld();
		}
		public void Callback_Button_Quit()
		{
			//Ask the player for confirmation, and then quit the world.
			var wnd = ContentUI.Instance.CreateDialog(() => Game.QuitWorld());
			wnd.Label_Title.Key = "WINDOW_CONFIRMQUIT_TITLE";
			wnd.Label_Message.Key = "WINDOW_CONFIRMQUIT_MESSAGE";
		}
	}
}