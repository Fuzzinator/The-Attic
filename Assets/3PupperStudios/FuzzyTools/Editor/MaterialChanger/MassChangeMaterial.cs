using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FuzzyTools
{
	public class MassChangeMaterial : EditorWindow
	{
		private const string Title = "Change Materials";
		private const string SourceFolder = "Material Source Folder";
		private const string AddSelection = "Add Selected";
		private const string ClearMats = "Clear List";
		private const string InvalidFolder = " is not a valid folder";
		private const string LoadFolderMats = "Load Materials From File";

		private const string MainTex = "_MainTex";
		private const string ColorProp = "_Color";
		private const string SearchMats = "t:Material";
		private const string Box = "box";

		private const string NewShader = "New Shader Properties";
		private const string ChangeShader = "Change Shader";
		private const string StandardShader = "Standard";
		private const string ChangeOption = "Change";
		private const string EnableInstance = "Enable Instancing";
		private const string SetChanges = "Set Changes";

		private static Object _sourceFolder;
		private static Vector2 _matScrollPos = Vector2.zero;
		private static Vector2 _popScrollPos = Vector2.zero;
		private static Rect _dropArea;
		
		private static readonly List<Material> TargetMats = new List<Material>();
		private readonly Dictionary<string, bool> _changeProperty = new Dictionary<string, bool>();
		private static string _folderPath = "";
		private static  GUIStyle _dropBox;
		private static Material _newMat;

		
		private static readonly Vector2 MinSize = new Vector2(750, 500);
        
		private Rect _gatherMaterialsRect = new Rect(10, 10, 200, 200);
		private Rect _changePropertiesRect = new Rect(100, 10, 200, 200);


		[MenuItem("Window/FuzzyTools/Materials/Change Material Properties")]
		private static void OpenWindow()
		{
			TargetMats.Clear();
			_matScrollPos = Vector2.zero;
			var window = GetWindow(typeof(MassChangeMaterial), true, Title);
			var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
			if (icon == null) return;
			window.titleContent.image = icon;
		}

		private void OnGUI()
		{
			minSize = MinSize;
			_gatherMaterialsRect.Set(10,10, (position.width *.5f) - 10, position.height - 25);
			_changePropertiesRect.Set((position.width *.5f) + 10, 10, (position.width *.5f) - 20, position.height - 25);

			//EditorGUILayout.BeginHorizontal("box");
			GUILayout.BeginArea(_gatherMaterialsRect);
			EditorGUILayout.BeginVertical(Box);
			_sourceFolder = EditorGUILayout.ObjectField(SourceFolder, _sourceFolder, typeof(DefaultAsset), false);
			EditorGUI.BeginDisabledGroup(!_sourceFolder);
			if(_sourceFolder)
			{
				_folderPath = AssetDatabase.GetAssetPath(_sourceFolder);
				if (!AssetDatabase.IsValidFolder(_folderPath))
				{
					Debug.Log(_sourceFolder.name + InvalidFolder);
					_sourceFolder = null;
				}
			}
			if (GUILayout.Button(LoadFolderMats))
			{
				AddMaterialsFromFile(_folderPath);
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			//var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
			//GUI.Box(dropArea, "Drop Game Objects / Prefabs Here", dropBox);
			//DropBoxGUI();
			ManageListButtons();
			
			EditorGUILayout.BeginVertical(Box);
			if (TargetMats.Count > 0)
            {
                
	            
                _matScrollPos = EditorGUILayout.BeginScrollView(_matScrollPos);
	            
                for (var i = 0; i < TargetMats.Count; i++)
                {
	                var mat = TargetMats[i];
                    if (TargetMats[i] == null)
                    {
	                    TargetMats.Remove(mat);
                        i--;
                        continue;
                    }

	                EditorGUILayout.BeginHorizontal(Box);
	                TargetMats[i] = EditorGUILayout.ObjectField(mat.name,  mat, typeof(Material), true) as Material;
	                if (mat == null)
	                {
		                EditorGUILayout.EndHorizontal();
		                continue;
	                }

	                if (mat.HasProperty(MainTex) && mat.GetTexture(MainTex) != null)
	                {
		                var img = mat.GetTexture(MainTex);
		                EditorGUILayout.BeginVertical(Box);
		                EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(50, 50), img);
		                EditorGUILayout.EndVertical();
	                }
	                else if(mat.HasProperty(ColorProp))
	                {
		                var col = mat.GetColor(ColorProp);
		                DrawColorBox(col);
		                //EditorGUI.ColorField(GUILayoutUtility.GetRect(50, 50), tex);
	                }
	                //matEditor.DrawHeader();
	                //EditorGUILayout.
					/*var matEditor = Editor.CreateEditor(mat);

	                if (matEditor.HasPreviewGUI())
		                matEditor.OnPreviewGUI(GUILayoutUtility.GetRect(50, 50), EditorStyles.whiteLabel);
	               */
 
	                EditorGUILayout.EndHorizontal();

                }
	            
	            EditorGUILayout.EndScrollView();
                

	            //PerformDragAndDrop();
            }
			else
			{
				EditorGUILayout.LabelField("");
				GUILayout.Space(position.height-149);
			}
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.EndVertical();
			GUILayout.EndArea();
			GUILayout.BeginArea(_changePropertiesRect);
			EditorGUILayout.BeginVertical(Box);
			//EditorGUILayout.BeginHorizontal();
			
			if(!_changeProperty.ContainsKey(ChangeShader)) _changeProperty.Add(ChangeShader, false);

			
			_changeProperty[ChangeShader] = EditorGUILayout.Toggle(ChangeShader, _changeProperty[ChangeShader]);
			//EditorGUI.BeginDisabledGroup(!changeProperty["Change Shader"]);
			if(_newMat == null) _newMat = new Material(Shader.Find(StandardShader)){name = NewShader};
			var matEditor = (MaterialEditor)Editor.CreateEditor(_newMat, typeof(MaterialEditor));
			matEditor.DrawHeader();
			
			
			//EditorGUI.EndDisabledGroup();
			//EditorGUILayout.EndHorizontal();
			var propCount = ShaderUtil.GetPropertyCount(_newMat.shader);
			var originalWidth = EditorGUIUtility.labelWidth;
			_popScrollPos = GUILayout.BeginScrollView(_popScrollPos);
			
			for (var i = 0; i < propCount; i++)
			{
				var propType = ShaderUtil.GetPropertyType(_newMat.shader, i);
				var propName = ShaderUtil.GetPropertyName(_newMat.shader, i);
				if(!_changeProperty.ContainsKey(propName)) _changeProperty.Add(propName, false);
				EditorGUIUtility.labelWidth = originalWidth;
				_changeProperty[propName] = EditorGUILayout.Toggle(ChangeOption, _changeProperty[propName]);
				EditorGUI.BeginDisabledGroup(!_changeProperty[propName]);
				var textDimensions = GUI.skin.label.CalcSize(new GUIContent(propName));
				EditorGUIUtility.labelWidth = /*originalWidth;// * 5;*/textDimensions.x + 20;
				
				switch (propType)
				{
					case ShaderUtil.ShaderPropertyType.Color:
						var propColor = _newMat.GetColor(propName);
						propColor = EditorGUILayout.ColorField(propName, propColor);
						_newMat.SetColor(propName, propColor);
						break;
					case ShaderUtil.ShaderPropertyType.Float:
					case ShaderUtil.ShaderPropertyType.Range:
						var propFloat = _newMat.GetFloat(propName);
						propFloat = EditorGUILayout.FloatField(propName, propFloat);
						_newMat.SetFloat(propName, propFloat);
						break;
					case ShaderUtil.ShaderPropertyType.Vector:
						var propVector = _newMat.GetVector(propName);
						propVector = EditorGUILayout.Vector4Field(propName, propVector);
						_newMat.SetVector(propName, propVector);
						break;
					case ShaderUtil.ShaderPropertyType.TexEnv:
						var propTex = _newMat.GetTexture(propName);
						propTex = (Texture)EditorGUILayout.ObjectField(propName, propTex, typeof(Texture), false);
						_newMat.SetTexture(propName, propTex);
						break;
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			if (!_changeProperty.ContainsKey(EnableInstance)) _changeProperty.Add(EnableInstance, false);
			_changeProperty[EnableInstance] =
				EditorGUILayout.Toggle(ChangeOption, _changeProperty[EnableInstance]);
			EditorGUI.BeginDisabledGroup(!_changeProperty[EnableInstance]);
			
			
			var thisSize = GUI.skin.label.CalcSize(new GUIContent(EnableInstance));
			EditorGUIUtility.labelWidth = /*originalWidth;// * 5;*/thisSize.x + 20;
			
			_newMat.enableInstancing = EditorGUILayout.Toggle(EnableInstance, _newMat.enableInstancing);
			EditorGUIUtility.labelWidth = originalWidth;
			EditorGUI.EndDisabledGroup();
			GUILayout.EndScrollView();
			EditorGUIUtility.labelWidth = originalWidth;

			if (GUILayout.Button(SetChanges))
			{
				if (_changeProperty[ChangeShader])
				{
					var matShader = _newMat.shader;
					foreach (var mat in TargetMats)
					{
						mat.shader = matShader;
					}
				}
				for (var i = 0; i < propCount; i++)
				{
					var propType = ShaderUtil.GetPropertyType(_newMat.shader, i);
					var propName = ShaderUtil.GetPropertyName(_newMat.shader, i);
					if (!_changeProperty[propName]) continue;
					//var textDimensions = GUI.skin.label.CalcSize(new GUIContent(propName));
					EditorGUIUtility.labelWidth = originalWidth * 5;//textDimensions.x + 20;
					
					switch (propType)
					{
						case ShaderUtil.ShaderPropertyType.Color:
							var propColor = _newMat.GetColor(propName);
							foreach (var mat in TargetMats)
							{
								if(mat.HasProperty(propName))
								{
									Undo.RegisterCompleteObjectUndo( mat,"ChangeMaterialProp");
									mat.SetColor(propName, propColor);
								}
							}
							break;
						case ShaderUtil.ShaderPropertyType.Float:
						case ShaderUtil.ShaderPropertyType.Range:
							var propFloat = _newMat.GetFloat(propName);
							foreach (var mat in TargetMats)
							{
								if(mat.HasProperty(propName))
								{
									Undo.RegisterCompleteObjectUndo( mat,"ChangeMaterialProp");
									mat.SetFloat(propName, propFloat);
								}
							}
							break;
						case ShaderUtil.ShaderPropertyType.Vector:
							var propVector = _newMat.GetVector(propName);
							foreach (var mat in TargetMats)
							{
								if(mat.HasProperty(propName))
								{
									Undo.RegisterCompleteObjectUndo( mat,"ChangeMaterialProp");
									mat.SetVector(propName, propVector);
								}
							}
							break;
						case ShaderUtil.ShaderPropertyType.TexEnv:
							var propTex = _newMat.GetTexture(propName);
							foreach (var mat in TargetMats)
							{
								if(mat.HasProperty(propName))
								{
									Undo.RegisterCompleteObjectUndo( mat,"ChangeMaterialProp");
									mat.SetTexture(propName, propTex);
								}
							}
							break;
					}
				}

				if (_changeProperty[EnableInstance])
				{
					var doInstance = _newMat.enableInstancing;
					foreach (var mat in TargetMats)
					{
						mat.enableInstancing = doInstance;
					}
				}
			}
			//matEditor.DrawDefaultInspector();
			/*var customShaders = CheckForCustomShaders();
			_hasCustomShaders = customShaders != null;

			allShaders.Clear();
			allShaders.Add("Keep Original Shader(s)");
			allShaders.AddRange(BuiltInShaders.NotHidden);
			if(_hasCustomShaders) allShaders.AddRange(customShaders);

			_shaderCount = EditorGUILayout.Popup("New Shader", _shaderCount, allShaders.ToArray());
			
			EditorGUILayout.EndHorizontal();
			if(_shaderCount != 0)
			{
				var tempMat = new Material(Shader.Find(allShaders[_shaderCount]));
				var matEditor = Editor.CreateEditor(tempMat);
				var editors = new[] {(Object)tempMat};
				matEditor.DrawHeader();
				//DestroyImmediate(tempMat);
			}*/
			EditorGUILayout.EndVertical();
			GUILayout.EndArea();
			//EditorGUILayout.EndHorizontal();
		}

		private static void DrawColorBox(Color col)
		{
			var tex = new Texture2D(1,1)
			{
				wrapMode = TextureWrapMode.Repeat
			};
			tex.SetPixel(1,1, col);
			tex.Apply();
			EditorGUILayout.BeginVertical(Box);
			EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(50, 50), tex);
			EditorGUILayout.EndVertical();
			DestroyImmediate(tex);
		}

		private static void AddMaterialsFromFile(string filePath)
		{
			var list = AssetDatabase.FindAssets(SearchMats, new[] {filePath});
			foreach (var matGuid in list)
			{
				var tempPath = AssetDatabase.GUIDToAssetPath(matGuid);
				var mat = AssetDatabase.LoadAssetAtPath(tempPath, typeof(Material)) as Material;
				if (mat == null) continue;
				TargetMats.AddIfDoesNotContain(mat);
			}
		}

		private static void ManageListButtons()
		{
			EditorGUILayout.BeginHorizontal(GUI.skin.button);
			if (GUILayout.Button(AddSelection))
			{
				var sels = Selection.objects;
				foreach (var obj in sels)
				{
					var mat = obj as Material;
					if (mat == null) continue;
					TargetMats.AddIfDoesNotContain(mat);
				}
			}

			if (GUILayout.Button(ClearMats))
			{
				TargetMats.Clear();
			}
			
			EditorGUILayout.EndHorizontal();
		}

		//TODO Get this sucker working
		/*private static void DropBoxGUI()
		{
			if (_dropBox == null)
			{
				_dropBox = new GUIStyle(GUI.skin.box)
				{
					fontStyle = FontStyle.Bold,
					alignment = TextAnchor.UpperLeft
				};
			}
			_dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
			GUI.Box(_dropArea, "Drop Game Objects / Prefabs Here", _dropBox);
			PerformDragAndDrop();
		}
		
		public static void PerformDragAndDrop()
        {
	       //Ok - set up for drag and drop
	        var objs = new List<Material>();
            switch (_currentEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!_dropArea.Contains(_currentEvent.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (_currentEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
						Debug.Log("dropped");
	                    var objects = DragAndDrop.objectReferences[0] as Material;
	                    objs.Add(objects);
	                    /*foreach (var draggedObject in DragAndDrop.objectReferences)
	                    {
		                    Debug.Log(draggedObject);
		                    var mat = draggedObject as Material;
		                    if (mat == null) continue;
		                    targetMats.AddIfDoesNotContain(mat);
		                    Debug.Log(mat.name);
	                    }#1#

                    }
				break;
            }
	        targetMats.AddRange(objs);
        }*/
	}
}