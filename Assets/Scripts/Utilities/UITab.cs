using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents a tab that can be clicked on, selected, and deselected.
/// All children under this object are assumed to be elements inside this tab's view.
/// If this object has a UI.Image component, its color will be modified
///     based on whether this tab is selected or deselected.
/// </summary>
public class UITab : MonoBehaviour
{
	public event Action<UITab> OnClicked;

	public Color SelectedColor = Color.white,
				 DeselectedColor = Color.grey;

	/// <summary>
	/// Any child GameObjects in this list will be left active when this tab gets deselected.
	/// </summary>
	public List<Transform> ChildrenToIgnore = new List<Transform>();

	public Transform MyTr { get; private set; }
	public UnityEngine.UI.Image MyImg { get; private set; }
	public bool IsSelected { get; private set; }


	public virtual void SelectMe()
	{
		for (int i = 0; i < MyTr.childCount; ++i)
            if (!ChildrenToIgnore.Contains(MyTr.GetChild(i)))
			    MyTr.GetChild(i).gameObject.SetActive(true);

		if (MyImg != null)
			MyImg.color = SelectedColor;
		IsSelected = true;
	}
	public virtual void DeselectMe()
	{
		for (int i = 0; i < MyTr.childCount; ++i)
			if (!ChildrenToIgnore.Contains(MyTr.GetChild(i)))
				MyTr.GetChild(i).gameObject.SetActive(false);

		if (MyImg != null)
			MyImg.color = DeselectedColor;
		IsSelected = false;
	}

	public void Callback_Clicked()
	{
		if (OnClicked != null)
			OnClicked(this);
	}

	protected virtual void Awake()
	{
		MyTr = transform;
		MyImg = GetComponent<UnityEngine.UI.Image>();
		IsSelected = false;
	}
}