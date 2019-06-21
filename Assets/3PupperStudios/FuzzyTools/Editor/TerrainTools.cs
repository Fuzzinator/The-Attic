using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace FuzzyTools
{
    public enum TerrainPrefabType
    {
        Texture,
        Tree,
        Grass,
        Details,
        Brush
    }

    public class TerrainTools : MonoBehaviour
    {

        public static void AddToSplatPrototypes()
        {
            var terrain = (Terrain) FindObjectOfType(typeof(Terrain));
            if (terrain == null)
            {
                Debug.LogWarning("Terrain Tool requires a terrain be in an open scene.");
                return;
            }
#if UNITY_2018_3_OR_NEWER
            var terrainLayers = new List<TerrainLayer>();
            terrainLayers.AddRange(terrain.terrainData.terrainLayers);
            var terrainAlbedoTextures = new List<Texture2D>();
            var terrainNormalTextures = new List<Texture2D>();
            var originalSplatCount = terrainLayers.Count;
            for (var i = 0; i < terrainLayers.Count; i++)
            {
                terrainAlbedoTextures.Insert(i, terrainLayers[i].diffuseTexture);
                terrainNormalTextures.Insert(i, terrainLayers[i].normalMapTexture);
            }
#endif
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
            var terrainSplats = new List<SplatPrototype>();
            terrainSplats.AddRange(terrain.terrainData.splatPrototypes);
            var terrainAlbedoTextures = new List<Texture2D>();
            var terrainNormalTextures = new List<Texture2D>();
            var originalSplatCount = terrainSplats.Count;

            for (var i = 0; i < terrainSplats.Count; i++)
            {
                terrainAlbedoTextures.Insert(i, terrainSplats[i].texture);
                terrainNormalTextures.Insert(i, terrainSplats[i].normalMap);
            }
#endif


            var selection = Selection.objects;
            foreach (var obj in selection)
            {
                var thisObject = obj as Texture2D;
                if ((thisObject == null))
                {
                    Debug.Log(obj.name + " skipped because not valid target");
                    if (EditorUtility.DisplayDialog(obj.name + " not added.",
                        obj.name + " skipped because it is not a valid target", "Continue", "Cancel"))
                    {
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }

                if (terrainAlbedoTextures.Contains(thisObject) || terrainNormalTextures.Contains(thisObject))
                {
                    var choice = EditorUtility.DisplayDialogComplex("Texture already Exists!",
                        "What would you like to do?", "Replace", "Skip", "Duplicate");
                    switch (choice)
                    {
                        case (0):
                            Debug.Log("Replacing Texture");
                            var path = AssetDatabase.GetAssetPath(obj);
                            var settings = (TextureImporter) AssetImporter.GetAtPath(path);
                            
                            if (settings.textureType == TextureImporterType.NormalMap)
                            {
                                terrainNormalTextures[terrainNormalTextures.IndexOf(obj as Texture2D)] =
                                    obj as Texture2D;
                            }
                            else if (settings.textureType == TextureImporterType.Default)
                            {
                                terrainAlbedoTextures[terrainAlbedoTextures.IndexOf(obj as Texture2D)] =
                                    obj as Texture2D;
                            }

                            break;
                        case (1):
                            Debug.Log("Skipping Texture");
                            continue;
                        case (2):
                            Debug.Log("Duplicating Texture");
                            break;
                    }
                }
                else
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    var settings = (TextureImporter) AssetImporter.GetAtPath(path);

                    switch (settings.textureType)
                    {
                        case TextureImporterType.NormalMap:
                            terrainNormalTextures.Add(thisObject);
                            break;
                        case TextureImporterType.Default:
                            terrainAlbedoTextures.Add(thisObject);
                            break;
                    }
                }
            }

            if (terrainAlbedoTextures.Count != originalSplatCount && terrainNormalTextures.Count > 1)
            {
                TerrainTexturesManager.Init(terrain, terrainAlbedoTextures, terrainNormalTextures);
            }
            else
            {
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                var newSplats = new List<SplatPrototype>();
                foreach (var albedo in terrainAlbedoTextures)
                {
                    var prototype = new SplatPrototype();
                    prototype.texture = albedo;
                    if (terrainNormalTextures.Count == 1)
                    {
                        prototype.normalMap = terrainNormalTextures[0];
                    }

                    newSplats.Add(prototype);
                }

                Undo.RegisterCompleteObjectUndo(terrain.gameObject, "AddNormalsToSplats");
                terrain.terrainData.splatPrototypes = newSplats.ToArray();
                terrain.Flush();
                terrain.terrainData.RefreshPrototypes();
#endif
#if UNITY_2018_3_OR_NEWER
                var newSplats = new List<TerrainLayer>();
                foreach (var albedo in terrainAlbedoTextures)
                {
                    var prototype = new TerrainLayer();
                    prototype.diffuseTexture = albedo;
                    if (terrainNormalTextures.Count == 1)
                    {
                        prototype.normalMapTexture = terrainNormalTextures[0];
                    }
                    newSplats.Add(prototype);
                }
                Undo.RegisterCompleteObjectUndo(terrain.gameObject, "AddNormalsToSplats");
                terrain.terrainData.terrainLayers = newSplats.ToArray();
                terrain.Flush();
                terrain.terrainData.RefreshPrototypes();
#endif
            }
        }

        public static void AddToTreePrototypes()
        {
            var terrain = (Terrain) FindObjectOfType(typeof(Terrain));
            var listOfTrees = new List<TreePrototype>();
            listOfTrees.AddRange(terrain.terrainData.treePrototypes);

            var treePrefabs = new List<GameObject>();


            foreach (var obj in listOfTrees)
            {
                treePrefabs.Add(obj.prefab);
            }

            var selection = Selection.objects;
            foreach (var obj in selection)
            {
                var thisObject = obj as GameObject;
                if ((thisObject == null))
                {
                    Debug.Log(obj.name + " skipped because not valid target");
                    if (EditorUtility.DisplayDialog(obj.name + " not added.",
                        obj.name + " skipped because it is not a valid target", "Continue", "Cancel"))
                    {
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }

                if (treePrefabs.Contains(thisObject))
                {
                    var choice = EditorUtility.DisplayDialogComplex("Tree already Exists!",
                        "What would you like to do?", "Replace Prefab", "Cancel", "Make Copy");
                    switch (choice)
                    {
                        case (0):
                            Debug.Log("Replacing Prefab");
                            treePrefabs[treePrefabs.IndexOf(obj as GameObject)] = obj as GameObject;
                            break;
                        case (1):
                            Debug.Log("Cancelling Process");
                            continue;
                        case (2):
                            Debug.Log("Duplicating Prefab");
                            break;
                    }
                }
                else
                {
                    var prototype = new TreePrototype();
                    prototype.prefab = thisObject;
                    listOfTrees.Add(prototype);
                    Undo.RegisterCompleteObjectUndo(terrain, "AddTreeToTerrain");
                    terrain.terrainData.treePrototypes = listOfTrees.ToArray();
                    terrain.Flush();
                    terrain.terrainData.RefreshPrototypes();
                    EditorUtility.DisplayDialog("Success",
                        obj.name + " successfully added to terrain as Tree" +
                        ". The change may take a moment to take effect.", "Continue");

                }
            }

            terrain.terrainData.RefreshPrototypes();
        }

        public static void AddToDetail(DetailRenderMode mode, bool isMesh)
        {
            var terrain = (Terrain) FindObjectOfType(typeof(Terrain));
            var listOfDetails = new List<DetailPrototype>();
            listOfDetails.AddRange(terrain.terrainData.detailPrototypes);
            var detailPrefabs = new List<GameObject>();
            var grassTex = new List<Texture2D>();
            foreach (var obj in listOfDetails)
            {
                detailPrefabs.Add(obj.prototype);
                if (!isMesh)
                    grassTex.Add(obj.prototypeTexture);
            }

            var selection = Selection.objects;
            foreach (var obj in selection)
            {
                var thisObject = obj as GameObject;
                var thisTexture = obj as Texture2D;
                if ((thisObject == null && isMesh) || (thisTexture == null && !isMesh))
                {
                    if (EditorUtility.DisplayDialog(obj.name + " not added.",
                        obj.name + " skipped because it is not a valid target", "Continue", "Cancel"))
                    {
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }

                if (isMesh)
                {
                    foreach (var rend in thisObject.GetComponentsInChildren<Renderer>())
                    {
                        if (rend.sharedMaterials.Length >= 1) continue;

                        if (EditorUtility.DisplayDialog(obj.name + " may cause issue",
                            obj.name +
                            " has multiple materials and will not display on terrain. Would you like to add to terrain anyway?",
                            "Add Anyway", "Camcel"))
                        {
                            Debug.Log(obj.name + " added to terrain anyway");
                        }
                        else
                        {
                            Debug.Log(obj.name + " not added to terrain");
                            return;
                        }
                    }
                }

                if ((detailPrefabs.Contains(thisObject) && isMesh) || (grassTex.Contains(thisTexture) && !isMesh))
                {
                    var choice = EditorUtility.DisplayDialogComplex("Detail Object already Exists!",
                        "What would you like to do?", "Replace Prefab", "Cancel", "Make Copy");
                    switch (choice)
                    {
                        case (0):
                            Debug.Log("Replacing Prefab");
                            if (isMesh)
                            {
                                detailPrefabs[detailPrefabs.IndexOf(obj as GameObject)] = obj as GameObject;
                            }
                            else
                            {
                                grassTex[grassTex.IndexOf(obj as Texture2D)] = obj as Texture2D;
                            }

                            break;
                        case (1):
                            Debug.Log("Cancelling Process");
                            continue;
                        case (2):
                            Debug.Log("Duplicating Prefab");
                            break;
                    }
                }

                var prototype = new DetailPrototype();
                prototype.renderMode = mode;
                prototype.usePrototypeMesh = isMesh;
                if (isMesh)
                {
                    prototype.prototype = obj as GameObject;
                    //if (fileExists)
                    //    prototype.prototype.name = GetUniqueName(prototype.prototype.name, protoNames);
                }
                else
                {
                    prototype.prototypeTexture = obj as Texture2D;
                    //if (fileExists)
                    //     prototype.prototypeTexture.name = GetUniqueName(prototype.prototype.name, protoNames);
                }

                listOfDetails.Add(prototype);
                Undo.RegisterCompleteObjectUndo(terrain, "AddDetailToTerrain");
                terrain.terrainData.detailPrototypes = listOfDetails.ToArray();
                terrain.Flush();
                terrain.terrainData.RefreshPrototypes();
                EditorUtility.DisplayDialog("Success",
                    obj.name + " successfully added to terrain as " + mode +
                    ". The change may take a moment to take effect.", "Continue");
            }

            terrain.Flush();
            terrain.terrainData.RefreshPrototypes();
        }

        public static void AddToBrushes()
        {
            if (Selection.objects.Length > 0 && EditorUtility.DisplayDialog("Add brushes to terrain?",
                    "Adding these files will create duplicates and place them in the Assets/Gizmos folder and will only work on valid textures. This may require restarting Unity to take effect.",
                    "Continue", "Cancel"))
            {
                var path = "/Gizmos/";
                FuzzyTools.CheckIfPathExists(path);
                foreach (var obj in Selection.objects)
                {
                    var texture = obj as Texture2D;
                    var texName = "brush";
                    if (texture == null)
                    {
                        Debug.Log(obj.name + " skipped because not valid target");
                        if (EditorUtility.DisplayDialog(obj.name + " not added.",
                            obj.name + " skipped because it is not a valid target", "Continue", "Cancel"))
                        {
                            continue;
                        }
                        else
                        {
                            return;
                        }
                    }

                    var texPath = AssetDatabase.GetAssetPath(obj);
                    var texSettings = (TextureImporter) AssetImporter.GetAtPath(texPath);
                    var isReadable = texSettings.isReadable;
                    texSettings.isReadable = true;
                    var compression = texSettings.textureCompression;
                    texSettings.textureCompression = TextureImporterCompression.Uncompressed;
                    AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

                    var validName = path + texName + "_" + 0;
                    var i = 0;

                    while (File.Exists(Application.dataPath + validName + ".png"))
                    {
                        validName = path + texName + "_" + i;
                        i++;
                    }

                    validName += ".png";
                    // Encode texture into PNG
                    var bytes = texture.EncodeToPNG();

                    File.WriteAllBytes(Application.dataPath + validName, bytes);

                    texSettings.isReadable = isReadable;
                    texSettings.textureCompression = compression;
                    AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

                    Debug.Log(obj + " successfully added to terrain brushes");

                    /*string settingsPath = Application.dataPath + validName;//AssetDatabase.GetAssetPath(obj);

                    TextureImporter settings = (TextureImporter)TextureImporter.GetAtPath(settingsPath);

                    float timeOut = 0;
                    while (settings == null && timeOut < 1)
                    {
                        settings = (TextureImporter)TextureImporter.GetAtPath(settingsPath);
                        EditorUtility.DisplayProgressBar("Changing brush import settings", timeOut.ToString(), timeOut);
                        timeOut += .0001f;
                    }
                    if (settings == null)
                    {
                        Debug.Log("Settings still null");
                        return;
                    }

                    TextureImporterAlphaSource alphaSource = settings.alphaSource;
                    settings.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                    bool transparency = settings.alphaIsTransparency;
                    settings.alphaIsTransparency = true;
                    AssetDatabase.ImportAsset(settingsPath, ImportAssetOptions.ForceUpdate);*/
                    //TODO figure out how to convert brushes import settings before it is imported
                }
            }
        }
    }

    public class ShowTerrainPrefabAdder : EditorWindow
    {
        public TerrainPrefabType type;
        static EditorWindow window;
        bool scaleReset = false;
        bool setDetailsScale = false;

        [MenuItem("Assets/FuzzyTools/Terrain/Add To Terrain")]
        private static void AddToTerrain()
        {
            Init();
        }
        [MenuItem("Assets/FuzzyTools/Terrain/Add To Terrain", true)]
        private static bool AddToTerrainValidation()
        {
            if (Selection.objects.Length <= 0) return false;
            var objects = Selection.objects;
            return (objects.Any(obj => obj as GameObject != null || obj as Texture2D != null || obj as Terrain) &&
                    FindObjectOfType<Terrain>() != null);
        }
        
        public static void Init()
        {
            window = GetWindow(typeof(ShowTerrainPrefabAdder));
            window.titleContent.text = "TerrainPrefabAdder";
            window.position = new Rect(mouseOverWindow.position.x, mouseOverWindow.position.y, 400, 100);
            var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
            if (icon == null) return;
            window.titleContent.image = icon;
            window.ShowPopup();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Please select type of asset you would like to add.",
                EditorStyles.wordWrappedLabel);
            type = (TerrainPrefabType) EditorGUILayout.EnumPopup(type);
            var mode = DetailRenderMode.Grass;
            if (type == TerrainPrefabType.Details || type == TerrainPrefabType.Grass)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Please select detail render mode", EditorStyles.wordWrappedLabel);
                mode = (DetailRenderMode) EditorGUILayout.EnumPopup(mode);
                if (!setDetailsScale)
                {
                    scaleReset = false;
                    setDetailsScale = true;
                    window.position = new Rect(window.position.x, window.position.y, 400, 140);
                }
            }
            else
            {
                if (!scaleReset)
                {
                    scaleReset = true;
                    setDetailsScale = false;
                    window.position = new Rect(window.position.x, window.position.y, 400, 100);
                }
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Confirm"))
            {
                switch (type)
                {
                    case (TerrainPrefabType.Texture):
                        TerrainTools.AddToSplatPrototypes();
                        break;
                    case (TerrainPrefabType.Tree):
                        TerrainTools.AddToTreePrototypes();
                        break;
                    case (TerrainPrefabType.Grass):
                        TerrainTools.AddToDetail(mode, false);
                        break;
                    case (TerrainPrefabType.Details):
                        TerrainTools.AddToDetail(mode, true);
                        break;
                    case (TerrainPrefabType.Brush):
                        TerrainTools.AddToBrushes();
                        break;
                }

                this.Close();
            }
        }
    }

    public class TerrainTexturesManager : EditorWindow
    {
        static EditorWindow window;

        static List<Texture2D> albedos = new List<Texture2D>();
        static List<Texture2D> normals = new List<Texture2D>();
        static List<int> normalCount = new List<int>();
        static List<bool> increaseWindow = new List<bool>();
        static Terrain terrain = null;

        public static void Init(Terrain thisTerrain, List<Texture2D> albedoMaps, List<Texture2D> normalMaps)
        {


            window = GetWindow(typeof(TerrainTexturesManager));
            window.titleContent.text = "SplatManager";
            var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
            if (icon == null) return;
            window.titleContent.image = icon;
            window.position = new Rect(mouseOverWindow.position.x, mouseOverWindow.position.y, 400, 100);

            normals = normalMaps;
            albedos = albedoMaps;
            terrain = thisTerrain;
            for (var i = 0; i < albedos.Count; i++)
            {
                normalCount.Add(i);
                increaseWindow.Add(true);
            }

            window.ShowPopup();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please select the normal map for each texture.", EditorStyles.wordWrappedLabel);

            var normalNames = new List<string>();
            foreach (var tex in normals)
                normalNames.Add(tex.name);
            for (var i = 0; i < albedos.Count; i++)
            {
                EditorGUILayout.SelectableLabel(albedos[i].name);
                normalCount[i] = EditorGUILayout.Popup(normalCount[i], normalNames.ToArray());
                if (!increaseWindow[i]) continue;
                
                increaseWindow[i] = false;
                var height = window.position.height + 40;
                window.position = new Rect(window.position.x, window.position.y, window.position.width, height);
                
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Confirm"))
            {
                SetNormals();

            }
        }

        private void SetNormals()
        {
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
            var splatProtos = new List<SplatPrototype>();
            for (var i = 0; i < albedos.Count; i++)
            {
                var prototype = new SplatPrototype();
                prototype.texture = albedos[i];
                prototype.normalMap = normals[normalCount[i]];
                splatProtos.Insert(i, prototype);

            }

            Undo.RegisterCompleteObjectUndo(terrain.gameObject, "AddNormalsToSplats");
            terrain.terrainData.splatPrototypes = splatProtos.ToArray();
#endif
#if UNITY_2018_3_OR_NEWER
            var splatProtos = new List<TerrainLayer>();
            for (var i = 0; i < albedos.Count; i++)
            {
                var prototype = new TerrainLayer();
                prototype.diffuseTexture = albedos[i];
                prototype.normalMapTexture = normals[normalCount[i]];
                splatProtos.Insert(i, prototype);

            }
            Undo.RegisterCompleteObjectUndo(terrain.gameObject, "AddNormalsToSplats");
            terrain.terrainData.terrainLayers = splatProtos.ToArray();
#endif
            terrain.Flush();
            terrain.terrainData.RefreshPrototypes();
            Close();
        }
    }
}