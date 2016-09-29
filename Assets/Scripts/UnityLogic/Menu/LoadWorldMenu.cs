using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace UnityLogic
{
	public class LoadWorldMenu : Singleton<LoadWorldMenu>
	{
		public UnityEngine.UI.Dropdown SelectionDropdown;
		public UnityEngine.UI.Text DeleteButtonLabel;

		[NonSerialized]
		public string SelectedWorld = null;

		private List<string> availableWorlds;

		private float timeTillNoDelete = -1.0f;


		public void Callback_SelectionChanged(int newIndex)
		{
			SelectedWorld = availableWorlds[newIndex];
		}
		public void Callback_StartLoadedGame()
		{
			GameFSM.Instance.LoadWorld(SelectedWorld);
		}
		public void Callback_Delete()
		{
			if (SelectedWorld == null)
				return;

			if (timeTillNoDelete > 0.0f)
			{
				try
				{
					File.Delete(MenuConsts.SaveFilePath(SelectedWorld));
				}
				catch (Exception e)
				{
					Debug.LogError("Couldn't delete " + SelectedWorld + ": " + e.Message);
				}
				DeleteButtonLabel.text = "Delete";
			}
			else
			{
				timeTillNoDelete = 2.0f;
				DeleteButtonLabel.text = "Confirm?";
			}
		}

		private void OnEnable()
		{
			if (!Directory.Exists(MenuConsts.SaveFolderPath))
				Directory.CreateDirectory(MenuConsts.SaveFolderPath);

			string[] worldFiles = Directory.GetFiles(MenuConsts.SaveFolderPath, "*.world");
			availableWorlds = worldFiles.Select<string, string>(Path.GetFileNameWithoutExtension).ToList();

			SelectionDropdown.options = availableWorlds.Select(s => new UnityEngine.UI.Dropdown.OptionData(s)).ToList();

			if (availableWorlds.Count > 0)
				SelectedWorld = availableWorlds[0];
			else
				SelectedWorld = null;
		}
		private void Update()
		{
			if (timeTillNoDelete > 0.0f)
			{
				timeTillNoDelete -= Time.deltaTime;
				if (timeTillNoDelete <= 0.0f)
				{
					timeTillNoDelete = -1.0f;
					DeleteButtonLabel.text = "Delete";
				}
			}
		}
	}
}