using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Localizes a uGUI Text string using its initial value as the key.
/// Children can inherit from this to add string format arguments.
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Text))]
public class Localizer : MonoBehaviour
{
    //TODO: Display localized text in editor using OnValidate() and private serializable string field.


    private UnityEngine.UI.Text txt;

    protected virtual void Awake()
    {
        txt = GetComponent<UnityEngine.UI.Text>();
        Localization.Language.OnChanged += Callback_OnLangChanged;
    }
    protected virtual void Start()
    {
        txt.text = Localize(txt.text);
    }
    protected virtual void OnDestroyed()
    {
        Localization.Language.OnChanged -= Callback_OnLangChanged;
    }

    /// <summary>
    /// Override this to do something more complex,
    ///     like provide formatting arguments to the localization function.
    /// </summary>
    protected virtual string Localize(string key)
    {
        return Localization.Get(key);
    }
    /// <summary>
    /// Called when the user's language changes.
    /// </summary>
    protected virtual void Callback_OnLangChanged(object ignoreThis, SystemLanguage oldLang, SystemLanguage newLang)
    {
        txt.text = Localize(txt.text);
    }
}