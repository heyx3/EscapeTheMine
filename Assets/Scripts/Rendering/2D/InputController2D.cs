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


		public float ScrollSpeed = 0.44f;
		public float MinDragDist = 10.0f;
		public float ScrollZoomScale = 1.1f;

		public Color TileHighlightColor = new Color(1.0f, 1.0f, 1.0f, 0.1f);

        public Vector2i MouseTilePos { get; private set; }

		private ulong tileHighlightID;


		public void Click(Vector2 mPos)
		{
			Vector2 worldMPos = Content2D.Instance.Cam.ScreenToWorldPoint(mPos);
			GameLogic.Map map = UnityLogic.EtMGame.Instance.Map;

			//Exit if we clicked outside the map.
			Vector2i tilePos = new Vector2i(worldMPos);
			if (!map.Tiles.IsValid(tilePos))
				return;

            //If nothing special happens because of this click,
            //    do the usual behavior -- show a window for the unit(s) that were clicked on.
            if (!OnWorldTileClicked.Raise(tilePos))
            {
                //Get the units in the part of the map we clicked on and show the window for them.
                List<GameLogic.Unit> clickedUnits = map.GetUnits(tilePos).ToList();
                if (clickedUnits.Count == 1)
                {
                    MyUI.ContentUI.Instance.CreateUnitWindow(clickedUnits[0]);
                }
                else if (clickedUnits.Count > 1)
                {
                    MyUI.ContentUI.Instance.CreateWindow(MyUI.ContentUI.Instance.Window_SelectUnit,
                                                         clickedUnits);
                }
				//If there were no units, show a window about that tile.
				else
				{
					MyUI.ContentUI.Instance.CreateWindow(MyUI.ContentUI.Instance.Window_Tile,
														 tilePos);
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

		public void Scroll(float scrollWheelAmount, Vector2 mousePos)
		{
			float scale = Mathf.Pow(ScrollZoomScale, scrollWheelAmount);

			//Remember the world position of the mouse before zooming.
			Vector2 mouseWorldPos = Content2D.Instance.Cam.ScreenToWorldPoint(mousePos);

			//Zoom.
			UnityEngine.Assertions.Assert.IsTrue(Content2D.Instance.Cam.orthographic);
			Content2D.Instance.Cam.orthographicSize /= scale;

			//Preserve the world position under the mouse.
			Vector2 newMouseWorldPos = Content2D.Instance.Cam.ScreenToWorldPoint(mousePos);
			Content2D.Instance.CamTr.position += (Vector3)(mouseWorldPos - newMouseWorldPos);
		}

		private void Start()
		{
			tileHighlightID = MyUI.TileHighlighter.Instance.CreateHighlight(new Vector2i(0, 0),
																			TileHighlightColor);
		}
		private void Update()
        {
            Vector2 worldMPos = Content2D.Instance.Cam.ScreenToWorldPoint(Input.mousePosition);
            MouseTilePos = new Vector2i(worldMPos);

			MyUI.TileHighlighter.Instance.SetPos(tileHighlightID, MouseTilePos);
        }
	}
}
