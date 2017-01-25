using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


public class Editor_UnityDictionary<KeyType, ValueType> : PropertyDrawer
{
	private bool keyAndValOnSameLine = false;
	private bool showElements = false;

	private GUIContent emptyGUIContent = new GUIContent();


	public Editor_UnityDictionary(bool _keyAndValOnSameLine)
	{
		keyAndValOnSameLine = _keyAndValOnSameLine;
	}


	public UnityDictionary<KeyType, ValueType> GetObjBeingEdited(SerializedProperty thisProperty)
	{
		var ownerObj = thisProperty.serializedObject.targetObject;
		var ownerObjType = ownerObj.GetType();
		var value = ownerObjType.GetField(thisProperty.name).GetValue(ownerObj);
		return (UnityDictionary<KeyType, ValueType>)value;
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (showElements)
		{
			float lineHeight = base.GetPropertyHeight(property, label);

			var keys = property.FindPropertyRelative("keys");
			int nKeys = keys.arraySize;
			
			if (nKeys < 1)
				return lineHeight;

			float keyPropertySize = EditorGUI.GetPropertyHeight(keys.GetArrayElementAtIndex(0)),
				  valPropertySize = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("values")
																		.GetArrayElementAtIndex(0));

			if (keyAndValOnSameLine)
				return lineHeight + (nKeys * (Math.Max(keyPropertySize, valPropertySize) + 5.0f));
			else
				return lineHeight + (nKeys * (keyPropertySize + valPropertySize + 10.0f));
		}
		else
		{
			return base.GetPropertyHeight(property, label);
		}
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		float lineHeight = 16.0f;
		const float buttonWidth = 20.0f;

		var objBeingEdited = GetObjBeingEdited(property);

		EditorGUI.BeginProperty(position, label, property);
		
		//Draw the foldout title.
		showElements = EditorGUI.Foldout(new Rect(position.x, position.y,
												  position.width - buttonWidth - 5.0f, lineHeight),
										 showElements, property.name);
		//Draw an "add" button next to the foldout.
		if (objBeingEdited.CanAddNewKey() &&
			GUI.Button(new Rect(position.x + position.width - buttonWidth, position.y,
								buttonWidth, lineHeight),
					   "+"))
		{
			objBeingEdited.Add(new KeyValuePair<KeyType, ValueType>(objBeingEdited.GetNewKey(),
																	default(ValueType)));
		}
		position.y += lineHeight;

		if (showElements && property.FindPropertyRelative("keys").arraySize > 0)
		{
			//Found how to do this from https://forum.unity3d.com/threads/working-with-arrays-as-serializedproperty-ies-in-editor-script.97356/#post-1325574
			var keysProp = property.FindPropertyRelative("keys");
			var valsProp = property.FindPropertyRelative("values");
			float halfWidth = position.width * 0.5f;
			float keyHeight = EditorGUI.GetPropertyHeight(keysProp.GetArrayElementAtIndex(0)),
				  valHeight = EditorGUI.GetPropertyHeight(valsProp.GetArrayElementAtIndex(0));
			EditorGUI.indentLevel += 1;
			for (int i = 0; i < keysProp.arraySize; ++i)
			{
				if (GUI.Button(new Rect(position.x, position.y, buttonWidth, lineHeight), "-"))
				{
					objBeingEdited.RemoveAt(i);
				}
				if (keyAndValOnSameLine)
				{
					EditorGUI.PropertyField(new Rect(position.x + buttonWidth, position.y,
													 halfWidth - buttonWidth, keyHeight),
											keysProp.GetArrayElementAtIndex(i), emptyGUIContent);
					EditorGUI.PropertyField(new Rect(position.x + halfWidth + buttonWidth, position.y,
													 halfWidth - buttonWidth, valHeight),
											valsProp.GetArrayElementAtIndex(i), emptyGUIContent);

					position.y += lineHeight;
				}
				else
				{
					EditorGUI.PropertyField(new Rect(position.x + buttonWidth, position.y,
													 position.width - buttonWidth, keyHeight),
											keysProp.GetArrayElementAtIndex(i),
											emptyGUIContent);
					position.y += lineHeight;
					EditorGUI.PropertyField(new Rect(position.x, position.y,
													 position.width, valHeight),
											valsProp.GetArrayElementAtIndex(i),
											emptyGUIContent);
					position.y += lineHeight;

					position.y += 5.0f;
				}

				position.y += 5.0f;
			}
			EditorGUI.indentLevel -= 1;
		}

		EditorGUI.EndProperty();
	}
}

//Below is all the definitions of child classes of Editor_UnityDictionary.
namespace Editor_Dict
{
	[CustomPropertyDrawer(typeof(Dict.GameObjectsByTileType))]
	public class Editor_GameObjectsByTileType : Editor_UnityDictionary<GameLogic.TileTypes, GameObject>
	{
		public Editor_GameObjectsByTileType() : base(true) { }
	}
	[CustomPropertyDrawer(typeof(Dict.GameObjectsByViewMode))]
	public class Editor_GameObjectsByViewMode : Editor_UnityDictionary<UnityLogic.ViewModes, GameObject>
	{
		public Editor_GameObjectsByViewMode() : base(true) { }
	}
	[CustomPropertyDrawer(typeof(Dict.UITabsByTabType))]
	public class Editor_UITabsByTabType : Editor_UnityDictionary<MyUI.Window_PlayerChar.TabTypes, UITab>
	{
		public Editor_UITabsByTabType() : base(true) { }
	}
}