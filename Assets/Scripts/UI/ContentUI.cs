using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	public class ContentUI : Singleton<ContentUI>
	{
		/// <summary>
		/// Prefabs for windows.
		/// </summary>
		public GameObject Window_Options,
						  Window_SelectUnit,
						  Window_SelectTile,

						  Window_ConfirmDialog,

						  Window_TestChar,
						  Window_TestStructure,
						  Window_PlayerChar;


		public Transform TheCanvas
		{
			get
			{
				if (theCanvas == null)
					theCanvas = FindObjectOfType<Canvas>().transform;
				return theCanvas;
			}
		}
		private Transform theCanvas = null;


		/// <summary>
		/// Creates a window for editing the given type of item.
		/// Starts the window in the center of the screen.
		/// Closes any other windows that are editing the same target.
		/// </summary>
		public Window<T> CreateWindow<T>(GameObject prefab, T target)
		{
			GameObject go = Instantiate(prefab);

			//Parent the object to the canvas and move it to the center of the screen.
			Transform tr = go.transform;
			tr.SetParent(TheCanvas, false);
			tr.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);

			//Set up the window.
			Window<T> wnd = go.GetComponent<Window<T>>();
			wnd.Target = target;

			return wnd;
		}


		/// <summary>
		/// Creates a window that has no target.
		/// Starts the window in the center of the screen.
		/// </summary>
		public Window_Global CreateGlobalWindow(GameObject prefab)
		{
			GameObject go = Instantiate(prefab);

			//Parent the object to the canvas and move it to the center of the screen.
			Transform tr = go.transform;
			tr.SetParent(TheCanvas, true);
			tr.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);

			return go.GetComponent<Window_Global>();
		}

		/// <summary>
		/// Creates the correct window for editing the given unit.
		/// Returns the window's GameObject.
		/// </summary>
		public GameObject CreateUnitWindow(GameLogic.Unit unit)
		{
			switch (unit.MyType)
			{
				case GameLogic.Unit.Types.TestChar:
					return CreateWindow(Window_TestChar,
										(GameLogic.Units.TestChar)unit).gameObject;
				case GameLogic.Unit.Types.TestStructure:
					return CreateWindow(Window_TestStructure,
										(GameLogic.Units.TestStructure)unit).gameObject;
				case GameLogic.Unit.Types.PlayerChar:
					return CreateWindow(Window_PlayerChar,
										(GameLogic.Units.PlayerChar)unit).gameObject;
				default:
					throw new NotImplementedException(unit.MyType.ToString());
			}
		}

		/// <summary>
		/// Creates an "OK/Cancel"-type dialog.
		/// Destroys it once an option was selected.
		/// </summary>
		public Window_ConfirmDialog CreateDialog(Action onConfirm, Action onDeny = null)
		{
			Action<Window_ConfirmDialog, bool> onDone = (_wnd, _result) =>
			{
				if (_result && onConfirm != null)
					onConfirm();
				else if (!_result && onDeny != null)
					onDeny();

				Destroy(_wnd.gameObject);
			};

			return (Window_ConfirmDialog)CreateWindow(Window_ConfirmDialog, onDone);
		}
	}
}