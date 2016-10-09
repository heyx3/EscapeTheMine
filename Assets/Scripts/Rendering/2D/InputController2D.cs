using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	/// <summary>
	/// Handles mouse/tap input.
	/// </summary>
	public class InputController2D : MonoBehaviour
	{
		//TODO: Handle pinch to zoom.

		public float ScrollSpeed = 5.0f;
		public float MinDragDist = 10.0f;
		public float ScrollZoomScale = 1.1f;
		
		
		private Vector2? lastMousePos = null,
						 initialMousePos = null;

		private bool isDragging = false;

		
		void Update()
		{
			if (Application.isMobilePlatform)
				if (Input.touchCount == 1)
					ClickInput(Input.GetTouch(0).position);
				else
					NoClickInput();
			else
				if (Input.GetMouseButton(0))
					ClickInput(Input.mousePosition);
				else
					NoClickInput();

			//Scroll wheel for zooming.
			float scrollWheel = Input.mouseScrollDelta.y;
			if (scrollWheel != 0.0f)
				ZoomInput(Mathf.Pow(ScrollZoomScale, scrollWheel));
		}

		private void ClickInput(Vector2 pos)
		{
			if (lastMousePos.HasValue)
			{
				Vector2 delta = pos - lastMousePos.Value;

				if (isDragging)
				{
					MouseCursor.Instance.SetCursor(MouseCursor.Cursors.Drag);

					Vector2 deltaCam = delta * ScrollSpeed * Time.deltaTime *
									   Content2D.Instance.Cam.orthographicSize;
					Content2D.Instance.CamTr.position -= (Vector3)deltaCam;
				}
				else
				{
					MouseCursor.Instance.SetCursor(MouseCursor.Cursors.Normal);

					if (delta.sqrMagnitude > (MinDragDist * MinDragDist))
						isDragging = true;
				}
			}
			else
			{
				initialMousePos = pos;
			}

			lastMousePos = pos;
		}
		private void NoClickInput()
		{
			if (lastMousePos.HasValue)
			{
				MouseCursor.Instance.SetCursor(MouseCursor.Cursors.Normal);

				lastMousePos = null;
				if (!isDragging)
				{
					//TODO: Click on the screen.
				}

				isDragging = false;
			}
		}

		private void ZoomInput(float scale)
		{
			if (Content2D.Instance.Cam.orthographic)
				Content2D.Instance.Cam.orthographicSize /= scale;
			else
				Content2D.Instance.Cam.fieldOfView /= scale;
		}
	}
}
