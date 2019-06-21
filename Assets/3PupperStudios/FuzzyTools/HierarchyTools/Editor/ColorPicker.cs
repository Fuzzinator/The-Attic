using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace FuzzyTools
{
	
	public class ColorPicker : EditorWindow
	{
		#region const Strings

		private const string MenuPlacement = "GameObject/FuzzyTools/Hierarchy/Change Color Or Style";
		private const string TrackerName = "InSceneTrackerForFuzzyToolsHierarchy";
		private const string EditorOnly = "EditorOnly";
		private const string ChangeStyle = "Change Style";
		private const string UniformChange = "Change Colors in Uniform";
		private const string EditorPrefUniform = "UniformChangeColors";
		private const string BackGroundColor = "Background Color";
		private const string FontColor = "Font Color";
		private const string FontStyleName = "Font Style";

		#endregion
		private static GameObject[] _selectedGameObjs;
		private static Color[] _backgroundColors;
		private static Color[] _fontColors;
		private static FontStyle[] _fontStyles;

		private static Color _skinDefault;
		private static Color _fontDefault;
		private static FontStyle _styleDefault;

		private static Vector2 _scrollPos = Vector2.zero;
		private static InSceneTracker _inSceneTracker;

		[MenuItem(MenuPlacement, false, 8)]
		private static void ChangeColor()
		{
			var selectedGameObjs = Selection.gameObjects;
			if (selectedGameObjs.Length == 0) return;
		
			Init(selectedGameObjs);
		}
		private static void Init(GameObject[] selected)
		{
			_scrollPos = Vector2.zero;
			_selectedGameObjs = selected;
			_backgroundColors = new Color[_selectedGameObjs.Length];
			_fontColors = new Color[_selectedGameObjs.Length];
			_fontStyles = new FontStyle[_selectedGameObjs.Length];
			_styleDefault = FontStyle.Normal;

			_skinDefault = EditorGUIUtility.isProSkin ? HierarchyTools.DefaultProSkin : HierarchyTools.DefaultSkin;
			_fontDefault = EditorGUIUtility.isProSkin ? Color.white : Color.black;

			for (var i = 0; i < _backgroundColors.Length; i++)
			{
				_backgroundColors[i] = _skinDefault;
				_fontColors[i] = _fontDefault;
				_fontStyles[i] = _styleDefault;
			}

			_inSceneTracker = FindObjectOfType<InSceneTracker>();
			if (_inSceneTracker == null)
			{
				var tracker = new GameObject(TrackerName, typeof(InSceneTracker))
				{
					hideFlags = HideFlags.HideInHierarchy,
					tag = EditorOnly
				};

				_inSceneTracker = tracker.GetComponent<InSceneTracker>();
			}

			EditorSceneManager.MarkAllScenesDirty();
			var window = GetWindow(typeof(ColorPicker), true, ChangeStyle);
			var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
			if (icon == null) return;
			window.titleContent.image = icon;
		}

		private void OnGUI()
		{
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
			FuzzyTools.uniformChangeColors =
				EditorGUILayout.Toggle(UniformChange, FuzzyTools.uniformChangeColors);
			FuzzyHelper.SetEditorPrefBool(EditorPrefUniform, FuzzyTools.uniformChangeColors);

			if (FuzzyTools.uniformChangeColors)
			{
				_skinDefault = EditorGUILayout.ColorField(BackGroundColor, _skinDefault);
				_fontDefault = EditorGUILayout.ColorField(FontColor, _fontDefault);
				_styleDefault = (FontStyle) EditorGUILayout.EnumPopup(FontStyleName, _styleDefault);
			}


			for (var i = 0; i < _selectedGameObjs.Length; i++)
			{
				if (!FuzzyTools.uniformChangeColors)
				{
					EditorGUILayout.LabelField(_selectedGameObjs[i].name);
					_backgroundColors[i] = EditorGUILayout.ColorField(BackGroundColor, _backgroundColors[i]);
					_fontColors[i] = EditorGUILayout.ColorField(FontColor, _fontColors[i]);
					_fontStyles[i] = (FontStyle) EditorGUILayout.EnumPopup(FontStyleName, _fontStyles[i]);
				}
				else
				{
					_backgroundColors[i] = _skinDefault;
					_fontColors[i] = _fontDefault;
					_fontStyles[i] = _styleDefault;
				}

				var customObjs = _inSceneTracker.customizedObjs;
				var customOptions = _inSceneTracker.options;
				var options = new HierarchyOptions()
				{
					backgroundColor = _backgroundColors[i],
					fontColor = _fontColors[i],
					style = _fontStyles[i]
				};

				if (customObjs.Contains(_selectedGameObjs[i]))
				{
					var index = customObjs.IndexOf(_selectedGameObjs[i]);
					customOptions[index] = options;
				}
				else
				{

					customObjs.Add(_selectedGameObjs[i]);
					customOptions.Add(options);
				}

				EditorGUILayout.Separator();
			}

			EditorGUILayout.EndScrollView();
		}
	}
}