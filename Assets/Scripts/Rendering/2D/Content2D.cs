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
			Cam.gameObject.SetActive(false);
		}
	}
}