using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace MyUI
{
	/// <summary>
	/// A window that prompts the user to select some number of tiles.
	/// </summary>
	public class Window_SelectTiles : Window<Window_SelectTiles.TilesSelectionData>
	{
		public struct TilesSelectionData
		{
            /// <summary>
            /// Null is passed if the user pressed "Cancel".
            /// </summary>
			public Action<HashSet<Vector2i>> OnFinished;
            /// <summary>
            /// Tells the window whether the given tile is allowed in the selection.
            /// </summary>
			public Func<Vector2i, bool> IsTileValid;
            
            /// <summary>
            /// If true, all tiles must be connected to each other.
            /// </summary>
            public bool MustBeConnected;

			public string TitleKey, MessageKey;
			public object[] TitleArgs, MessageArgs;

			public TilesSelectionData(Action<HashSet<Vector2i>> onFinished,
									  Func<Vector2i, bool> isTileValid,
                                      bool mustBeConnected,
									  string title, string message,
									  object[] messageArgs = null,
									  object[] titleArgs = null)
			{
				OnFinished = onFinished;
                IsTileValid = isTileValid;
                MustBeConnected = mustBeConnected;
				TitleKey = title;
				MessageKey = message;
				TitleArgs = titleArgs;
				MessageArgs = messageArgs;
			}
		}


		/// <summary>
		/// It doesn't make sense to have more than one of these open at once.
		/// </summary>
		private static Window_SelectTiles instance = null;

		//TODO: Make a TileHighlight behavior (based on view mode: 2d or 3d) that allocates/distributes "highlights" this window captures and uses.
		public Localizer Label_Title, Label_Message;
        public UnityEngine.UI.Button ConfirmButton;

		private HashSet<Vector2i> currentChoice = new HashSet<Vector2i>();

        private bool canConfirm
        {
            get { return _canConfirm; }
            set
            {
                _canConfirm = value;
                ConfirmButton.interactable = value;
            }
        }
        private bool _canConfirm = true;


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

            canConfirm = !Target.MustBeConnected || IsFullyConnected();

			//Every time the view mode changes,
			//    we need to change what kind of input callback we respond to.
			UnityLogic.Options.OnChanged_ViewMode += Callback_NewViewMode;
			Callback_NewViewMode(UnityLogic.ViewModes.TwoD, UnityLogic.Options.ViewMode);

            UnityLogic.EtMGame.Instance.Map.Tiles.OnTileChanged += Callback_TileChanged;
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			CleanUpCallbacks(UnityLogic.Options.ViewMode);
			UnityLogic.Options.OnChanged_ViewMode -= Callback_NewViewMode;

			if (instance == this)
				instance = null;

            if (UnityLogic.EtMGame.InstanceExists)
                UnityLogic.EtMGame.Instance.Map.Tiles.OnTileChanged -= Callback_TileChanged;
        }

		public void Callback_FinishedChoosingTile(bool didCancel)
		{
			Target.OnFinished(currentChoice);

			Destroy(gameObject);
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
        public void Callback_TileChanged(GameLogic.TileGrid tiles, Vector2i pos,
                                         GameLogic.TileTypes oldType, GameLogic.TileTypes newType)
        {
            if (currentChoice.Contains(pos) && !Target.IsTileValid(pos))
            {
                currentChoice.Remove(pos);
                canConfirm = !Target.MustBeConnected || IsFullyConnected();
            }
        }
		
        /// <summary>
		/// Returns "true" if the given tile is a valid selection.
		/// </summary>
		public bool Callback_WorldTileClicked(Vector2i tilePos)
		{
            //If the tile was already selected, remove it.
            if (currentChoice.Contains(tilePos))
            {
                currentChoice.Remove(tilePos);
                return true;
            }
            //Otherwise, add it to the selection.
            else
            {
                if (Target.IsTileValid(tilePos))
                {
                    currentChoice.Add(tilePos);
                    return true;
                }

			    return false;
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
        private bool IsFullyConnected()
        {
            //Edge case: zero or one tiles selected.
            if (currentChoice.Count < 2)
                return true;

            //Try to find all selected tiles starting from one selected tile.

            HashSet<Vector2i> missingTiles = new HashSet<Vector2i>(currentChoice),
                              foundTiles = new HashSet<Vector2i>(currentChoice),
                              tilesToSearch = new HashSet<Vector2i>();

            Vector2i startPos = missingTiles.First();
            tilesToSearch.Add(startPos);
            missingTiles.Remove(startPos);
            while (tilesToSearch.Count > 0)
            {
                //Mark the front of the search frontier as found.
                Vector2i pos = tilesToSearch.First();
                tilesToSearch.Remove(pos);
                foundTiles.Add(pos);

                //Add adjacent tiles to the search frontier if they're part of the selection.
                if (missingTiles.Contains(pos.LessX))
                {
                    missingTiles.Remove(pos.LessX);
                    tilesToSearch.Add(pos.LessX);
                }
                if (missingTiles.Contains(pos.LessY))
                {
                    missingTiles.Remove(pos.LessY);
                    tilesToSearch.Add(pos.LessY);
                }
                if (missingTiles.Contains(pos.MoreX))
                {
                    missingTiles.Remove(pos.MoreX);
                    tilesToSearch.Add(pos.MoreX);
                }
                if (missingTiles.Contains(pos.MoreY))
                {
                    missingTiles.Remove(pos.MoreY);
                    tilesToSearch.Add(pos.MoreY);
                }
            }

            return missingTiles.Count == 0;
        }
	}
}
