using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace FuzzyTools
{

	public class ChangeGOVis : EditorWindow
	{
		private static GameObject[] selected;
		public static Dictionary<GameObject, HierarchyInfo> activeObjs = new Dictionary<GameObject, HierarchyInfo>();

		public static Dictionary<GameObject, HierarchyInfo> modifiedObjs = new Dictionary<GameObject, HierarchyInfo>();

		private const string WindowTitle = "Set Visibility";
		private const string Box = "Box";
		private const string None = "None";
		private const string HideInHierarchy = "Hide In Hierarchy";
		private const string HideInInspector = "Hide In Inspector";
		private const string NotEditable = "Not Editable";
		private const string DontSave = "Don't Save";
		private const string Button = "Button";

		[MenuItem("GameObject/FuzzyTools/Change Visibility", false, 50)]
		private static void ChangeVis()
		{
			//selected = Selection.gameObjects;
			activeObjs.Clear();
			var window = GetWindow(typeof(ChangeGOVis), true, WindowTitle);
			var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
			if (icon == null) return;
			window.titleContent.image = icon;
		}

		private void OnGUI()
		{
			if(Selection.gameObjects.Length == 0) Close();
			foreach (var obj in Selection.gameObjects)
			{
				
				if (!activeObjs.ContainsKey(obj)) activeObjs.Add(obj, GetInfo(obj));
				//if(!activeFlags.ContainsKey(obj)) activeFlags.Add(obj, new bool[8]);
				EditorGUILayout.BeginVertical(Box);
				var info = activeObjs[obj];
				info.opened = EditorGUILayout.Foldout(activeObjs[obj].opened, obj.name);
				activeObjs[obj] = info;
				if (activeObjs[obj].opened)
				{
					var flags = obj.hideFlags;
					var toggle = flags == HideFlags.None;
					if (GUILayout.Toggle(toggle, None, Button))
					{
						flags = HideFlags.None;
					}

					flags = SetHideFlags(HideInHierarchy, flags, HideFlags.HideInHierarchy);
					flags = SetHideFlags(HideInInspector, flags, HideFlags.HideInInspector);
					flags = SetHideFlags(NotEditable, flags, HideFlags.NotEditable);
					flags = SetHideFlags(DontSave, flags, HideFlags.DontSave);

					obj.hideFlags = flags;
					if (flags == HideFlags.None)
					{
						if (!modifiedObjs.ContainsKey(obj)) continue;
						modifiedObjs.Remove(obj);
					}
					else
					{
						if (modifiedObjs.ContainsKey(obj)) continue;
						modifiedObjs.Add(obj, activeObjs[obj]);
					}

				}

				EditorGUILayout.EndVertical();
			}

			foreach (var key in activeObjs.Keys.ToArray())
			{
				if (!Selection.gameObjects.Contains(key)) activeObjs.Remove(key);
			}
		}

		HierarchyInfo GetInfo(GameObject obj)
		{
			var indent = 0;
			var parent = obj.transform.parent;
			while (parent != null)
			{
				indent++;
				parent = parent.parent;
			}
			var info = new HierarchyInfo()
			{
				opened = Selection.gameObjects.Length < 3,
				indent = indent
			};
			return info;
		}

		HideFlags SetHideFlags(string toggleName, HideFlags flags, HideFlags newFlag)
		{
			var exists = (flags & newFlag) > 0;
			if (GUILayout.Toggle(exists, toggleName, Button))
				flags |= newFlag;
			else
				flags &= ~newFlag;
			return flags;
		}
	}
}