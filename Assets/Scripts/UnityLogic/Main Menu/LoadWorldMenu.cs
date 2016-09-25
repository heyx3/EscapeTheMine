using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace UnityLogic
{
	public class LoadWorldMenu : Singleton<LoadWorldMenu>
	{
		public const string SaveFolderName = "Saves";

		public string SaveFolderPath { get { return Path.Combine(Application.dataPath, SaveFolderName); } }


		public UnityEngine.UI.Dropdown SelectionDropdown;

		[NonSerialized]
		public string SelectedWorld = null;

		private List<string> availableWorlds;


		public void Callback_SelectionChanged(int newIndex)
		{
			SelectedWorld = availableWorlds[newIndex];
		}

		private void OnEnable()
		{
			if (!Directory.Exists(SaveFolderPath))
				Directory.CreateDirectory(SaveFolderPath);

			string[] worldFiles = Directory.GetFiles(SaveFolderPath, "*.world");
			availableWorlds = worldFiles.Select<string, string>(Path.GetFileNameWithoutExtension).ToList();

			SelectionDropdown.options = availableWorlds.Select(s => new UnityEngine.UI.Dropdown.OptionData(s)).ToList();

			if (availableWorlds.Count > 0)
				SelectedWorld = availableWorlds[0];
			else
				SelectedWorld = null;
		}
	}
}