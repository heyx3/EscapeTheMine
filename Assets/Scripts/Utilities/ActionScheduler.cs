using System;
using System.Collections;
using UnityEngine;


public class ActionScheduler : Singleton<ActionScheduler>
{
	public void Schedule(Action toDo, float time) { StartCoroutine(DoActionCoroutine(toDo, time)); }
	public void Schedule(Action toDo, int frames) { StartCoroutine(DoActionCoroutine(toDo, frames)); }

	private IEnumerator DoActionCoroutine(Action toDo, float time)
	{
		yield return new WaitForSeconds(time);
		toDo();
	}
	private IEnumerator DoActionCoroutine(Action toDo, int nFrames)
	{
		for (int i = 0; i < nFrames; ++i)
			yield return null;
		toDo();
	}
}