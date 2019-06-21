using UnityEditor;
using System;

namespace FuzzyTools
{
	[CustomEditor(typeof(MeshMergerPolicyList))]
	public class MeshPolicyListGUI : Editor
	{
		private SerializedProperty staticFlagMask;
		private SerializedProperty identifiers;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("policyType"));
			staticFlagMask.intValue = (int) ((MeshMergerPolicyList.CheckTypes) EnumField("Check Types",
				(MeshMergerPolicyList.CheckTypes) staticFlagMask.intValue));
			ArrayGui(identifiers);

			serializedObject.ApplyModifiedProperties();
		}
		
		private void OnEnable()
		{
			staticFlagMask = serializedObject.FindProperty("checkType");
			identifiers = serializedObject.FindProperty("identifiers");
		}

		private static Enum EnumField(string label, Enum enumValue)
		{
#if UNITY_2017_3_OR_NEWER
			return EditorGUILayout.EnumFlagsField(label, enumValue);
#else
            return EditorGUILayout.EnumMaskField(label, enumValue);
#endif
		}

		private static void ArrayGui(SerializedProperty property)
		{
			var arraySizeProp = property.FindPropertyRelative("Array.size");
			EditorGUILayout.PropertyField(arraySizeProp);
			EditorGUI.indentLevel++;

			for (var i = 0; i < arraySizeProp.intValue; i++)
			{
				EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
			}

			EditorGUI.indentLevel--;
		}

	}
}