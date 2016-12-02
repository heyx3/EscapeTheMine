using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace MyUI
{
	/// <summary>
	/// A window that prompts the user to select a tile.
	/// </summary>
	public class Window_SelectTile : Window<Window_SelectTile.TileSelectionData>
	{
		public struct TileSelectionData
		{
			public Action<Vector2i?> OnFinished;
			public Predicate<Vector2i> IsTileValid;

			public string TitleKey, MessageKey;
			public object[] TitleArgs, MessageArgs;

			public TileSelectionData(Action<Vector2i?> onFinished,
									 Predicate<Vector2i> isTileValid,
									 string title, string message,
									 object[] messageArgs = null,
									 object[] titleArgs = null)
			{
				OnFinished = onFinished;
				IsTileValid = isTileValid;
				TitleKey = title;
				MessageKey = message;
				TitleArgs = titleArgs;
				MessageArgs = messageArgs;
			}
		}


		/// <summary>
		/// It doesn't make sense to have more than one of these open at once.
		/// </summary>
		private static Window_SelectTile instance = null;

        //TODO: Make a TileHighlight behavior (based on view mode: 2d or 3d) that allocates/distributes "highlights" this window captures and uses.
        public Localizer Label_Title, Label_Message;

		private Vector2i? currentChoice = null;


		protected override void Awake()
		{
			base.Awake();

			//If another instance was already open, close it.
			if (instance != null)
				instance.Callback_FinishedChoosingTile(true);
			instance = this;
		}
		private void Start()
		{
			Label_Title.Key = Target.TitleKey;
			Label_Title.Args = Target.TitleArgs;

			Label_Message.Key = Target.MessageKey;
			Label_Message.Args = Target.MessageArgs;

			//Every time the view mode changes,
			//    we need to change what kind of input callback we respond to.
			UnityLogic.Options.OnChanged_ViewMode += Callback_NewViewMode;
			Callback_NewViewMode(UnityLogic.ViewModes.TwoD, UnityLogic.Options.ViewMode);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			CleanUpCallbacks(UnityLogic.Options.ViewMode);
			UnityLogic.Options.OnChanged_ViewMode -= Callback_NewViewMode;

			if (instance == this)
				instance = null;
		}
		private void Update()
		{
			//Double-check that the chosen tile didn't become invalid after the user chose it.
			if (currentChoice.HasValue && !Target.IsTileValid(currentChoice.Value))
				currentChoice = null;
		}

		public void Callback_FinishedChoosingTile(bool didCancel)
		{
			//Double-check that the chosen tile didn't become invalid after the user chose it.
			if (currentChoice.HasValue && !Target.IsTileValid(currentChoice.Value))
				currentChoice = null;

			Target.OnFinished(currentChoice);

			Destroy(gameObject);
		}
		/// <summary>
		/// Returns "true" if the given tile is a valid selection.
		/// </summary>
		public bool Callback_WorldTileClicked(Vector2i tilePos)
		{
			if (Target.IsTileValid(tilePos))
			{
				currentChoice = tilePos;
				return true;
			}
			return false;
		}
		public void Callback_NewViewMode(UnityLogic.ViewModes oldViewMode,
										 UnityLogic.ViewModes newViewMode)
		{
			CleanUpCallbacks(oldViewMode);

			switch (newViewMode)
			{
				case UnityLogic.ViewModes.TwoD:
					Rendering.TwoD.InputController2D.Instance.OnWorldTileClicked.Add(
						Callback_WorldTileClicked);
					break;

				case UnityLogic.ViewModes.ThreeD:
					throw new NotImplementedException();

				default:
					throw new NotImplementedException(newViewMode.ToString());
			}
		}

		private void CleanUpCallbacks(UnityLogic.ViewModes viewMode)
		{
			switch (viewMode)
			{
				case UnityLogic.ViewModes.TwoD:
					Rendering.TwoD.InputController2D.Instance.OnWorldTileClicked.Remove(
						Callback_WorldTileClicked);
					break;

				case UnityLogic.ViewModes.ThreeD:
					throw new NotImplementedException();

				default: throw new NotImplementedException(viewMode.ToString());
			}
		}
	}
}
