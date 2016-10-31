using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;


namespace UnityLogic
{
	public class NewWorldMenu : Singleton<NewWorldMenu>
	{
		public GameFSM.WorldSettings Settings { get { return GameFSM.Instance.Settings; } }

		[SerializeField]
		private UnityEngine.UI.Text ui_Message,
									ui_WorldName,
									ui_WorldSize;

		private float messageTimer = -1.0f;
		private bool resetWorldName = false;

		
		public void SetMessage(string msg, float time = -1.0f)
		{
			messageTimer = time;
			ui_Message.text = msg;
		}

		public void Callback_WorldNameChanged(string newValue)
		{
			Settings.Name = newValue;

			//If the name is invalid, swap out the bad characters with underscores.
			if (ui_WorldName.text.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
			{
				SetMessage(Localization.Get("NEWWORLDMENU_MSG_INVALIDCHARS"), 5.0f);
				resetWorldName = true;
				
				UnityEngine.Assertions.Assert.IsFalse(Path.GetInvalidFileNameChars().Contains('_'));
				foreach (char c in Path.GetInvalidFileNameChars())
					Settings.Name = Settings.Name.Replace(c, '_');
			}
			//If a file with that name already exists, warn.
			else if (File.Exists(MenuConsts.Instance.GetSaveFilePath(Settings.Name)))
			{
				SetMessage(Localization.Get("NEWWORLDMENU_MSG_WORLDEXISTS"), 2.0f);

            }
			else
			{
				SetMessage("");
			}
		}
		public void Callback_WorldSizeChanged(string newValue)
		{
			int size;
			if (int.TryParse(newValue, out size) && size > 32)
			{
				Settings.Size = size;
				SetMessage("");
			}
			else
			{
				SetMessage(Localization.Get("NEWWORLDMENU_MSG_INVALIDSIZE"), 1.0f);
            }
		}
		public void Callback_StartNewGame()
		{
            MenuController.Instance.Activate(null);
			GameFSM.Instance.GenerateWorld();
		}
		public void Callback_WorldGenSettings()
		{
			MenuController.Instance.Activate(MenuController.Instance.Menu_WorldGenSettings);
		}

		private void OnEnable()
		{
			ui_Message.text = "";
			ui_WorldName.text = Settings.Name;
			ui_WorldSize.text = Settings.Size.ToString();
		}
		private void Update()
		{
			if (resetWorldName)
			{
				resetWorldName = false;
				ui_WorldName.text = Settings.Name;
			}

			if (messageTimer > 0.0f)
			{
				messageTimer -= Time.deltaTime;
				if (messageTimer <= 0.0f)
					SetMessage("");
			}
		}
	}
}