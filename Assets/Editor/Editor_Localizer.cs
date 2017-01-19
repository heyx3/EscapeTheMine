using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Localizer))]
public class Editor_Localizer : Editor
{
	[MenuItem("EtM/Generate")]
	public static void Generate()
	{
		System.Random rng = new System.Random();
		string[] names = new string[25];
		for (int i = 0; i < names.Length; ++i)
			names[i] = GameLogic.Units.Player_Char.Personality.GenerateName(GameLogic.Units.Player_Char.Personality.Genders.Male, rng.Next());

		string msg = names.Aggregate("", (_msg, element) => _msg + "\n" + element);
		EditorUtility.DisplayDialog("Names", msg, "OK");
	}

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