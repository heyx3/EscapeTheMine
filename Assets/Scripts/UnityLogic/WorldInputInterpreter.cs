using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace UnityLogic
{
	/// <summary>
	/// Handles click/tap and scroll/pinch input on the world
	///     (as opposed to being used on the UI).
	/// </summary>
	public class WorldInputInterpreter : MonoBehaviour
	{
		public KeyCode Key_ScrollUp = KeyCode.Equals,
					   Key_ScrollDown = KeyCode.Minus;

		private bool isClicking = false,
					 isDragging = false;
		private Vector2 startMousePos, lastMousePos;
		

		private Rendering.TwoD.InputController2D Input2D { get { return Rendering.TwoD.InputController2D.Instance; } }
		private Rendering.ThreeD.InputController3D Input3D { get { return Rendering.ThreeD.InputController3D.Instance; } }
		
		private bool Use2D { get { return Input2D != null && Input2D.gameObject.activeSelf; } }
		private bool Use3D { get { return Input3D != null && Input3D.gameObject.activeSelf; } }


		public void Callback_StartDrag()
		{
			isDragging = true;

			lastMousePos = Input.mousePosition;
			startMousePos = lastMousePos;

			if (Use2D)
				Input2D.StartDragging(lastMousePos);
			if (Use3D)
				Input3D.StartDragging(lastMousePos);
		}
		public void Callback_EndDrag()
		{
			isClicking = false;
			isDragging = false;

			if (Use2D)
			{
				Input2D.DragInput(lastMousePos, Input.mousePosition);
				Input2D.StopDragging(startMousePos, Input.mousePosition);
			}
			if (Use3D)
			{
				Input3D.DragInput(lastMousePos, Input.mousePosition);
				Input3D.StopDragging(startMousePos, Input.mousePosition);
			}
		}

		public void Callback_PointerDown()
		{
			isClicking = true;
		}
		public void Callback_PointerClick()
		{
			if (!isClicking)
				return;
			isClicking = false;
			
			if (!isDragging)
			{
				if (Use2D)
					Input2D.Click(Input.mousePosition);
				if (Use3D)
					Input3D.Click(Input.mousePosition);
			}
		}

		private void Update()
		{
			if (isDragging)
			{
				if (Use2D)
					Input2D.DragInput(lastMousePos, Input.mousePosition);
				if (Use3D)
					Input3D.DragInput(lastMousePos, Input.mousePosition);

				lastMousePos = Input.mousePosition;
			}


			if (Application.isMobilePlatform)
			{
				//TODO: Detect pinching. Maybe we can use the same component that raises drag/click events?
			}
			else
			{
				//Only check the scroll wheel if the user is currently dragging the camera,
				//    to avoid issues when they're interacting with UI widgets.
				float scrollWheel = 0.0f;
				if (isDragging)
					scrollWheel += Input.mouseScrollDelta.y;
				if (Input.GetKeyDown(Key_ScrollUp))
					scrollWheel += 1;
				else if (Input.GetKeyDown(Key_ScrollDown))
					scrollWheel -= 1;

				if (scrollWheel != 0.0f)
				{
					var zoomScreenCenter = isDragging ?
										       lastMousePos :
											   (new Vector2(Screen.width, Screen.height) / 2);

					if (Use2D)
						Input2D.Scroll(scrollWheel, zoomScreenCenter);
					if (Use3D)
						Input3D.Scroll(scrollWheel, zoomScreenCenter);
				}
			}
		}
	}
}
