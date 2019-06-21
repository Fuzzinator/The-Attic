using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace FuzzyTools
{
	public struct HierarchyInfo
	{
		public bool opened;
		public int indent;
	}

	public class GOVisManager : EditorWindow
	{
		private static int _radioSelection = 0;
		
		private const string HierarchyName = "InSceneTrackerForFuzzyToolsHierarchy";
		private const string WindowTitle = "Vis Manager";
		private const string Box = "box";
		private const string OnlyModified = "Only Modified";
		private const string AllGos = "All GameObjects";
		private const string None = "None";
		private const string HideInHierarchy = "Hide In Hierarchy";
		private const string HideInInspector = "Hide In Inspector";
		private const string NotEditable = "Not Editable";
		private const string DontSave = "Don't Save";
		private const string Button = "Button";
		private const string SortBy = "Sort by: ";
		private const string Alphabetic = "Alphabetical";
		private const string Transform = "Transform";

		private static GUIStyle _header;


		private static GameObject _hierarchyTool;
		private static Dictionary<GameObject, HierarchyInfo> listedObjs;
		
		private static Vector2 scroll = Vector2.zero;

		private static List<GameObject> _objs;

		private static GUIStyle _headder;

		
		[MenuItem("Assets/FuzzyTools/GameObject Vis Manager", false)]
		private static void OpenManager()
		{
			_hierarchyTool = GameObject.Find(HierarchyName);
			var info = GetInfo(_hierarchyTool);
			
			listedObjs = ChangeGOVis.modifiedObjs;
			_objs = listedObjs.Keys.ToList();
			
			var window = GetWindow(typeof(GOVisManager), false, WindowTitle);
			var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
			if (icon == null) return;
			window.titleContent.image = icon;
		}

		private void OnGUI()
		{
			if (_header == null)
			{
				_header =  new GUIStyle();
				_header.fontSize = 15;
				_header.fontStyle = FontStyle.Bold;
			}
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button(OnlyModified))
			{
				listedObjs = ChangeGOVis.modifiedObjs;
				_objs = listedObjs.Keys.ToList();
			}

			if (GUILayout.Button(AllGos))
			{
				var allObjs = FindObjectsOfType<GameObject>();
				listedObjs = new Dictionary<GameObject, HierarchyInfo>();
				foreach (var obj in allObjs)
				{
					listedObjs.Add(obj, GetInfo(obj));
				}
				_objs = listedObjs.Keys.ToList();
			}
			if(listedObjs == null) listedObjs = ChangeGOVis.modifiedObjs;
			EditorGUILayout.EndHorizontal();
			scroll = EditorGUILayout.BeginScrollView(scroll, Box);
			
			if(_objs != null)
			{
				for (var i = 0; i < _objs.Count; i++)
				{
					var obj = _objs[i];
					
					if (obj == null)
					{
						_objs.Remove(obj);
						if (listedObjs.ContainsKey(obj)) listedObjs.Remove(obj);
						if (ChangeGOVis.modifiedObjs.ContainsKey(obj)) ChangeGOVis.modifiedObjs.Remove(obj);
						i--;
						continue;
					}
					EditorGUILayout.BeginHorizontal();
					if (obj == null)
					{
						EditorGUILayout.EndHorizontal();
						continue;
					}
					EditorGUILayout.ObjectField(obj.name, obj, typeof(GameObject), false);
					DisplayButtons(obj);


					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.BeginHorizontal();
			if (_headder == null)
			{
				_headder = new GUIStyle();
				_headder.fontSize = 15;
				_headder.fontStyle = FontStyle.Bold;
				_headder.stretchWidth = false;
				_headder.margin = new RectOffset(5,5,5,5);
			}
			var labelSize = EditorGUIUtility.labelWidth;
			
			GUILayout.Label(SortBy, _headder);
			if (GUILayout.Button(Alphabetic))
			{
				if(_objs != null)
				{
					_objs = _objs.OrderBy(obj => obj.name).ToList();
				}
			}
			EditorGUI.BeginDisabledGroup(true);
			if (GUILayout.Button(Transform))
			{
				//_objs = SortByTransform(_objs);
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
		}
		
		private static HierarchyInfo GetInfo(GameObject obj)
		{
			var indent = 0;
			if (obj == null) return new HierarchyInfo();
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

		private void DisplayButtons(GameObject obj)
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
				if (!ChangeGOVis.modifiedObjs.ContainsKey(obj)) return;
				ChangeGOVis.modifiedObjs.Remove(obj);
			}
			else
			{
				if (ChangeGOVis.modifiedObjs.ContainsKey(obj)) return;
				if(listedObjs.ContainsKey(obj))
				{
					ChangeGOVis.modifiedObjs.Add(obj, listedObjs[obj]);
				}
				else
				{
					var info = GetInfo(obj);
					listedObjs.Add(obj, info);
					ChangeGOVis.modifiedObjs.Add(obj, info);
				}
			}
		}

		/*private List<GameObject> SortByTransform(List<GameObject> objs)
		{
			//var hierarchy = new List<List<GameObject>>();
			var top = new List<GameObject>();
			for (var i = 0; i< objs.Count; i++)
			{
				var obj = objs[i];
				if(obj.transform.parent != null) continue;
				top.AddIfDoesNotContain(top);
				objs.Remove(obj);
				i--;
			}
	
			top = top.OrderBy(obj => obj.transform.GetSiblingIndex()).ToList();
			var hierarchy = new GameObject[top.Count+1];
			foreach (var obj in top)
			{
				if (obj == null) continue;
				var checking = obj.transform;
				var indexOffset = 0;
				var checkIndex = 0;
				while (checkIndex <= checking.childCount)
				{
					
					foreach (var child in checking)
					{
						var go = (child as Transform);
						if (go == null) continue;
						
						if (objs.Contains(child))
						{
							top.Insert(top.IndexOf(obj) + indexOffset, go.gameObject);
						}
					}

					checkIndex++;
				}
			}
			/*for (var i = 0; i < objs.Count; i++)
			{
				var obj = objs[i];
				if(obj == null) continue;
				var parent = obj.transform.parent;
				var index = 0;
				while (parent != null)
				{
					if (top.Contains(parent.gameObject))
					{
						
					}
				}
			}#1#

			return objs;
		}*/
	}
}