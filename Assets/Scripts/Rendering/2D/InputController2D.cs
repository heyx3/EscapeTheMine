using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	/// <summary>
	/// Handles mouse/tap input.
	/// </summary>
	public class InputController2D : Singleton<InputController2D>
	{
        /// <summary>
        /// Raised when a world tile is clicked on.
        /// If no responders "cancel" the event, the usual behavior for clicking on the world is done
        ///     (i.e. opening a window for the selected unit).
        /// </summary>
        public CancelableEvent<Vector2i> OnWorldTileClicked = new CancelableEvent<Vector2i>();


		public float ScrollSpeed = 5.0f;
		public float MinDragDist = 10.0f;
		public float ScrollZoomScale = 1.1f;
		

		public void Click(Vector2 mPos)
		{
			Vector2 worldMPos = Content2D.Instance.Cam.ScreenToWorldPoint(mPos);
			GameLogic.Map map = UnityLogic.GameFSM.Instance.Map;

			//Exit if we clicked outside the map.
			Vector2i tilePos = new Vector2i(worldMPos);
			if (!map.Tiles.IsValid(tilePos))
				return;

            //If nothing special happens because of this click,
            //    do the usual behavior -- show a window for the unit(s) that were clicked on.
            if (!OnWorldTileClicked.Raise(tilePos))
            {
                //Get the units in the part of the map we clicked on and show the window for them.
                List<GameLogic.Unit> clickedUnits = map.GetUnitsAt(tilePos).ToList();
                if (clickedUnits.Count == 1)
                {
                    MyUI.ContentUI.Instance.CreateUnitWindow(clickedUnits[0]);
                }
                else if (clickedUnits.Count > 1)
                {
                    MyUI.ContentUI.Instance.CreateWindow(MyUI.ContentUI.Instance.Window_SelectUnit,
                                                         clickedUnits);
                }
            }
		}

		public void StartDragging(Vector2 mousePos)
		{
			MouseCursor.Instance.SetCursor(MouseCursor.Cursors.Drag);
		}
		public void DragInput(Vector2 lastMPos, Vector2 currentMPos)
		{
			Vector2 deltaCam = (currentMPos - lastMPos) *
							   ScrollSpeed * Time.deltaTime *
							   Content2D.Instance.Cam.orthographicSize;
			Content2D.Instance.CamTr.position -= (Vector3)deltaCam;
		}
		public void StopDragging(Vector2 startMPos, Vector2 endMPos)
		{
			MouseCursor.Instance.SetCursor(MouseCursor.Cursors.Normal);
		}

		public void Scroll(float scrollWheelAmount)
		{
			float scale = Mathf.Pow(ScrollZoomScale, scrollWheelAmount);

			UnityEngine.Assertions.Assert.IsTrue(Content2D.Instance.Cam.orthographic);
			Content2D.Instance.Cam.orthographicSize /= scale;
		}
	}
}
