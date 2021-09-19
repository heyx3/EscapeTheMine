using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MyUI
{
	//TODO: When a Unit dies, its window should close. Maybe make a UnitWindow base class?

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

		protected static UnityLogic.EtMGame Game { get { return UnityLogic.EtMGame.Instance; } }


		/// <summary>
		/// Raised when the "Close" button is pressed.
		/// </summary>
		public event Action<Window<T>> OnWindowClosed;
		/// <summary>
		/// Raised when the window is dragged around by its title bar.
		/// The first Vector2 is the old position, and the second one is the new position.
		/// </summary>
		public event Action<Window<T>, Vector2, Vector2> OnWindowDragged;


		private bool hasOwner = false;
		private Component owner = null;

		private T target = default(T);
		private bool isTargetInitialized = false;
		private Vector2 dragOffset;


		public Transform MyTr { get; private set; }

		public T Target
		{
			get { UnityEngine.Assertions.Assert.IsTrue(isTargetInitialized); return target; }
			set { UnityEngine.Assertions.Assert.IsFalse(isTargetInitialized); target = value; isTargetInitialized = true; }
		}


		/// <summary>
		/// Ties the lifetime of this window to the given object.
		/// </summary>
		public void SetOwner(GameObject o)
        {
			SetOwner(o.transform);
        }
		/// <summary>
		/// Ties the lifetime of this window to the given Component.
		/// </summary>
		public void SetOwner(Component c)
        {
			hasOwner = true;
			owner = c;
        }

		protected virtual void Awake()
		{
			MyTr = transform;
			allWindows.Add(this);

			Game.OnStart += Callback_MapChanging;
			Game.OnEnd += Callback_MapChanging;
		}
		protected virtual void Update()
        {
			if (hasOwner && owner == null)
				Callback_Button_Close();
        }
		protected virtual void OnDestroy()
		{
			allWindows.Remove(this);

			if (UnityLogic.EtMGame.InstanceExists)
			{
				Game.OnStart -= Callback_MapChanging;
				Game.OnEnd -= Callback_MapChanging;
			}
		}


		#region Callbacks

		public virtual void Callback_Button_Close()
		{
			if (OnWindowClosed != null)
				OnWindowClosed(this);

			Destroy(gameObject);
		}

		public virtual void Callback_StartDragButton_Titlebar()
		{
			dragOffset = (Vector2)MyTr.position - (Vector2)Input.mousePosition;
		}
		public virtual void Callback_DragButton_Titlebar()
		{
			Vector2 oldPos = MyTr.position;
			Vector2 newPos = (Vector2)Input.mousePosition + dragOffset;

			MyTr.position = newPos;

			if (OnWindowDragged != null)
				OnWindowDragged(this, oldPos, newPos);
		}

		/// <summary>
		/// Default behavior: closes this window.
		/// </summary>
		protected virtual void Callback_MapChanging()
		{
			Callback_Button_Close();
		}

		#endregion
	}
}