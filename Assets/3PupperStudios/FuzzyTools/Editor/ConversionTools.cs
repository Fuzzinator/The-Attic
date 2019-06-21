using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace FuzzyTools
{
	public class ConvertImages : EditorWindow
	{
		private const string DesiredType = "Desired Type: ";
		private const string KeepOriginal = "Keep Original: ";
		private const string SameName = "Same Name: ";
		private const string NewName = "New Name";
		private const string AddToList = "Add Selection";
		private const string ClearList = "Clear List";
		private const string Convert = "Convert";
		private const string Destructive = "Warning this is destructive and cannot be undone!";
		private const string MessagePt1 = "This will create a duplicate of the image in ";
		private const string MessagePt2 = " format and destroy the original \nThis cannot be undone.";
		private const string Continue = "Continue";
		private const string Cancel = "Cancel";
		private const string Keep = "Keep Originals";

		private static readonly Vector2 MinSize = new Vector2(400, 240);

		private static readonly GUIStyle _warningFont = new GUIStyle();




		private static Vector2 _scrollPos = Vector2.zero;
		private static List<Texture2D> _textures = new List<Texture2D>();
		private static ImageType _chosenType = ImageType.PNG;
		private static bool _keepOriginal = true;
		private static bool _sameName = true;
		private static List<string> _newNames = new List<string>();
		private static EditorWindow _window;

		private static int _radioSelection = 0;
		private const string ConvertedLocation = "Save Converted File to:";
		private static readonly string[] RadioOptions = {"Original File Location", "Custom Location"};
		private static string _saveLocation = FuzzyTools.DefaultImagePath;
		private static GUIStyle _bold = new GUIStyle();

		[MenuItem("Assets/FuzzyTools/Convert/Image(s)")]
		private static void OpenConverter()
		{
			Init();
		}

		public static void Init()
		{
			_bold.fontStyle = FontStyle.Bold;
			_warningFont.fontStyle = FontStyle.Bold;
			_warningFont.normal.textColor = Color.red;
			_textures.Clear();
			AddSelectionToTextures();
			_chosenType = ImageType.PNG;
			_keepOriginal = true;
			_sameName = true;
			_newNames = new List<string>();
			_scrollPos = Vector2.zero;
			_radioSelection = 0;
			_window = GetWindow(typeof(ConvertImages), true, "Convert Images");
			var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
			if (icon == null) return;
			_window.titleContent.image = icon;
			_saveLocation = FuzzyTools.DefaultImagePath;
			_window.minSize = MinSize;
		}

		private void OnGUI()
		{

			EditorGUILayout.Space();
			_chosenType = (ImageType) EditorGUILayout.EnumPopup(DesiredType, _chosenType);
			EditorGUILayout.BeginHorizontal();
			var textDimensions = GUI.skin.label.CalcSize(new GUIContent(KeepOriginal));
			EditorGUIUtility.labelWidth = textDimensions.x;
			_keepOriginal = EditorGUILayout.Toggle(KeepOriginal, _keepOriginal);
			textDimensions = GUI.skin.label.CalcSize(new GUIContent(SameName));
			EditorGUIUtility.labelWidth = textDimensions.x;
			_sameName = EditorGUILayout.Toggle(SameName, _sameName);
			//EditorGUILayout.LabelField(SameName, quarterWindowLayout);
			EditorGUILayout.EndHorizontal();
			if (!_keepOriginal) EditorGUILayout.LabelField(Destructive, _warningFont);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(ConvertedLocation, _bold);
			_radioSelection = GUILayout.SelectionGrid(_radioSelection, RadioOptions, RadioOptions.Length,
				EditorStyles.radioButton);
			_saveLocation = _radioSelection == 1 ? EditorGUILayout.TextField(_saveLocation) : _saveLocation;
			if (_saveLocation.Length > 0 && !_saveLocation.EndsWith("/")) _saveLocation += "/";
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(AddToList))
			{
				AddSelectionToTextures();
			}

			if (GUILayout.Button(ClearList))
			{
				_textures.Clear();
			}

			EditorGUILayout.EndHorizontal();

			for (var i = 0; i < _textures.Count; i++)
			{
				if (_textures[i] != null) continue;

				_textures.Remove(_textures[i]);
				i--;

			}

			_newNames.SetLength(_textures.Count, "");
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
			for (var i = 0; i < _textures.Count; i++)
			{
				var texture = _textures[i];
				var obj = (Object) texture;
				EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);

				if (_sameName)
				{
					texture = (Texture2D) EditorGUILayout.ObjectField(texture != null ? texture.name : "", obj,
						typeof(Texture2D), false);
				}
				else
				{

					_newNames[i] = EditorGUILayout.TextField(_newNames[i]);

					texture = (Texture2D) EditorGUILayout.ObjectField(_newNames[i], obj,
						typeof(Texture2D), false);

					if (_newNames[i] == "") _newNames[i] = texture != null ? texture.name : "";
					if (_newNames[i] == "///") _newNames[i] = "Pupper 1 is Penny Potato";


				}
			}

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button(Convert))
			{
				ConvertImage();
			}
		}

		private static void AddSelectionToTextures()
		{
			var objects = Selection.objects;
			foreach (var obj in objects)
			{
				var image = obj as Texture2D;
				if (image != null)
				{
					_textures.AddIfDoesNotContain(image);
				}
			}
		}

		private static void ConvertImage()
		{
			if (!_keepOriginal)
			{
				var i = EditorUtility.DisplayDialogComplex(Destructive, MessagePt1 + _chosenType + MessagePt2,
					Continue, Cancel, Keep);
				switch (i)
				{
					case (0):
						break;
					case (1):
						return;
					case (2):
						_keepOriginal = false;
						break;
				}
			}

			var j = 0;
			foreach (var tex in _textures)
			{
				if (tex == null) continue;

				var texName = tex.name;
				var texPath = AssetDatabase.GetAssetPath(tex);

				var pos = texPath.LastIndexOf(".") + 1;
				if (texPath.Substring(pos, texPath.Length - pos).Equals(_chosenType.ToString(),
					StringComparison.InvariantCultureIgnoreCase))
				{
					if (EditorUtility.DisplayDialog("Already " + _chosenType + " file",
						texName + " is already a " + _chosenType + ". What would you like to do?",
						"Skip", "Convert Anyway"))
					{
						continue;
					}
				}

				var texSettings = AssetImporter.GetAtPath(texPath) as TextureImporter;
				var originalSettings = texSettings;
				if (texSettings != null)
				{

					texSettings.isReadable = true;
					texSettings.textureCompression = TextureImporterCompression.Uncompressed;
					AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
				}

				texPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1);
				//texName = _sameName?texName:
				var validName = _radioSelection == 1 ? "Assets/" + _saveLocation : texPath;
				FuzzyTools.CheckIfPathExists(validName);
				validName += _sameName ? texName : _newNames[j];
				var l = 0;


				while (File.Exists(validName + "." + _chosenType.ToString().ToLowerInvariant()))
				{
					validName = (_radioSelection == 1 ? "Assets/" + _saveLocation : texPath) + texName + "_" + l;
					l++;
				}

				validName += "." + _chosenType.ToString().ToLowerInvariant();

				var bytes = new byte[0];
				// Encode texture into PNG
				switch (_chosenType)
				{
					case ImageType.EXR:
						var hdrFormat = new Texture2D(tex.width, tex.height, TextureFormat.RGBAFloat, false);
						hdrFormat.SetPixels(tex.GetPixels());
						hdrFormat.Apply();

						bytes = hdrFormat.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
						DestroyImmediate(hdrFormat);
						break;
					case ImageType.JPG:
						bytes = tex.EncodeToJPG();
						break;
					case ImageType.PNG:
						bytes = tex.EncodeToPNG();
						break;
				}

				File.WriteAllBytes(validName, bytes);
				texSettings = originalSettings;
				//Debug.Log(validName);
				//Debug.Log(texPath);
				AssetDatabase.ImportAsset(validName, ImportAssetOptions.ForceUpdate);
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex), ImportAssetOptions.ForceUpdate);
				if (!_keepOriginal)
				{
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tex));
				}

				j++;
			}

		}
	}

	public class ConvertModelToTerrain : EditorWindow
	{
		private static GameObject _sourceGeo;
		private const string SourceGeometry = "Source Geometry: ";
		private const string Resolution = "Heightmap Resolution: ";
		private const string TerrainPosition = "Terrain Position: ";
		private const string TerrainOffset = "Terrain Y Offset: ";
		private const string ConvertToTerrain = "Convert To Terrain";
		private const string AssetsFolder = "Assets/";
		private const string Asset = ".asset";
		private const string TerrainName = "New Terrain Name: ";
		private const string DefaultTerrainName = "New Terrain";

		private const string ScanMode = "Scan Mode:";
		//private const string RegenerateCollider = "Temporarily Regenerate Collider";

		private static readonly Vector3 PlusTenth = new Vector3(.1f, 0, .1f);

		private static readonly Vector2 MinSize = new Vector2(300, 225);
		private static readonly Vector2 OtherMinSize = new Vector2(300, 240);

		private static int _resolution = 512;
		private static int _rezSelection = 4;
		private static Vector3 _addTerrain;
		private static int _bottomTopRadioSelected = 0;
		private static readonly string[] BottomTopRadio = new string[] {"Bottom Up", "Top Down"};
		private static float _shiftHeight = 0f;

		private static string _terrainName = "";
		private static GameObject _storedAsset = null;
		private static bool _addedToScene = false;

		private static List<MeshCollider> newMeshColliders = new List<MeshCollider>();

		private static int _radioSelection = 0;
		private const string ConvertedLocation = "Save Converted File to:";
		private static readonly string[] RadioOptions = {"Origin File Location", "Custom Location"};
		private static string _saveLocation = FuzzyTools.DefaultTerrainPath;
		private static GUIStyle _bold = new GUIStyle();

		[MenuItem("Assets/FuzzyTools/Convert/Model to Terrain")]
		private static void OpenObjToObjAssets()
		{
			Init();
		}
		
		[MenuItem("GameObject/FuzzyTools/Convert/Model to Terrain", false, 5)]
		private static void OpenObjToTerrainGameObject()
		{
			Init();
		}

		public static void Init()
		{
			_terrainName = "";
			_rezSelection = 4;
			_sourceGeo = Selection.activeObject as GameObject;
			_bold.fontStyle = FontStyle.Bold;
			_radioSelection = 0;
			var window = GetWindow<ConvertModelToTerrain>(true, ConvertToTerrain);
			var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
			if (icon == null) return;
			window.titleContent.image = icon;
			window.minSize = MinSize;
		}

		void OnGUI()
		{

			_sourceGeo = (GameObject) EditorGUILayout.ObjectField(SourceGeometry, _sourceGeo, typeof(GameObject), true);
			_terrainName = EditorGUILayout.TextField(TerrainName, _terrainName);
			if (_terrainName == "")
			{
				_terrainName = _sourceGeo != null ? _sourceGeo.name : DefaultTerrainName;
			}

			if (_terrainName == "///") _terrainName = "Pupper 2 is Hiccup";
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(ConvertedLocation, _bold);
			_radioSelection = GUILayout.SelectionGrid(_radioSelection, RadioOptions, RadioOptions.Length,
				EditorStyles.radioButton);
			if (_radioSelection == 1)
			{
				minSize = OtherMinSize;
				_saveLocation = EditorGUILayout.TextField(_saveLocation);
			}
			else
			{
				minSize = MinSize;
			}

			if (_saveLocation.Length > 0 && !_saveLocation.EndsWith("/")) _saveLocation += "/";

			EditorGUILayout.Space();

			_rezSelection = EditorGUILayout.Popup(Resolution, _rezSelection, FuzzyTools.terrainRezOptions);
			_resolution =
				int.Parse(FuzzyTools
					.terrainRezOptions[_rezSelection]); //EditorGUILayout.IntField(Resolution, _resolution);
			_addTerrain = EditorGUILayout.Vector3Field(TerrainPosition, _addTerrain);
			_shiftHeight = EditorGUILayout.Slider(TerrainOffset, _shiftHeight, -1f, 1f);
			EditorGUILayout.LabelField(ScanMode);
			//EditorGUILayout.BeginHorizontal();
			_bottomTopRadioSelected = GUILayout.SelectionGrid(_bottomTopRadioSelected, BottomTopRadio,
				BottomTopRadio.Length, EditorStyles.radioButton);
			//_regenerateCollider = EditorGUILayout.ToggleLeft(RegenerateCollider, _regenerateCollider);
			//EditorGUILayout.BeginToggleGroup("",_sourceGeo != null);
			//EditorGUILayout.EndHorizontal();

			EditorGUI.BeginDisabledGroup(_sourceGeo == null);

			if (GUILayout.Button("Create Terrain"))
			{

				if (_sourceGeo == null)
				{

					EditorUtility.DisplayDialog("No source GameObject", "Please select an object.", "Ok");
					return;
				}

				else
				{
					CreateTerrain();
				}
			}

			EditorGUI.EndDisabledGroup();
			//EditorGUILayout.EndToggleGroup();
		}
/*

		private delegate void CleanUp();*/

		private static void CreateTerrain()
		{
			CheckIfExistsInScene(_sourceGeo);

			var meshFilters = _sourceGeo.GetComponentsInChildren<MeshFilter>();
			var meshColliders = _sourceGeo.GetComponentsInChildren<MeshCollider>();
			newMeshColliders.Clear();
			foreach (var mesh in meshFilters)
			{
				if (mesh.GetComponent<MeshCollider>() || mesh.sharedMesh == null) continue;
				var collider = mesh.gameObject.AddComponent<MeshCollider>();
				if (collider == null) Debug.Log("What the what?");
				collider.sharedMesh = mesh.sharedMesh;
				collider.inflateMesh = mesh.sharedMesh.triangles.Length < 256;
				newMeshColliders.Add(collider);
			}

			var myCollider = _sourceGeo.GetComponent<MeshCollider>();
			if (myCollider == null)
				myCollider = _sourceGeo.GetComponentInChildren<MeshCollider>();

			if (myCollider == null)
			{
				EditorUtility.DisplayDialog("Cannot Generate Collider",
					"We were unable to generate a mesh collider, most likely due to a lack of MeshFilters." + "\n" +
					"A Terrain will not be created.", "Okay");
				Init();
				if (_addedToScene)
				{
					DestroyImmediate(_sourceGeo);
					_sourceGeo = _storedAsset;
					_addedToScene = false;
				}

				return;
			}



			//fire up the progress bar
			ShowProgressBar(1, 100);

			var terrain = new TerrainData()
			{
				heightmapResolution = _resolution,
				name = _terrainName
			};
			var terrainObject = Terrain.CreateTerrainGameObject(terrain);









			/*var myCollider = _sourceGeo.GetComponent<MeshCollider>();
			if (myCollider == null)
			{
				myCollider = _sourceGeo.AddComponent<MeshCollider>();
				cleanUp = () => DestroyImmediate(myCollider);
			}

			//Add a collider to our source object if it does not exist.
			//Otherwise raycasting doesn't work.
			foreach (Transform child in _sourceGeo.transform)
			{
				if (child.GetComponent<MeshCollider>()) continue;

				var collider = child.GetComponent<MeshFilter>() != null
					? _sourceGeo.AddComponent<MeshCollider>()
					: null;
				if (!collider)
				{
					cleanUp = () => DestroyImmediate(collider);
				}
			}*/

			//Bounds bounds = collider.bounds;
			var bounds = new Bounds(Vector3.zero, Vector3.zero);
			bounds = bounds.GetBounds(_sourceGeo);

			var sizeFactor = bounds.size.y / (bounds.size.y + _addTerrain.y);
			terrain.size = bounds.size + PlusTenth + _addTerrain;

			//var sizeFactor = collider.bounds.size.y / (collider.bounds.size.y + addTerrain.y);
			//terrain.size = collider.bounds.size + addTerrain;
			bounds.size = new Vector3(terrain.size.x, bounds.size.y, terrain.size.z);

			// Do raycasting samples over the object to see what terrain heights should be
			var heightMaps = new float[terrain.heightmapWidth, terrain.heightmapHeight];

			var ray = new Ray(new Vector3(bounds.min.x, bounds.max.y + bounds.size.y, bounds.min.z), -Vector3.up);
			var hit = new RaycastHit();
			var meshHeightInverse = 1 / bounds.size.y;
			var rayOrigin = ray.origin;

			var maxLength = terrain.heightmapWidth;
			var maxHeight = terrain.heightmapHeight;

			var stepXZ = new Vector2(bounds.size.x / maxLength, bounds.size.z / maxHeight);

			for (var zCount = 0; zCount < maxHeight; zCount++)
			{

				ShowProgressBar(zCount, maxHeight);

				for (var xCount = 0; xCount < maxLength; xCount++)
				{

					var height = 0.0f;
					foreach (var collider in newMeshColliders)
					{
						if (height != 0) break;
						if (collider.Raycast(ray, out hit, bounds.size.y * 3))
						{
							//if(meshColliders.Contains(hit.collider) || newMeshColliders.Contains(hit.collider))
							//{
							height = (hit.point.y - bounds.min.y) * meshHeightInverse;
							height += _shiftHeight;

							//bottom up
							if (_bottomTopRadioSelected == 0)
							{

								height *= sizeFactor;
							}

							//clamp
							if (height < 0)
							{

								height = 0;
							}

							//}
						}
					}

					foreach (var collider in meshColliders)
					{
						if (height != 0) break;
						if (collider.Raycast(ray, out hit, bounds.size.y * 3))
						{
							//if(meshColliders.Contains(hit.collider) || newMeshColliders.Contains(hit.collider))
							//{
							height = (hit.point.y - bounds.min.y) * meshHeightInverse;
							height += _shiftHeight;

							//bottom up
							if (_bottomTopRadioSelected == 0)
							{

								height *= sizeFactor;
							}

							//clamp
							if (height < 0)
							{

								height = 0;
							}

							//}
						}

					}
					/*if (Collider.Raycast(ray, out hit, bounds.size.y * 3))
					{
                        //if(meshColliders.Contains(hit.collider) || newMeshColliders.Contains(hit.collider))
                        //{
                            height = (hit.point.y - bounds.min.y) * meshHeightInverse;
                            height += _shiftHeight;

                            //bottom up
                            if (_bottomTopRadioSelected == 0)
                            {

                                height *= sizeFactor;
                            }

                            //clamp
                            if (height < 0)
                            {

                                height = 0;
                            }
                        //}
					}
*/

					heightMaps[zCount, xCount] = height;
					rayOrigin.x += stepXZ[0];
					ray.origin = rayOrigin;
				}

				rayOrigin.z += stepXZ[1];
				rayOrigin.x = bounds.min.x;
				ray.origin = rayOrigin;
			}

			terrain.SetHeights(0, 0, heightMaps);


			CreateTerrainAsset(terrain);
			Undo.RegisterCreatedObjectUndo(terrainObject, "Model to Terrain");

			EditorUtility.ClearProgressBar();

			if (_addedToScene)
			{
				DestroyImmediate(_sourceGeo);
				_sourceGeo = _storedAsset;
				_addedToScene = false;
			}

			for (var i = 0; newMeshColliders.Count != 0; i = 0)
			{
				var collider = newMeshColliders[i];

				newMeshColliders.Remove(collider);
				if (collider == null)
				{
					continue;
				}

				DestroyImmediate(collider.gameObject.GetComponent<MeshCollider>(), true);
			}

			newMeshColliders.Clear();
		}

/*	    public static void GetFullMeshCollider(GameObject obj)
	    {
            
	        var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
	        var combine = new CombineInstance[meshFilters.Length];

	        var i = 0;
	        while (i < meshFilters.Length)
	        {
	            combine[i].mesh = meshFilters[i].sharedMesh;
	            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
	            meshFilters[i].gameObject.SetActive(false);

	            i++;
	        }

	        var addedMesh = false;

	        _oldMeshFilter = obj.GetComponent<MeshFilter>();
	        if (_oldMeshFilter == null)
	        {
	            addedMesh = true;
	            _newMeshFilter = obj.AddComponent<MeshFilter>();
	        }
	        
	        obj.GetComponent<MeshFilter>().sharedMesh = new Mesh();
	        obj.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
	        _oldMeshCollider = obj.GetComponent<MeshCollider>();
	        if (_oldMeshCollider == null)
	        {
	            _newMeshCollider = obj.AddComponent<MeshCollider>();
	            _newMeshCollider.enabled = true;
	            _newMeshCollider.inflateMesh = true;
	            _newMeshCollider.sharedMesh = addedMesh ? _newMeshFilter.sharedMesh : _oldMeshFilter.sharedMesh;
	        }
	        else
	        {
	            _newMeshCollider = new MeshCollider()
	            {
	                enabled = true,
	                inflateMesh = true,
	                sharedMesh = addedMesh ? _newMeshFilter.sharedMesh : _oldMeshFilter.sharedMesh
	            };
	        }
	        obj.gameObject.SetActive(true);
	    }*/

		private static void CheckIfExistsInScene(GameObject obj)
		{
			if (obj.scene.IsValid()) return;
			_storedAsset = obj;
			_sourceGeo = Instantiate(obj);
			_addedToScene = true;
		}

		private static void CreateTerrainAsset(TerrainData terrainData)
		{
			var texPath = AssetDatabase.GetAssetPath(_storedAsset == null ? _sourceGeo : _storedAsset);
			texPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1);
			if (texPath == "") texPath = AssetsFolder;
			var path = _radioSelection == 0 ? texPath : AssetsFolder + _saveLocation;
			FuzzyTools.CheckIfPathExists(path);

			path += _terrainName + Asset;

			AssetDatabase.CreateAsset(terrainData, AssetDatabase.GenerateUniqueAssetPath(path));
		}

		public static void ShowProgressBar(float progress, float maxProgress)
		{
			var p = progress / maxProgress;
			EditorUtility.DisplayProgressBar("Creating Terrain...", Mathf.RoundToInt(p * 100f) + " %", p);
		}
	}

	public class ConvertTerrainToModel : EditorWindow
	{
		private const string PickOne = "Add Terrain Source: ";
		private const string UsingTerrain = "Using Terrain";
		private const string UsingTerrainData = "Using TerrainData";
		private const string OrThis = "     Or      ";
		private const string SourceTerrain = "Source Terrain (Scene) ";
		private const string SourceTerrainData = "Source TerrainData (Assets)";
		private const string Convert = "Convert";
		private const string SaveFormat = "Save Format";
		private const string TopologyFormat = "Topology Mode";
		private const string SaveResolution = "Converted Resolution";
		private const string ModelName = "New Model Name: ";
		private const string DefaultModelName = "New Model";

		private static Terrain _sourceTerrain;
		private static TerrainData _sourceTerrainData;

		private static ModelFormat _saveFormat;
		private static TopologyMode _topologyMode;
		private static Resolution _resolution;

		private static Vector3 _terrainPos;

		private static int _radioSelection = 0;

		//private const string Instruction = "If you are wanting to add a terrain from Assets, add it to the TerrainData. If you are wanting to add it from the scene, add it directly to the "
		private const string ConvertedLocation = "Save Converted File to:";
		private static readonly string[] RadioOptions = {"Origin File Location", "Custom Location"};
		private static string _saveLocation = FuzzyTools.DefaultModelPath;
		private static GUIStyle _bold = new GUIStyle();
		private static GUIStyle _smallFont = new GUIStyle();
		private static GUIStyle _largeFont = new GUIStyle();

		//private static readonly Vector2 MinSize1 = new Vector2(300, 240);
		private static readonly Vector2 OtherMinSize1 = new Vector2(300, 259);

		//private static readonly Vector2 MinSize2 = new Vector2(300, 204);
		private static readonly Vector2 OtherMinSize2 = new Vector2(300, 222);


		//private static int _changeNameSelection = 0;
		private static string _modelName = "";

		//private static GameObject _storedAsset = null;
		//private static bool _addedToScene = false;
		private static bool _noTerrainOrData = true;


		[MenuItem("Assets/FuzzyTools/Convert/Terrain To Model")]
		private static void OpenTerrainToOBJAssets()
		{
			var terrain = Selection.activeObject as TerrainData;
			Init(null, terrain);
		}
		
		[MenuItem("GameObject/FuzzyTools/Convert/Terrain To Model", false, 6)]
		private static void OpenTerrainToOBJGameObject()
		{
			var terrain = Selection.activeGameObject.GetComponent<Terrain>();
			Init(terrain, null);
		}

		public static void Init(Terrain source, TerrainData sourceData)
		{
			_sourceTerrain = source;
			_sourceTerrainData = sourceData;
			_saveFormat = FuzzyTools.DefaultModelFormat;
			_topologyMode = FuzzyTools.DefaultTopologyMode;
			_resolution = FuzzyTools.DefaultMeshResolution;
			_radioSelection = 0;
			_saveLocation = FuzzyTools.DefaultModelPath;
			_bold.fontStyle = FontStyle.Bold;
			_smallFont.fontSize = 6;
			_largeFont.fontSize = 15;
			_largeFont.fontStyle = FontStyle.Bold;
			//_changeNameSelection = 0;
			//_storedAsset = null;
			//_addedToScene = false;
			_modelName = _sourceTerrain != null ? _sourceTerrain.name : DefaultModelName;

			var window = GetWindow(typeof(ConvertTerrainToModel), true, "Terrain To Model");
			var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
			if (icon == null) return;
			window.titleContent.image = icon;
		}

		private void OnGUI()
		{
			var textDimensions = GUI.skin.label.CalcSize(new GUIContent(SourceTerrainData));
			EditorGUIUtility.labelWidth = textDimensions.x;
			if (_sourceTerrainData == null && _sourceTerrain == null)
			{
				EditorGUILayout.LabelField(PickOne, _largeFont);
				_sourceTerrain =
					(Terrain) EditorGUILayout.ObjectField(SourceTerrain, _sourceTerrain, typeof(Terrain), true);

				EditorGUILayout.LabelField(OrThis);

				_sourceTerrainData = (TerrainData) EditorGUILayout.ObjectField(SourceTerrainData, _sourceTerrainData,
					typeof(TerrainData), true);

				//EditorGUILayout.TextArea("",GUI.skin.horizontalSlider);
				_noTerrainOrData = true;
			}
			else if (_sourceTerrain != null)
			{
				EditorGUILayout.LabelField(UsingTerrain, _largeFont);
				_sourceTerrain =
					(Terrain) EditorGUILayout.ObjectField(SourceTerrain, _sourceTerrain, typeof(Terrain), true);
				_noTerrainOrData = false;
			}
			else if (_sourceTerrainData != null)
			{
				EditorGUILayout.LabelField(UsingTerrainData, _largeFont);
				_sourceTerrainData = (TerrainData) EditorGUILayout.ObjectField(SourceTerrainData, _sourceTerrainData,
					typeof(TerrainData), true);
				_noTerrainOrData = false;
			}

			EditorGUILayout.Space();

			_modelName = EditorGUILayout.TextField(ModelName, _modelName);

			if (_modelName == "") _modelName = _sourceTerrain != null ? _sourceTerrain.name : DefaultModelName;
			if (_modelName == "///") _modelName = "Pupper 3 is Ben The Creator";

			//_changeNameSelection = EditorGUILayout.SelectableLabel()
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(ConvertedLocation, _bold);
			_radioSelection = GUILayout.SelectionGrid(_radioSelection, RadioOptions, RadioOptions.Length,
				EditorStyles.radioButton);

			minSize = _noTerrainOrData ? OtherMinSize1 : OtherMinSize2;
			if (_radioSelection == 1)
			{
				//minSize = _noTerrainOrData ? OtherMinSize1 : OtherMinSize2;
				_saveLocation = EditorGUILayout.TextField(_saveLocation);
			}
			else
			{
				var texPath = _sourceTerrain != null
					? AssetDatabase.GetAssetPath(_sourceTerrain.terrainData)
					: AssetDatabase.GetAssetPath(_sourceTerrainData);

				EditorGUI.BeginDisabledGroup(true);
				if (texPath == "") EditorGUILayout.TextField("");
				else
				{
					if (texPath.StartsWith("Assets/")) texPath = texPath.Substring(6, texPath.Length - 6);
					texPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1);

					EditorGUILayout.TextField(texPath);
				}

				EditorGUI.EndDisabledGroup();
				//minSize = _noTerrainOrData ? MinSize1 : MinSize2;
			}

			if (_saveLocation.Length > 0 && !_saveLocation.EndsWith("/")) _saveLocation += "/";

			EditorGUILayout.Space();

			_saveFormat = (ModelFormat) EditorGUILayout.EnumPopup(SaveFormat, _saveFormat);
			_topologyMode = (TopologyMode) EditorGUILayout.EnumPopup(TopologyFormat, _topologyMode);
			_resolution = (Resolution) EditorGUILayout.EnumPopup(SaveResolution, _resolution);
			EditorGUILayout.Space();

			EditorGUI.BeginDisabledGroup(_sourceTerrain == null && _sourceTerrainData == null);
			if (GUILayout.Button(Convert))
			{
				ConvertTerrain();
			}

			EditorGUI.EndDisabledGroup();

		}

		private void ConvertTerrain()
		{
			//string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");
			if (_sourceTerrainData == null && _sourceTerrain != null) _sourceTerrainData = _sourceTerrain.terrainData;
			if (_sourceTerrainData == null) return;

			var path = _saveLocation;
			if (_radioSelection == 0)
			{
				path = AssetDatabase.GetAssetPath(_sourceTerrainData);
				path = path.Substring(0, path.LastIndexOf("/") + 1);
			}

			FuzzyTools.CheckIfPathExists(path);
			if (path.StartsWith("Assets/"))
				path = path.Substring(6, path.Length - 6);
			if (!path.StartsWith("/")) path = "/" + path;

			_terrainPos = _sourceTerrain != null ? _sourceTerrain.transform.position : Vector3.zero;

			var width = _sourceTerrainData.heightmapWidth; // - 1;
			var height = _sourceTerrainData.heightmapHeight; // - 1;
			var terrainSize = _sourceTerrainData.size;
			var saveRes = (int) Mathf.Pow(2, (int) _resolution);
			terrainSize = new Vector3(terrainSize.x / (width - 1) * saveRes, terrainSize.y,
				terrainSize.z / (height - 1) * saveRes);
			var uvScale = new Vector2(1.0f / (width - 1), 1.0f / height - 1);
			var heights = _sourceTerrainData.GetHeights(0, 0, width, height);

			width = (width - 1) / saveRes + 1;
			height = (height - 1) / saveRes + 1;

			var vertCount = width * height;

			var verts = new Vector3[vertCount];
			var uVs = new Vector2[vertCount];
			//var normals = new Vector3[vertCount];

			var polygons = _topologyMode == TopologyMode.Triangles
				? new int[(width - 1) * (height - 1) * 6]
				: new int[(width - 1) * (height - 1) * 4];

			//Find Verts positions and UVs
			for (var y = 0; y < height - 1; y++)
			{
				for (var x = 0; x < width - 1; x++)
				{
					verts[y * width + x] =
						Vector3.Scale(terrainSize, new Vector3(-y, heights[x * saveRes, y * saveRes], x)) + _terrainPos;
					uVs[y * width + x] = Vector2.Scale(new Vector2(x * saveRes, y * saveRes), uvScale);
				}
			}

			var polygonIndex = 0;
			if (_topologyMode == TopologyMode.Triangles)
			{
				// Build triangle indices: 3 indices into vertex array for each triangle
				for (int y = 0; y < height - 1; y++)
				{
					for (int x = 0; x < width - 1; x++)
					{
						// For each grid cell output two triangles
						polygons[polygonIndex++] = (y * width) + x;
						polygons[polygonIndex++] = ((y + 1) * width) + x;
						polygons[polygonIndex++] = (y * width) + x + 1;

						polygons[polygonIndex++] = ((y + 1) * width) + x;
						polygons[polygonIndex++] = ((y + 1) * width) + x + 1;
						polygons[polygonIndex++] = (y * width) + x + 1;
					}
				}
			}
			else
			{
				// Build quad indices: 4 indices into vertex array for each quad
				for (var y = 0; y < height - 1; y++)
				{
					for (var x = 0; x < width - 1; x++)
					{
						// For each grid cell output one quad
						polygons[polygonIndex++] = (y * width) + x;
						polygons[polygonIndex++] = ((y + 1) * width) + x;
						polygons[polygonIndex++] = ((y + 1) * width) + x + 1;
						polygons[polygonIndex++] = (y * width) + x + 1;
					}
				}
			}

			switch (_saveFormat)
			{
				//case ModelFormat.FBX://TODO
				//    break;
				case ModelFormat.OBJ:
					path += _modelName;
					CreateModel.ObjFile<ConvertTerrainToModel>(verts, polygons, null, uVs, path, _topologyMode);
					break;
			}

			if ((int) _resolution > 2)
			{
				//EditorUtility.DisplayDialog("Saving Model", "Depending on your settings, this may take some time.",
				//    "Okay");
				EditorUtility.DisplayDialog("Saving model",
					"Depending on your settings, this may take some time. \n" +
					" Smaller Resolutions may not show a progress bar.", "Okay");
			}

			_sourceTerrain = null;
			_sourceTerrainData = null;
		}
	}

	public class ConvertMeshToModel : EditorWindow
	{
		private static GameObject _sourceObj;
		private static Mesh _sourceMesh;

		private static string _modelName = "New Model";
		private static string _saveLocation = FuzzyTools.DefaultModelPath;
		private static ModelFormat _saveFormat = FuzzyTools.DefaultModelFormat;

		private static readonly GUIStyle Bold = new GUIStyle();
		private static readonly GUIStyle SmallFont = new GUIStyle();
		private static readonly GUIStyle LargeFont = new GUIStyle();

		private const string PickOne = "Add Mesh Source: ";
		private const string UsingGameObj = "Using GameObject";
		private const string UsingMesh = "Using Mesh";
		private const string OrThis = "     Or      ";
		private const string SourceGameObj = "Source GameObject ";
		private const string SourceMesh = "Source Mesh";
		private const string ModelName = "New Model Name: ";
		private const string DefaultModelName = "New Model";
		private const string ConvertedLocation = "Save Converted File to:";
		private const string SaveFormat = "Save Format";
		private const string ConvertButton = "Convert";

		private static readonly string[] RadioOptions = {"Origin File Location", "Custom Location"};
		private static int _radioSelection = 0;

		private static readonly Vector2 OtherMinSize1 = new Vector2(300, 215);
		private static readonly Vector2 OtherMinSize2 = new Vector2(300, 178);

		[MenuItem("Assets/FuzzyTools/Convert/Model(s)")]
		private static void ConvertModelsAssets()
		{
			var mesh = Selection.activeObject as Mesh;
			GameObject obj = null;
			if (mesh == null)
			{
				obj = Selection.activeObject as GameObject;
				if (obj == null) return;
			}

			Init(obj, mesh);
		}
		
		[MenuItem("GameObject/FuzzyTools/Convert/Model(s)", false, 4)]
		private static void ConvertModelsGameObject()
		{
			var obj = Selection.activeGameObject;
			Init(obj, null);
            
		}
		
		public static void Init(GameObject obj, Mesh mesh)
		{
			_sourceObj = obj;
			_sourceMesh = mesh;
			_saveLocation = FuzzyTools.DefaultModelPath;
			Bold.fontStyle = FontStyle.Bold;
			SmallFont.fontSize = 6;
			LargeFont.fontSize = 15;
			LargeFont.fontStyle = FontStyle.Bold;
			_modelName = DefaultModelName;
			_radioSelection = 0;
			_saveFormat = FuzzyTools.DefaultModelFormat;
			var window = GetWindow(typeof(ConvertMeshToModel), true, "Mesh to Model");
			var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
			if (icon == null) return;
			window.titleContent.image = icon;
		}

		private void OnGUI()
		{
			var noMesh = false;
			if (_sourceObj == null && _sourceMesh == null)
			{
				EditorGUILayout.LabelField(PickOne, LargeFont);
				_sourceObj =
					(GameObject) EditorGUILayout.ObjectField(SourceGameObj, _sourceObj, typeof(GameObject), true);
				EditorGUILayout.LabelField(OrThis);
				_sourceMesh = (Mesh) EditorGUILayout.ObjectField(SourceMesh, _sourceMesh,
					typeof(Mesh), false);

				noMesh = true;
			}
			else if (_sourceObj != null)
			{
				EditorGUILayout.LabelField(UsingGameObj, LargeFont);
				_sourceObj =
					(GameObject) EditorGUILayout.ObjectField(SourceGameObj, _sourceObj, typeof(GameObject), true);
				noMesh = false;
			}
			else if (_sourceMesh != null)
			{
				EditorGUILayout.LabelField(UsingMesh, LargeFont);
				_sourceMesh = (Mesh) EditorGUILayout.ObjectField(SourceMesh, _sourceMesh, typeof(Mesh), false);
				noMesh = false;
			}

			EditorGUILayout.Space();

			_modelName = EditorGUILayout.TextField(ModelName, _modelName);
			if (_modelName == "")
				_modelName = _sourceMesh != null ? _sourceMesh.name :
					_sourceObj != null ? _sourceObj.name : DefaultModelName;

			if (_modelName == "///") _modelName = "Secret Catto is Russel";

			EditorGUILayout.Space();
			EditorGUILayout.LabelField(ConvertedLocation, Bold);
			_radioSelection = GUILayout.SelectionGrid(_radioSelection, RadioOptions, RadioOptions.Length,
				EditorStyles.radioButton);

			minSize = noMesh ? OtherMinSize1 : OtherMinSize2;

			var path = "";
			if (_radioSelection == 1)
			{
				_saveLocation = EditorGUILayout.TextField(_saveLocation);
				path = _saveLocation;
			}
			else
			{
				var texPath = _sourceMesh != null
					? AssetDatabase.GetAssetPath(_sourceMesh)
					: AssetDatabase.GetAssetPath(_sourceObj);


				EditorGUI.BeginDisabledGroup(true);
				if (texPath == "") EditorGUILayout.TextField("");
				else
				{
					if (texPath.StartsWith("Assets/")) texPath = texPath.Substring(6, texPath.Length - 6);
					texPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1);

					EditorGUILayout.TextField(texPath);
				}

				EditorGUI.EndDisabledGroup();
				path = texPath;
				//minSize = _noTerrainOrData ? MinSize1 : MinSize2;
			}

			if (_saveLocation.Length > 0 && !_saveLocation.EndsWith("/")) _saveLocation += "/";

			EditorGUILayout.Space();

			_saveFormat = (ModelFormat) EditorGUILayout.EnumPopup(SaveFormat, _saveFormat);

			EditorGUI.BeginDisabledGroup(noMesh);
			if (GUILayout.Button(ConvertButton))
			{

				ConvertModel(_sourceObj != null ? GetFullMesh(_sourceObj) : _sourceMesh, path, _modelName);

			}

			EditorGUI.EndDisabledGroup();
		}

		private static Mesh GetFullMesh(GameObject obj)
		{
			var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
			var combine = new CombineInstance[meshFilters.Length];

			var i = 0;
			while (i < meshFilters.Length)
			{
				combine[i].mesh = meshFilters[i].sharedMesh;
				combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
				i++;
			}

			var myMesh = new Mesh();

			myMesh.CombineMeshes(combine);

			return myMesh;
		}

		private static void ConvertModel(Mesh newMesh, string path, string name)
		{

			var verts = newMesh.vertices;
			var newObj = new GameObject();
			var newFilter = newObj.AddComponent<MeshFilter>();
			newFilter.mesh = newMesh;

			for (var j = 0; j < verts.Length; j++)
			{
				var newPos = newFilter.transform.TransformPoint(verts[j]);

				verts[j] = new Vector3(-newPos.x, newPos.y, newPos.z);
			}

			var normals = newMesh.normals;
			for (var i = 0; i < normals.Length; i++)
			{
				var newPos = newFilter.transform.TransformDirection(normals[i]);

				normals[i] = new Vector3(-newPos.x, newPos.y, newPos.z);
			}

			DestroyImmediate(newObj);

			FuzzyTools.CheckIfPathExists(path);
			if (path.StartsWith("Assets/"))
				path = path.Substring(6, path.Length - 6);
			if (!path.StartsWith("/")) path = "/" + path;

			path += name;

			if (_saveFormat == ModelFormat.OBJ)
			{
				CreateModel.ObjFile<ConvertMeshToModel>(verts, newMesh.triangles.Reverse().ToArray(), normals,
					newMesh.uv, path, TopologyMode.Triangles);
			}

			Init(null, null);
			AssetDatabase.Refresh();
		}
	}
}