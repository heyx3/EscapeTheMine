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
			public event Action<Vector2i?> OnFinished;
			public Predicate<Vector2i> IsTileValid;

			public string TitleKey, MessageKey;
			public object[] TitleArgs, MessageArgs;

			public TileSelectionData(Predicate<Vector2i> isTileValid,
									 string title, string message,
									 object[] messageArgs = null,
									 object[] titleArgs = null)
			{
				OnFinished = null;
				IsTileValid = isTileValid;
				TitleKey = title;
				MessageKey = message;
				TitleArgs = titleArgs;
				MessageArgs = messageArgs;
			}

			public void Raise_OnFinished(Vector2i? selectedTile)
			{
				if (OnFinished != null)
					OnFinished(selectedTile);
			}
		}


		/// <summary>
		/// It doesn't make sense to have more than one of these open at once.
		/// </summary>
		private static Window_SelectTile instance = null;


        public Localizer Label_Title, Label_Message;
        public Color TileGoodHighlightColor = Color.yellow,
                     TileBadHighlightColor = Color.red;

        private ulong tileHighlight;

        private Vector2i mousedOverTile
        {
            get
            {
                switch (UnityLogic.Options.ViewMode)
                {
                    case UnityLogic.ViewModes.TwoD:
                        return Rendering.TwoD.InputController2D.Instance.MouseTilePos;
                    case UnityLogic.ViewModes.ThreeD:
                        throw new NotImplementedException();

                    default:
                        throw new NotImplementedException(UnityLogic.Options.ViewMode.ToString());
                }
            }
        }


		protected override void Awake()
		{
			base.Awake();

            //If another instance was already open, close it.
            if (instance != null)
                Destroy(instance.gameObject);
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

            tileHighlight = TileHighlighter.Instance.CreateHighlight(mousedOverTile,
                                                                     TileGoodHighlightColor);
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();

			CleanUpCallbacks(UnityLogic.Options.ViewMode);
			UnityLogic.Options.OnChanged_ViewMode -= Callback_NewViewMode;

            TileHighlighter.Instance.DestroyHighlight(tileHighlight);

			if (instance == this)
				instance = null;
        }
        protected override void Update()
        {
            Vector2i mTilePos = mousedOverTile;

            TileHighlighter.Instance.SetPos(tileHighlight, mTilePos);
            TileHighlighter.Instance.SetColor(tileHighlight,
                                              Target.IsTileValid(mTilePos) ?
                                                  TileGoodHighlightColor :
                                                  TileBadHighlightColor);

			base.Update();
		}

		public override void Callback_Button_Close()
        {
            Target.Raise_OnFinished(null);

            base.Callback_Button_Close();
        }

        /// <summary>
        /// Returns "true" if the given tile is a valid selection.
        /// </summary>
        public bool Callback_WorldTileClicked(Vector2i tilePos)
		{
			if (Target.IsTileValid(tilePos))
			{
                Target.Raise_OnFinished(tilePos);
                Destroy(gameObject);
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
                    if (Rendering.TwoD.InputController2D.Instance != null)
                    {
                        Rendering.TwoD.InputController2D.Instance.OnWorldTileClicked.Remove(
                            Callback_WorldTileClicked);
                    }
                    break;

				case UnityLogic.ViewModes.ThreeD:
					throw new NotImplementedException();

				default: throw new NotImplementedException(viewMode.ToString());
			}
		}
	}
}
