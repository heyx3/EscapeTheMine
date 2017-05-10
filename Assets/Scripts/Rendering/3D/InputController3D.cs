using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.ThreeD
{
	/// <summary>
	/// Handles mouse/tap input.
	/// </summary>
	public class InputController3D : Singleton<InputController3D>
	{
		public void Click(Vector2 mPos)
		{
		}

		public void StartDragging(Vector2 mousePos)
		{
			MouseCursor.Instance.SetCursor(MouseCursor.Cursors.Drag);
		}
		public void DragInput(Vector2 lastMPos, Vector2 currentMPos)
		{
		}
		public void StopDragging(Vector2 startMPos, Vector2 endMPos)
		{
			MouseCursor.Instance.SetCursor(MouseCursor.Cursors.Normal);
		}

		public void Scroll(float scrollWheelAmount, Vector3 mousePos)
		{
		}
	}
}
