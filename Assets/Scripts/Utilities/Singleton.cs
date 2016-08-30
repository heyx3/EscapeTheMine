using System;
using UnityEngine;


public class Singleton<T> : MonoBehaviour
	where T : Singleton<T>
{
	public static T Instance { get; private set; }

	protected virtual void Awake()
	{
		UnityEngine.Assertions.Assert.IsNull(Instance, gameObject.name);
		Instance = (T)this;
	}
	protected virtual void OnDestroy()
	{
		UnityEngine.Assertions.Assert.AreEqual(this, Instance, gameObject.name);
		Instance = null;
	}
}