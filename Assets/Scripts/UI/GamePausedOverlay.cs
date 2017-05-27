using System;
using UnityEngine;


namespace MyUI
{
	public class GamePausedOverlay : MonoBehaviour
	{
		private UnityLogic.EtMGame Game { get { return UnityLogic.EtMGame.Instance; } }


		private void Start()
		{
			gameObject.SetActive(Game.IsInGame && Game.Map.IsPaused);

			Game.Map.IsPaused.OnChanged += Callback_PauseToggled;
			Game.OnStart += Callback_MapStart;
			Game.OnEnd += Callback_MapEnd;
		}

		private void Callback_PauseToggled(GameLogic.Map map, bool oldVal, bool newVal)
		{
			gameObject.SetActive(Game.IsInGame && newVal);
		}
		private void Callback_MapStart()
		{
			gameObject.SetActive(Game.Map.IsPaused);
		}
		private void Callback_MapEnd()
		{
			gameObject.SetActive(false);
		}
	}
}