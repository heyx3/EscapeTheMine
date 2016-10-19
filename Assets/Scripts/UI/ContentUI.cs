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
		public Window<T> CreateWindowFor<T>(GameObject prefab, T target)
			where T : class
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
		public Window_Global CreateWindow(GameObject prefab)
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
		public GameObject CreateWindowFor(GameLogic.Unit unit)
		{
			if (unit is GameLogic.Units.TestChar)
				return CreateWindowFor(Window_TestChar, (GameLogic.Units.TestChar)unit).gameObject;
			else if (unit is GameLogic.Units.TestStructure)
				return CreateWindowFor(Window_TestStructure, (GameLogic.Units.TestStructure)unit).gameObject;
			else if (unit is GameLogic.Units.PlayerChar)
				return CreateWindowFor(Window_PlayerChar, (GameLogic.Units.PlayerChar)unit).gameObject;
			else
				throw new NotImplementedException(unit.GetType().Name);
		}
	}
}