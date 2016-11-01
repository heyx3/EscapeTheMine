using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace UnityLogic
{
	public class WorldGenSettingsMenu : Singleton<WorldGenSettingsMenu>
	{
		public UnityLogic.MapGen.BiomeGenSettings Biome { get { return GameFSM.Instance.Settings.Biome; } }
		public UnityLogic.MapGen.RoomGenSettings Rooms { get { return GameFSM.Instance.Settings.Rooms; } }
		public UnityLogic.MapGen.CAGenSettings CA {  get { return GameFSM.Instance.Settings.CA; } }

		[SerializeField]
		private UnityEngine.UI.Text ui_Message,
									ui_BiomeNoiseOctaves,
									ui_BiomeNoiseScale,
									ui_BiomeNoisePersistence,
									ui_RoomsNumber,
									ui_RoomsSpacing,
									ui_MinCirclesPerRoom,
									ui_MaxCirclesPerRoom,
									ui_CirclePosVariance,
									ui_CircleMinRadius,
									ui_CircleMaxRadius,
									ui_CA_NIterations,
									ui_CA_TileVariation;

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
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_0"));

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
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE"));
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
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE"));
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
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_0"));
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
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_0"));
			}
		}
		public void Callback_CA_NIterationsChanged(string newValue)
		{
			int valI;
			if (int.TryParse(newValue, out valI) && valI >= 0)
			{
				CA.NIterations = valI;
				SetMessage("");
			}
			else
			{
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_POS"));
			}
		}
		public void Callback_RoomsMinCircPerRoomChanged(string newValue)
		{
			int valI;
			if (int.TryParse(newValue, out valI) && valI > 0)
			{
				Rooms.MinCirclesPerRoom = valI;
				SetMessage("");
			}
			else
			{
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_0"));
			}
		}
		public void Callback_RoomsMaxCircPerRoomChanged(string newValue)
		{
			int valI;
			if (int.TryParse(newValue, out valI) && valI >= Rooms.MinCirclesPerRoom)
			{
				Rooms.MaxCirclesPerRoom = valI;
				SetMessage("");
			}
			else
			{
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_MINCIRCLES"));
			}
		}
		public void Callback_CircPosVarianceChanged(string newValue)
		{
			float valF;
			if (float.TryParse(newValue, out valF) && valF >= 0.0f && valF <= 1.0f)
			{
				Rooms.CirclePosVariance = valF;
				SetMessage("");
			}
			else
			{
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_01"));
			}
		}
		public void Callback_CircMinRadiusChanged(string newValue)
		{
			float valF;
			if (float.TryParse(newValue, out valF) && valF >= 0.0f)
			{
				Rooms.CircleMinRadius = valF;
				SetMessage("");
			}
			else
			{
				SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_POS"));
			}
		}
		public void Callback_CircMaxRadiusChanged(string newValue)
		{
			float valF;
			if (float.TryParse(newValue, out valF) && valF >= Rooms.CircleMinRadius)
			{
				Rooms.CircleMaxRadius = valF;
				SetMessage("");
			}
			else
            {
                SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_CIRCLEMINRAD"));
            }
		}
		public void Callback_CA_TileVariationChanged(string newValue)
		{
			float valF;
			if (float.TryParse(newValue, out valF) && valF >= 0.0f && valF <= 1.0f)
			{
				CA.TileVariation = valF;
				SetMessage("");
			}
			else
            {
                SetMessage(Localization.Get("WORLDGENMENU_MSG_INVALIDVALUE_01"));
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
