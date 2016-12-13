using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.TwoD
{
	public class Content2D : Singleton<Content2D>
	{
		public Camera Cam;
		public Transform CamTr { get; private set; }

        public GameObject Prefab_HungerWarning;


		protected override void Awake()
		{
			base.Awake();

			CamTr = Cam.transform;
			UnityEngine.Assertions.Assert.IsTrue(Cam.orthographic);
		}
		private void OnEnable()
		{
			Cam.gameObject.SetActive(true);
		}
		private void OnDisable()
		{
			if (Cam != null)
				Cam.gameObject.SetActive(false);
		}

		
		public void ZoomToSee(IEnumerable<Vector2i> poses)
		{
			//Get the range in world space spanned by the positions.
			Vector2i max = new Vector2i(int.MinValue, int.MinValue),
					 min = new Vector2i(int.MaxValue, int.MaxValue);
			foreach (var pos in poses)
			{
				max.x = Math.Max(max.x, pos.x);
				max.y = Math.Max(max.y, pos.y);
				min.x = Math.Min(min.x, pos.x);
				min.y = Math.Min(min.y, pos.y);
			}

			//If there weren't any elements in the collection, exit.
			if (max.x < min.x || max.y < min.y)
				return;

			//If there is only one tile being highlighted, don't mess with the zoom level.
			if (max.x == min.x || max.y == min.y)
			{
				CamTr.position = new Vector3(min.x + 0.5f, min.y + 0.5f, CamTr.position.z);
				return;
			}

			//Move the camera to see it.
			Vector2 size = new Vector2(max.x - min.x + 1,
									   max.y - min.y + 1);
			Cam.orthographicSize = size.y * 0.5f;
			CamTr.position = new Vector3(min.x + (size.x * 0.5f),
										 min.y + (size.y * 0.5f),
										 CamTr.position.z);
		}
	}
}