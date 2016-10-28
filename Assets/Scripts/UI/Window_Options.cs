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
			FSM.SaveWorld();
		}
		public void Callback_Button_Quit()
		{
			//Ask the player for confirmation, and then quit the world.
			//TODO: Localize.
			ContentUI.Instance.CreateDialog("Quit?",
											"Are you sure you want to quit?\nMake sure to save first!",
											() => FSM.QuitWorld());
		}
	}
}