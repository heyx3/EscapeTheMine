using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Localizes a uGUI Text string using its initial value as the key.
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Text))]
public class Localizer : MonoBehaviour
{
	/// <summary>
	/// The arguments to string.Format() when getting the localized string.
	/// If set to null, no arguments will be supplied.
	/// </summary>
	public object[] Args = null;


    [SerializeField]
    private string key = "MAINMENU_TITLE";

    private UnityEngine.UI.Text txt = null;


    public string Key
	{
		get { return key; }
		set
		{
			key = value;
			OnValidate();
		}
	}
	public string Value
	{
		get
		{
#if UNITY_EDITOR
			if (_editor_nArgs <= 0)
				Args = null;
			else if (Args == null || Args.Length != _editor_nArgs)
				Args = new object[_editor_nArgs];
#endif

			return (Args == null ?
						Localization.Get(Key) :
						Localization.Get(Key, Args));
		}
	}


	/// <summary>
	/// Updates the text. Automatically gets called whenever the language changes.
	/// </summary>
    public void OnValidate()
    {
		if (txt == null)
			txt = GetComponent<UnityEngine.UI.Text>();
		txt.text = Value;
    }

    private void Awake()
    {
        Localization.Language.OnChanged += Callback_OnLangChanged;
    }
    private void OnDestroyed()
    {
        Localization.Language.OnChanged -= Callback_OnLangChanged;
    }
    private void Start()
    {
		OnValidate();
    }
	
    /// <summary>
    /// Called when the user's language changes.
    /// </summary>
    private void Callback_OnLangChanged(object ignoreThis, SystemLanguage oldLang, SystemLanguage newLang)
    {
		OnValidate();
    }


	//In the editor, provide a way to set the number of arguments that Localization should expect.
	//This is so that the preview of the localized value in-editor doesn't crash
	//    for strings that should have at least one argument.
#if UNITY_EDITOR
	[SerializeField]
	private int _editor_nArgs = 0;
#endif
}