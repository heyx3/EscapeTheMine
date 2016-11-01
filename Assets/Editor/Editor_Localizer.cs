using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Localizer))]
public class Editor_Localizer : Editor
{
    public override void OnInspectorGUI()
    {
        Localizer myLoc = (Localizer)target;

        base.OnInspectorGUI();

        GUILayout.Space(15.0f);

		string localizedString = myLoc.Value;
        if (localizedString == null)
            localizedString = "KEY NOT FOUND";

        GUILayout.Label("My language: " + Localization.Language.Value.ToString());

        if (GUILayout.Button("Refresh localization files"))
        {
            Localization.ReloadLocalization();
        }

        myLoc.OnValidate();
    }
}