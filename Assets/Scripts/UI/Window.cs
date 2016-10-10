using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	/// <summary>
	/// A draggable, closeable set of controls surrounded by a box.
	/// </summary>
	/// <typeparam name="T">
	/// The type of object that this window is the focus of.
	/// </typeparam>
	public class Window<T> : MonoBehaviour
	{
		private static HashSet<Window<T>> allWindows = new HashSet<Window<T>>();

		/// <summary>
		/// Gets all open windows that edit the same kind of object.
		/// </summary>
		public static IEnumerable<Window<T>> AllWindows { get { return allWindows; } }

		protected static UnityLogic.GameFSM FSM { get { return UnityLogic.GameFSM.Instance; } }


		/// <summary>
		/// Raised when the "Close" button is pressed.
		/// </summary>
		public event Action<Window<T>> OnWindowClosed;
		/// <summary>
		/// Raised when the window is dragged around by its title bar.
		/// The first Vector2 is the old position, and the second one is the new position.
		/// </summary>
		public event Action<Window<T>, Vector2, Vector2> OnWindowDragged;


		private T target = default(T);
		private bool isTargetInitialized = false;


		public Transform MyTr { get; private set; }

		public T Target
		{
			get { UnityEngine.Assertions.Assert.IsTrue(isTargetInitialized); return target; }
			set { UnityEngine.Assertions.Assert.IsFalse(isTargetInitialized); target = value; isTargetInitialized = true; }
		}


		protected virtual void Awake()
		{
			MyTr = transform;
			allWindows.Add(this);
		}
		protected virtual void OnDestroy()
		{
			allWindows.Remove(this);
		}


		#region Callbacks

		public void Callback_Button_Close()
		{
			if (OnWindowClosed != null)
				OnWindowClosed(this);

			Destroy(gameObject);
		}
		public void Callback_DragButton_Titlebar()
		{
			Vector2 oldPos = MyTr.position;
			Vector2 newPos = Input.mousePosition;

			MyTr.position = newPos;

			if (OnWindowDragged != null)
				OnWindowDragged(this, oldPos, newPos);
		}

		#endregion
	}
}