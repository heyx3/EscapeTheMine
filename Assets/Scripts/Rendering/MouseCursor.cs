using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering
{
	/// <summary>
	/// Handles the mouse cursor.
	/// If the platform is mobile, no cursor will be visible.
	/// </summary>
	public class MouseCursor : Singleton<MouseCursor>
	{
		[SerializeField]
		private Texture2D Cursor_Normal, Cursor_Drag;
		[SerializeField]
		private Vector2 Hotspot_Normal, Hotspot_Drag;


		public enum Cursors { Normal, Drag, }

		
		public void SetCursor(Cursors newType)
		{
			if (Application.isMobilePlatform)
				return;

			switch (newType)
			{
				case Cursors.Normal:
					Cursor.SetCursor(Cursor_Normal, Hotspot_Normal, CursorMode.Auto);
					break;
				case Cursors.Drag:
					Cursor.SetCursor(Cursor_Drag, Hotspot_Drag, CursorMode.Auto);
					break;
				default:
					throw new NotImplementedException(newType.ToString());
			}
		}

		protected override void Awake()
		{
			base.Awake();
			SetCursor(Cursors.Normal);
		}
	}
}