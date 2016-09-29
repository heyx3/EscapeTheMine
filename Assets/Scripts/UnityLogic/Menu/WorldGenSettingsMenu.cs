using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace UnityLogic
{
	public class WorldGenSettingsMenu : Singleton<WorldGenSettingsMenu>
	{
		public UnityLogic.MapGen.BiomeGenSettings Biome { get { return GameFSM.Instance.WorldSettings.Biome; } }
		public UnityLogic.MapGen.RoomGenSettings Rooms { get { return GameFSM.Instance.WorldSettings.Rooms; } }

		[SerializeField]
		private UnityEngine.UI.Text ui_Message,
									ui_BiomeNoiseOctaves,
									ui_BiomeNoiseScale,
									ui_BiomeNoisePersistence,
									ui_RoomsNumber,
									ui_RoomsSpacing,
									ui_RoomsSize,
									ui_RoomsNIterations,
									ui_RoomsTileVariation;
		[SerializeField]
		private UnityEngine.UI.Text[] UIs_RoomsTileChangeChances = new UnityEngine.UI.Text[9];

		private float messageTimer = -1.0f;
		
		
		public void SetMessage(string msg, float time = -1.0f)
		{
			messageTimer = time;
			ui_Message.text = msg;
		}

		public void Callback_NewWorldMenu()
		{
			MenuController.Instance.Activate(MenuController.Instance.Menu_NewWorld);
		}
		public void Callback_BiomeNoiseOctavesChanged(string newValue)
		{
			int valI;
			if (int.TryParse(newValue, out valI) && valI > 0)
			{
				Biome.Noise.NOctaves = valI;
				SetMessage("");
			}
			else
			{
				SetMessage("Invalid value");
			}
		}
		public void Callback_BiomeNoiseScaleChanged(string newValue)
		{
			float valF;
			if (float.TryParse(newValue, out valF))
			{
				Biome.Noise.StartScale = valF;
				SetMessage("");
			}
			else
			{
				SetMessage("Invalid value");
			}
		}
		public void Callback_BiomeNoisePersistenceChanged(string newValue)
		{
			float valF;
			if (float.TryParse(newValue, out valF))
			{
				Biome.Noise.Persistence = valF;
				SetMessage("");
			}
			else
			{
				SetMessage("Invalid value");
			}
		}
		public void Callback_RoomsNumberChanged(string newValue)
		{
			int valI;
			if (int.TryParse(newValue, out valI) && valI > 0)
			{
				Rooms.NRooms = valI;
				SetMessage("");
			}
			else
			{
				SetMessage("Invalid value");
			}
		}
		public void Callback_RoomsSpacingChanged(string newValue)
		{
			int valI;
			if (int.TryParse(newValue, out valI) && valI > 0)
			{
				Rooms.RoomSpacing = valI;
				SetMessage("");
			}
			else
			{
				SetMessage("Invalid value");
			}
		}
		public void Callback_RoomsSizeChanged(string newValue)
		{
			float valF;
			if (float.TryParse(newValue, out valF) && valF >= 0.0f && valF <= 1.0f)
			{
				Rooms.RoomSize = valF;
				SetMessage("");
			}
			else
			{
				SetMessage("Invalid value");
			}
		}
		public void Callback_RoomsNIterationsChanged(string newValue)
		{
			int valI;
			if (int.TryParse(newValue, out valI) && valI >= 0)
			{
				Rooms.NIterations = valI;
				SetMessage("");
			}
			else
			{
				SetMessage("Invalid value");
			}
		}
		public void Callback_RoomsTileVariationChanged(string newValue)
		{
			float valF;
			if (float.TryParse(newValue, out valF) && valF >= 0.0f && valF <= 1.0f)
			{
				Rooms.TileVariation = valF;
				SetMessage("");
			}
			else
			{
				SetMessage("Invalid value");
			}
		}
		public void Callback_RoomsTileChangeChanceChanged(string newValue, int index)
		{
			float valF;
			if (float.TryParse(newValue, out valF) && valF >= 0.0f && valF <= 1.0f)
			{
				Rooms.TileChangeChances[index] = valF;
				SetMessage("");
			}
			else
			{
				SetMessage("Invalid value");
			}
		}

		private void OnEnable()
		{

		}
		private void Update()
		{
			//Update the UI message.
			if (messageTimer > 0.0f)
			{
				messageTimer -= Time.deltaTime;
				if (messageTimer <= 0.0f)
				{
					messageTimer = -1.0f;
					ui_Message.text = "";
				}
			}
		}
	}
}
