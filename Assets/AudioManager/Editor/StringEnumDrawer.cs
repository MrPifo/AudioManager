using Sperlich.AStar;
using Sperlich.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sperlich.Editor {
	[CustomPropertyDrawer(typeof(StringEnum<>), true)]
	public class StringEnumDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var stringEnum = (IStringEnum)fieldInfo.GetValue(property.serializedObject.targetObject);
			if(stringEnum != null) {
				System.Enum @enum = stringEnum.GetValue();

				EditorGUI.BeginProperty(position, label, property);

				var newValue = EditorGUILayout.EnumPopup(property.displayName, @enum);
				if(newValue != @enum) {
					Undo.RecordObject(property.serializedObject.targetObject, $"{@enum}");
					stringEnum.SetValue(newValue);
				}

				EditorGUI.EndProperty();
			}
		}

	}
}