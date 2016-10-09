using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Rendering.ThreeD
{
	public class Content3D : Singleton<Content3D>
	{
		public Camera Cam;
		public Transform CamTr { get; private set; }


		protected override void Awake()
		{
			base.Awake();

			CamTr = Cam.transform;
			UnityEngine.Assertions.Assert.IsFalse(Cam.orthographic);
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