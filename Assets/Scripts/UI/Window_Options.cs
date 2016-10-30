﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class Window_Options : Window_Global
	{
		public void Callback_Button_Save()
		{
			FSM.SaveWorld();
		}
		public void Callback_Button_Quit()
		{
			//Ask the player for confirmation, and then quit the world.
			ContentUI.Instance.CreateDialog(Localization.Get("WINDOW_CONFIRMQUIT_TITLE"),
											Localization.Get("WINDOW_CONFIRMQUIT_MESSAGE"),
											() => FSM.QuitWorld());
		}
	}
}