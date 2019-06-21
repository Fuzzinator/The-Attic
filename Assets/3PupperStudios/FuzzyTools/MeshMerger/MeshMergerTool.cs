using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR 
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FuzzyTools
{
#if UNITY_EDITOR
    public class CreateMerger
    {
        private const string meshMerger = "Mesh Merger";
        private const string editorTag = "EditorOnly";

        private static Type[] mergerScript =
        {
            typeof(MeshMerger),
            typeof(MeshMergerPolicyList)
        };

        [MenuItem("GameObject/FuzzyTools/Create Mesh Merger", false, 27)]
        public static void Instantiate()
        {
            var obj = new GameObject(meshMerger, mergerScript)
            {
                tag = editorTag
            };
            var merger = obj.GetComponent<MeshMerger>();
            var policyList = obj.GetComponent<MeshMergerPolicyList>();
            merger.policyList = policyList;
            obj.SetActive(true);
        }
    }

    public struct LODOrigins
    {
        public LODGroup[] originalLodGroups;
        public GameObject[] newGameObjects;

        public LODOrigins(LODGroup[] groups, GameObject[] objects)
        {
            originalLodGroups = groups;
            newGameObjects = objects;
        }
    }

    public class MeshMergerTool : EditorWindow
    {
        #region Const & Static Strings

        private const string meshMerger = "Mesh Merger";
        private const string box = "box";
        private const string mergeAllMatching = "Merge All Matching Material Meshes";
        private const string mergeAllMeshes = "Merge All Meshes";
        private const string groupObjToSection = "Group All Objects To Sections";
        private const string sectionText = "Section ";
        private const string loDSectionText = "LOD Section ";
        private const string colon = ":";
        private const string mergeMatching = "Merge Matching Material Meshes";
        private const string mergeMeshes = "Merge Meshes";
        private const string groupObj = "Group All To Section";
        private const string sourceFilters = "Source MeshFilter: ";
        private const string scannedMesh = "Scanned Mesh";

        private const string saveLoc = "Save Location: ";
        private const string defaultNamePlaceholder = "Section# - MaterialSet#";
        private const string defaultSavePrefix = "Mesh Group ";
        private const string dash = " - ";
        private const string defaultSaveSuffix = "_Merged_Mesh";
//        private static string fileType = dotAsset;
        private const string dotAsset = ".asset";
        private const string dotObj = ".obj";
        private static string saveName = "";

        private static string saveLocation = "Assets/";
        private const string groupName = "New Mesh Name: ";
        private const string nameSequentially = "Name Sequentially:";
        private const string extras = "Copy Extras:";
        private const string copyColliders = "Colliders: ";
        private const string colls = "Colliders";
        private const string copyLayerAndTagInfo = "Tags & Layers";

        private const string doWithOriginals = "After Merging Meshes";

        private static string[] doWithOptions =
        {
            "Group and Disable Originals",
            "Delete Originals",
            "Leave Originals Untouched",
            "Group Originals",
            "Disable Originals"
        };

        private const string saveMesh = "Save New Mesh Method";

        private static string[] saveOptions =
        {
            "As Mesh Asset",
            "As OBJ",
            "Don't Save Mesh"
        };

        #endregion

        #region static ints

        private const int maxVerts = 60000;
        private static int _saveOption = 0;
        private static int _doWithOption = 0;
        private static int _totalCount = 0;
        private static int _saveFolderSelection = 0;

        #endregion

        #region static bools

        private static bool _sequentialName = true;
        private static bool _copyColliders = false;
        private static bool _copyLayerAndTag = false;
        private static bool _ignoreLODs = true;

        #endregion


        private static Dictionary<Vector3, List<FilterAndMat>> _foundMeshes =
            new Dictionary<Vector3, List<FilterAndMat>>();
        private static Dictionary<Vector3, Dictionary<int, List<FilterAndMat>>> _lodMeshes =
            new Dictionary<Vector3, Dictionary<int, List<FilterAndMat>>>();

        
        private static Vector2 _areaScroll = Vector2.zero;
        
        private readonly List<bool> ViewPositions = new List<bool>();
        private readonly List<bool> LodViewPositions = new List<bool>();
        
        private static readonly List<Vector2> SectionScroll = new List<Vector2>();
        private static readonly List<Vector2> LoDSectionScroll = new List<Vector2>();
        
        private static readonly List<Vector2> MatScrolls = new List<Vector2>();
        private static readonly List<Vector2> LoDMatScrolls = new List<Vector2>();
        
        
        private static List<Vector3> _keys = new List<Vector3>();
        private static List<Vector3> _lodKeys = new List<Vector3>();
        private static List<GameObject> saveAndDelete = new List<GameObject>();
        private static string[] allFolders = new []{""};
        
        private static Dictionary<Transform, Transform> _hierarchy = new Dictionary<Transform, Transform>();

        public static void Init(Dictionary<Vector3, List<FilterAndMat>> meshes, Dictionary<Vector3, Dictionary<int, List<FilterAndMat>>> lods, bool ignoreLOD)
        {
            _foundMeshes = meshes;
            _lodMeshes = lods;
            _ignoreLODs = ignoreLOD;
            _totalCount = _foundMeshes.Count + _lodMeshes.Count;//_foundMeshes.Count>_lodMeshes.Count ? _foundMeshes.Count:_lodMeshes.Count;
            _keys = _foundMeshes.Keys.ToList();
            _lodKeys = _lodMeshes.Keys.ToList();
            _keys.AddIfDoesNotContain(_lodKeys);
            
            
            _areaScroll = Vector2.zero;
            saveAndDelete.Clear();
            GetWindow<MeshMergerTool>(false, meshMerger, true);
        }

        public static void SetLength<T>(List<T> myList, int length, T defaultNew)
        {
            if (length > myList.Count)
            {
                for (var i = myList.Count; i < length; i++)
                {
                    myList.Add(defaultNew);
                }
            }
            else
            {
                while (length != myList.Count)
                {
                    myList.RemoveAt(myList.Count - 1);
                }
            }
        }

        private void OnGUI()
        {
            if (_foundMeshes.Count == 0 && _lodMeshes.Count == 0) Close();
            SetLength(ViewPositions, _foundMeshes.Count, false);
            SetLength(SectionScroll, _foundMeshes.Count, Vector2.zero);
            SetLength(MatScrolls, _foundMeshes.Count, Vector2.zero);
            SetLength(LodViewPositions, _totalCount, false);
            SetLength(LoDSectionScroll, _totalCount, Vector2.zero);
            SetLength(LoDMatScrolls, _totalCount, Vector2.zero);
           
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(box);
            _saveOption = EditorGUILayout.Popup(saveMesh, _saveOption, saveOptions);
            if (_saveOption != 2)
            {
                EditorGUILayout.BeginHorizontal();
                if(allFolders[0] == "") UpdateFolders();
                _saveFolderSelection = EditorGUILayout.Popup(saveLoc, _saveFolderSelection, allFolders);
                saveLocation = allFolders[_saveFolderSelection];
                if (!saveLocation.EndsWith("/")) saveLocation += "/";
                //fileType = _saveOption == 0 ? dotAsset : dotObj;
                if(GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), GUILayout.Width(25)))
                {
                    UpdateFolders();
                }
                //EditorGUILayout.LabelField(saveName == ""
                //    ? defaultSavePrefix + dash + defaultNamePlaceholder + defaultSaveSuffix + fileType
                //    : saveName + fileType);
                EditorGUILayout.EndHorizontal();
            }

            saveName = EditorGUILayout.TextField(groupName, saveName);
            _sequentialName = EditorGUILayout.Toggle(nameSequentially, _sequentialName);
            _doWithOption = EditorGUILayout.Popup(doWithOriginals, _doWithOption, doWithOptions);

            EditorGUILayout.LabelField(extras);
            _copyColliders = EditorGUILayout.Toggle(copyColliders, _copyColliders);
            EditorGUI.BeginDisabledGroup(!_copyColliders);
            _copyLayerAndTag = EditorGUILayout.Toggle(copyLayerAndTagInfo, _copyLayerAndTag);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginVertical(box);
            if (GUILayout.Button(mergeAllMatching))
            {
                for (var i = 0; i < _keys.Count; i++)
                {
                    if(_foundMeshes.ContainsKey(_keys[i]))
                    {
                        var currentList = _foundMeshes[_keys[i]];
                        if (currentList.Count == 0)
                        {
                            continue;
                        }

                        MergeMatchingMaterialMeshes(currentList, i);
                    }
                    if(_ignoreLODs) continue;
                    if (!_lodMeshes.ContainsKey(_keys[i])) continue;
                    var lodList = _lodMeshes[_keys[i]];
                    if (lodList.Count == 0)
                    {
                        continue;
                    }
                    if (lodList[0].Count == 0) continue;
                    LODMergeMatching(lodList, i);
                }
            }

            if (GUILayout.Button(mergeAllMeshes))
            {
                for (var i = 0; i < _keys.Count; i++)
                {
                    if(_foundMeshes.ContainsKey(_keys[i]))
                    {
                        var currentList = _foundMeshes[_keys[i]];
                        if (currentList.Count == 0)
                        {
                            continue;
                        }

                        MergeAllMeshes(currentList, i);
                    }
                    if(_ignoreLODs) continue;
                    if (!_lodMeshes.ContainsKey(_keys[i])) continue;
                    var lodList = _lodMeshes[_keys[i]];
                    if (lodList.Count == 0)
                    {
                        continue;
                    }
                    if (lodList[0].Count == 0) continue;
                    LODMergeAll(lodList, i);
                }
            }

            if (GUILayout.Button(groupObjToSection))
            {
                for (var i = 0; i < _foundMeshes.Count; i++)
                {
                    var currentList = _foundMeshes[_keys[i]];
                    if (currentList.Count == 0)
                    {
                        continue;
                    }

                    JustGroup(currentList, i);
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);

            _areaScroll = EditorGUILayout.BeginScrollView(_areaScroll);
//            var lodIndex = 0;
            for (var i = 0; i< _totalCount; i++)
            {
                if(i<_keys.Count)
                {
                    if(_foundMeshes.ContainsKey(_keys[i]))
                    {
                        var currentList = _foundMeshes[_keys[i]];

                        if (currentList.Count != 0)
                        {
                            EditorGUILayout.BeginVertical(box);
                            ViewPositions[i] = EditorGUILayout.Foldout(ViewPositions[i], sectionText + i + colon);
                            if (ViewPositions[i])
                            {
                                EditorGUILayout.BeginVertical(box);
                                EditorGUILayout.BeginHorizontal(box);
                                if (GUILayout.Button(mergeMatching))
                                {
                                    MergeMatchingMaterialMeshes(currentList, i);
                                    if (_ignoreLODs) continue;
                                    if (!_lodMeshes.ContainsKey(_keys[i])) continue;
                                }

                                if (GUILayout.Button(mergeMeshes))
                                {
                                    MergeAllMeshes(currentList, i);
                                    if (_ignoreLODs) continue;
                                    if (!_lodMeshes.ContainsKey(_keys[i])) continue;
                                }

                                if (GUILayout.Button(groupObj))
                                {
                                    JustGroup(currentList, i);
                                }

                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.BeginVertical(box);

                                GUILayoutOption[] sectionOptionSize =
                                {
                                    GUILayout.MinHeight(80),
                                    GUILayout.MaxHeight(40 + currentList.Count * 40)
                                };

                                SectionScroll[i] =
                                    EditorGUILayout.BeginScrollView(SectionScroll[i], sectionOptionSize);
                                if (MatScrolls.Count < currentList.Count)
                                    SetLength(MatScrolls, currentList.Count, Vector2.zero);
                                for (var i2 = 0; i2 < currentList.Count; i2++)
                                {
                                    var meshMat = currentList[i2];
                                    if (meshMat.filter == null) currentList.Remove(meshMat);
                                    meshMat.filter = (MeshFilter) EditorGUILayout.ObjectField(sourceFilters,
                                        meshMat.filter,
                                        typeof(MeshFilter), false);

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.ObjectField(scannedMesh, meshMat.filter.sharedMesh, typeof(Mesh),
                                        false);
                                    EditorGUILayout.BeginVertical();

                                    MatScrolls[i2] = EditorGUILayout.BeginScrollView(MatScrolls[i2]);
                                    foreach (var mat in meshMat.mats)
                                    {
                                        EditorGUILayout.ObjectField(mat.name, mat, typeof(Material), false);
                                    }

                                    EditorGUILayout.EndScrollView();
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.EndHorizontal();
                                }

                                //EditorGUILayout.EndVertical();
                                EditorGUILayout.EndScrollView();
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.EndVertical();
                            }

                            EditorGUILayout.EndVertical();
                        }
                    }
                    if(!_lodMeshes.ContainsKey(_keys[i])) continue;
                    var lodList = _lodMeshes[_keys[i]][0];
                    if(lodList.Count ==0) continue;
                        EditorGUILayout.BeginVertical(box);
                    LodViewPositions[i] = EditorGUILayout.Foldout(LodViewPositions[i], loDSectionText + i + colon);
                    if (LodViewPositions[i])
                    {
                        //if(lodList)
                        EditorGUILayout.BeginVertical(box);
                        EditorGUILayout.BeginHorizontal(box);
                        if (GUILayout.Button(mergeMatching))
                        {
                            if (_ignoreLODs) continue;
                            if (!_lodMeshes.ContainsKey(_keys[i])) continue;
                            LODMergeMatching(_lodMeshes[_keys[i]], i);
                        }
                        if (GUILayout.Button(mergeMeshes))
                        {
                            if (_ignoreLODs) continue;
                            if (!_lodMeshes.ContainsKey(_keys[i])) continue;
                            LODMergeAll(_lodMeshes[_keys[i]], i);
                        }
                        if (GUILayout.Button(groupObj))
                        {
                            JustGroup(lodList, i);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginVertical(box);
                        GUILayoutOption[] sectionOptionSize =
                        {
                            GUILayout.MinHeight(80),
                            GUILayout.MaxHeight(40 + lodList.Count*40)
                        };
                        LoDSectionScroll[i] =
                            EditorGUILayout.BeginScrollView(LoDSectionScroll[i], sectionOptionSize);
                        if (LoDMatScrolls.Count < lodList.Count)
                            SetLength(LoDMatScrolls, lodList.Count, Vector2.zero);
                        
                        for (var i2 = 0; i2 < lodList.Count; i2++)
                        {
                            var meshMat = lodList[i2];
                            if (meshMat.filter == null) lodList.Remove(meshMat);
                            
                            meshMat.filter = (MeshFilter) EditorGUILayout.ObjectField(sourceFilters, meshMat.filter,
                                typeof(MeshFilter), false);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.ObjectField(scannedMesh, meshMat.filter.sharedMesh, typeof(Mesh),
                                false);
                            EditorGUILayout.BeginVertical();

                            LoDMatScrolls[i2] = EditorGUILayout.BeginScrollView(LoDMatScrolls[i2]);
                            foreach (var mat in meshMat.mats)
                            {
                                EditorGUILayout.ObjectField(mat.name, mat, typeof(Material), false);
                            }

                            EditorGUILayout.EndScrollView();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        
                        }
                        EditorGUILayout.EndScrollView();
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                }

            }
            EditorGUILayout.EndScrollView();
        }

        private Dictionary<GameObject, List<LODGroup>> MergeMatchingMaterialMeshes(IEnumerable<FilterAndMat> currentList, int count)
        {
            CheckIfPathExists(saveLocation);

            var meshDictionary = new Dictionary<Material[], List<MeshFilter>>();
            var keys = meshDictionary.Keys.ToArray();
            var objects = new Dictionary<GameObject, List<LODGroup>>();
            foreach (var meshMat in currentList)
            {
                var currentMats = meshMat.mats;
                if (keys.Length == 0)
                {
                    meshDictionary.Add(meshMat.mats, new List<MeshFilter>());
                }

                keys = meshDictionary.Keys.ToArray();
                foreach (var key in keys)
                {
                    if (!key.IsEqualTo(currentMats)) continue;
                    currentMats = key;
                    break;
                }

                if (!meshDictionary.ContainsKey(currentMats))
                {
                    meshDictionary.Add(currentMats, new List<MeshFilter>());
                }

                meshDictionary[currentMats].Add(meshMat.filter);
            }

            Transform colTrans = null;
            var index = 0;
            foreach (var key in keys) //which set of materials
            {
                var meshFilters = meshDictionary[key];
                

                if (_copyColliders)
                {
                    var colliderParent = new GameObject(colls);
                    Undo.RegisterCompleteObjectUndo(colliderParent, "CreateColliders");
                    colTrans = colliderParent.transform;
                    foreach (var mFilter in meshFilters)
                    {
                        var cols = mFilter.GetComponents<Collider>();
                        if (cols == null) continue;
                        foreach (var col in cols)
                        {
                            var obj = new GameObject(col.name + " " + col.GetType());
                            Undo.RegisterCompleteObjectUndo(obj, "CreateColliders");
                            obj.transform.SetParent(colTrans);
                            ComponentUtility.CopyComponent(col);
                            ComponentUtility.PasteComponentAsNew(obj);
                            if (!_copyLayerAndTag) continue;
                            var cObj = col.gameObject;
                            obj.transform.MatchInWorld(cObj.transform);
                            obj.tag = col.tag;
                            obj.layer = cObj.layer;
                            var flags = GameObjectUtility.GetStaticEditorFlags(cObj);
                            GameObjectUtility.SetStaticEditorFlags(obj, flags);
                            obj.hideFlags = cObj.hideFlags;
                        }
                    }
                }

                if (_doWithOption == 2 || _doWithOption == 4)
                {
                    foreach (var fil in meshFilters)
                    {
                        var t = fil.transform;
                        if (_hierarchy.ContainsKey(fil.transform)) continue;
                        _hierarchy.Add(t, t.parent);
                    }
                }

                var actualName = saveName == ""
                    ? defaultSavePrefix + dash + count + dash + index
                    : (_sequentialName ? saveName + dash + count + dash + index : saveName);

                var lodList = new List<FilterLOD>();
                if (!_ignoreLODs)
                {
                    foreach (var fil in meshFilters)
                    {
                        var lodGroup = fil.GetComponentInParent<LODGroup>();
                        if (lodGroup == null) continue;
                        lodList.Add(new FilterLOD(fil, lodGroup));
                    }
                }
                
                var group = GroupMeshes(actualName, meshFilters.ToArray());
                var rot = group.transform.rotation;
                var pos = group.transform.position;
                group.transform.position = Vector3.zero;
                group.transform.rotation = Quaternion.identity;

                if (!_ignoreLODs)
                {
                    foreach (var lod in lodList)
                    {
                        if (!objects.ContainsKey(group))
                        {
                            objects.Add(group, new List<LODGroup>());
                        }
                        objects[group].AddIfDoesNotContain(lod.lodGroup);
                    }
                    
                }
                
                var subMeshes = new List<Mesh>();

                for (var i = 0; i < key.Length; i++) //material in set
                {
                    var thisMatInstances = new CombineInstance[meshFilters.Count];
                    for (var m = 0; m < meshFilters.Count; m++)
                    {
                        thisMatInstances[m] = new CombineInstance()
                        {
                            mesh = meshFilters[m].sharedMesh,
                            transform = meshFilters[m].transform.localToWorldMatrix
                        };
                    }

                    var subMesh = new Mesh();
                    subMesh.CombineMeshes(thisMatInstances, true);
                    subMeshes.Add(subMesh);
                }

                var meshInstances = new CombineInstance[subMeshes.Count];
                for (var i = 0; i < meshInstances.Length; i++)
                {
                    meshInstances[i] = new CombineInstance()
                    {
                        mesh = subMeshes[i],
                        subMeshIndex = 0,
                        transform = Matrix4x4.identity
                    };
                }

                var finalMesh = new Mesh()
                {
                    name = actualName
                };

                finalMesh.CombineMeshes(meshInstances, false);
                var filter = group.AddComponent<MeshFilter>();
                filter.sharedMesh = finalMesh;
                var rend = group.AddComponent<MeshRenderer>();
                rend.sharedMaterials = key;


                group.transform.rotation = rot;
                group.transform.position = pos;

                if (group.transform.childCount == 0)
                {
                    group.transform.Reset();
                }

                
                switch (_doWithOption)
                    {
                        case 0:
                            foreach (var child in group.GetComponentsInChildren<MeshRenderer>())
                            {
                                var obj = child.gameObject;
                                if (obj == group) continue;
                                Undo.RegisterCompleteObjectUndo(obj, "DoWithObjects");
                                obj.SetActive(false);
                            }

                            break;
                        case 1:
                            var children = group.GetComponentsInChildren<MeshRenderer>();
                            for (var i = 0; i < children.Length; i++)
                            {
                                var child = children[i];
                                if (child.gameObject == group) continue;
                                Undo.RegisterCompleteObjectUndo(child.gameObject, "DoWithObjects");
                                if (_ignoreLODs)
                                {
                                    DestroyImmediate(child.gameObject);
                                }
                                else
                                {
                                    saveAndDelete.Add(child.gameObject);
                                }
                            }

                            break;
                        case 2:
                            var theseKeys = _hierarchy.Keys.ToArray();
                            foreach (var thisKey in theseKeys)
                            {
                                thisKey.SetParent(_hierarchy[thisKey], true);
                            }

                            break;
                        case 3:
                            break;
                        case 4:
                            var disableKeys = _hierarchy.Keys.ToArray();
                            foreach (var thisKey in disableKeys)
                            {
                                thisKey.SetParent(_hierarchy[thisKey], true);
                                Undo.RegisterCompleteObjectUndo(thisKey.gameObject, "DoWithObjects");
                                thisKey.gameObject.SetActive(false);
                            }

                            break;
                    }
                
                if (_copyColliders && colTrans != null)
                {
                    colTrans.transform.SetParent(group.transform);
                }


                switch (_saveOption)
                {
                    case 0:
                        AssetDatabase.CreateAsset(finalMesh, saveLocation + actualName + dotAsset);
                        break;
                    case 1:
                        ConvertModel(finalMesh, saveLocation, actualName);
                        break;
                    case 2:
                        break;
                }
                
                index++;
            }

            return objects;
            //return mesh;
        }

        private Dictionary<GameObject, List<LODGroup>> MergeAllMeshes(List<FilterAndMat> currentList, int count)
        {
            CheckIfPathExists(saveLocation);

            var meshFilters = currentList.Select(filter => filter.filter).ToArray();
            var materials = new List<Material>();
            var objects = new Dictionary<GameObject, List<LODGroup>>();
            foreach (var list in currentList)
            {
                var mats = list.mats;
                foreach (var mat in mats)
                {
                    if (materials.Contains(mat)) continue;
                    materials.Add(mat);
                }
            }

            Transform colTrans = null;

            if (_copyColliders)
            {
                var colliderParent = new GameObject(colls);
                Undo.RegisterCompleteObjectUndo(colliderParent, "CreateColliders");
                colTrans = colliderParent.transform;
                foreach (var mFilter in meshFilters)
                {
                    var cols = mFilter.GetComponents<Collider>();
                    if (cols == null) continue;
                    foreach (var col in cols)
                    {
                        var obj = new GameObject(col.name + " " + col.GetType());
                        Undo.RegisterCompleteObjectUndo(obj, "CreateColliders");
                        obj.transform.SetParent(colTrans);
                        ComponentUtility.CopyComponent(col);
                        ComponentUtility.PasteComponentAsNew(obj);
                        if (!_copyLayerAndTag) continue;
                        var cObj = col.gameObject;
                        obj.transform.MatchInWorld(cObj.transform);
                        obj.tag = col.tag;
                        obj.layer = cObj.layer;
                        obj.isStatic = cObj.isStatic;
                        obj.hideFlags = cObj.hideFlags;
                    }
                }
            }

            if (_doWithOption == 2 || _doWithOption == 4)
            {
                foreach (var fil in meshFilters)
                {
                    var t = fil.transform;
                    if (_hierarchy.ContainsKey(fil.transform)) continue;
                    _hierarchy.Add(t, t.parent);
                }
            }

            var actualName = saveName == ""
                ? defaultSavePrefix + dash + count
                : (_sequentialName ? saveName + dash + count : saveName);

            var lodList = new List<FilterLOD>();
            if (!_ignoreLODs)
            {
                foreach (var fil in meshFilters)
                {
                    var lodGroup = fil.GetComponentInParent<LODGroup>();
                    if (lodGroup == null) continue;
                    lodList.Add(new FilterLOD(fil, lodGroup));
                }
            }
            
            var group = GroupMeshes(actualName, meshFilters);
            
            var rot = group.transform.rotation;
            var pos = group.transform.position;
            group.transform.position = Vector3.zero;
            group.transform.rotation = Quaternion.identity;
            
            if (!_ignoreLODs)
            {
                foreach (var lod in lodList)
                {
                    if (!objects.ContainsKey(group))
                    {
                        objects.Add(group, new List<LODGroup>());
                    }
                    objects[group].AddIfDoesNotContain(lod.lodGroup);
                }
            }

            var subMeshes = new List<Mesh>();
            foreach (var mat in materials)
            {
                var thisMatInstances = new List<CombineInstance>();
                foreach (var filter in meshFilters)
                {
                    var render = filter.GetComponent<MeshRenderer>();
                    if (render == null) continue;
                    var mats = render.sharedMaterials;
                    for (var i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] != mat) continue;
                        var instance = new CombineInstance()
                        {
                            mesh = filter.sharedMesh,
                            subMeshIndex = i,
                            transform = filter.transform.localToWorldMatrix
                        };
                        thisMatInstances.Add(instance);
                    }
                }

                var subMesh = new Mesh();
                subMesh.CombineMeshes(thisMatInstances.ToArray(), true);
                subMeshes.Add(subMesh);
            }


            var meshInstances = new List<List<CombineInstance>>();
            var vertCount = 0;
            var meshCount = 0;
            var extraMeshes = new List<GameObject>();
            foreach (var mesh in subMeshes)
            {
                vertCount += mesh.vertexCount;
                var instance = new CombineInstance()
                {
                    mesh = mesh,
                    subMeshIndex = 0,
                    transform = Matrix4x4.identity
                };
                if(meshInstances.Count == 0) meshInstances.Add(new List<CombineInstance>());
#if UNITY_2018_3_OR_NEWER

#else
                if (vertCount >= maxVerts)
                {
                    Debug.LogError(
                        "Vert count of merged meshes too high consider adding more sections to the scanner" +
                        " and try again. This will not be supported in the future as unity 2018.3 and on does not" +
                        " have this problem");
                    Undo.PerformUndo();
                    return null;
                }
#endif
               /*
     
                    vertCount = 0;
                    meshInstances.Add(new List<CombineInstance>());
                    meshCount++;
                    var obj = new GameObject("SubMesh " + meshCount);
                    obj.transform.SetParent(group.transform);
                    obj.transform.MatchInWorld(group.transform);
                    obj.transform.SetSiblingIndex(meshCount);
                    extraMeshes.Add(obj);

                }*/
                meshInstances[meshCount].Add(instance);
                
            }
            
            if(meshInstances.Count == 0)
            {
                DestroyImmediate(group);
                return null;
            }

            var finalMesh = new Mesh()
            {
                name = actualName
            };
            var extras = new Mesh[meshCount];

            
            for(var i =0; i<meshInstances.Count; i++)
            {
                var me = meshInstances[i];
                if (me == meshInstances[0])
                {
                    finalMesh.CombineMeshes(me.ToArray(), false);
                    var thisFilter = group.AddComponent<MeshFilter>();
                    thisFilter.sharedMesh = finalMesh;
                    var rend = group.AddComponent<MeshRenderer>();
                    rend.sharedMaterials = materials.ToArray();
                }
                else
                {

                    extras[i-1] = new Mesh() {name = actualName + "SubMesh " + i};
                    extras[i-1].CombineMeshes(me.ToArray(), false);
                    var thisFilter = extraMeshes[i-1].AddComponent<MeshFilter>();
                    thisFilter.sharedMesh = extras[i-1];
                    var rend = extraMeshes[i-1].AddComponent<MeshRenderer>();
                    rend.sharedMaterials = materials.ToArray();
                }
            }
            


            group.transform.rotation = rot;
            group.transform.position = pos;
            if (group.transform.childCount == 0)
            {
                group.transform.Reset();
            }
            

            switch (_doWithOption)
            {
                case 0:
                    foreach (var child in group.GetComponentsInChildren<MeshRenderer>())
                    {
                        var obj = child.gameObject;
                        if (obj == group) continue;
                        if (extraMeshes.Contains(obj)) continue;
                        Undo.RegisterCompleteObjectUndo(obj, "DoWithObjects");
                        obj.SetActive(false);
                    }

                    break;
                case 1:
                    var children = group.GetComponentsInChildren<MeshRenderer>();
                    for (var i = 0; i < children.Length; i++)
                    {
                        var child = children[i];
                        if (child.gameObject == group) continue;
                        if (extraMeshes.Contains(child.gameObject)) continue;
                        Undo.RegisterCompleteObjectUndo(child.gameObject, "DoWithObjects");
                        if (_ignoreLODs)
                        {
                            DestroyImmediate(child.gameObject);
                        }
                        else
                        {
                            saveAndDelete.Add(child.gameObject);
                        }
                    }

                    break;
                case 2:
                    var theseKeys = _hierarchy.Keys.ToArray();
                    foreach (var thisKey in theseKeys)
                    {
                        thisKey.SetParent(_hierarchy[thisKey], true);
                    }

                    break;
                case 3:
                    break;
                case 4:
                    var disableKeys = _hierarchy.Keys.ToArray();
                    foreach (var thisKey in disableKeys)
                    {
                        thisKey.SetParent(_hierarchy[thisKey], true);
                        Undo.RegisterCompleteObjectUndo(thisKey.gameObject, "DoWithObjects");
                        thisKey.gameObject.SetActive(false);
                    }

                    break;
            }

            if (_copyColliders && colTrans != null)
            {
                colTrans.transform.SetParent(group.transform);
            }


            switch (_saveOption)
            {
                case 0:
                    AssetDatabase.CreateAsset(finalMesh, saveLocation + actualName + dotAsset);
                    break;
                case 1:
                    ConvertModel(finalMesh, saveLocation, actualName);
                    break;
                case 2:
                    break;
            }
            return objects;
        }

        private void LODMergeMatching(Dictionary<int, List<FilterAndMat>> lodFilters, int section)
        {
            var keys = lodFilters.Keys.ToArray();
            var lods = new Dictionary<int, Dictionary<GameObject, List<LODGroup>>>();
            
//            var allMats = new List<Material[]>();
            for (var i = 0; i < keys.Length; i++)
            {
                
                var tempSet = MergeMatchingMaterialMeshes(lodFilters[i], i);
                var tempKeys = tempSet.Keys.ToArray();
                foreach (var key in tempKeys)
                {
                    if (!lods.ContainsKey(i))
                    {
                        lods.Add(i, new Dictionary<GameObject, List<LODGroup>>());
                    }if (!lods[i].ContainsKey(key))
                    {
                        lods[i].Add(key, new List<LODGroup>());
                    }
                    lods[i][key].AddIfDoesNotContain(tempSet[key]);
                }
                
            }

            var firstSet = 0;
            while (!lods.ContainsKey(firstSet))
            {
                firstSet++;
                if (firstSet > 1000) break;
            }
            var lodSet = 0;
            foreach (var lod0 in lods[firstSet])
            {
                var toGroup = new List<GameObject>()
                {
                    lod0.Key
                };
                var lod0Value = lod0.Value;
                for (var i = 1; i < lods.Count; i++)
                {
                    foreach (var currentLOD in lods[i])
                    {
                        var matching = currentLOD.Value.All(v => lod0Value.Contains(v));
                        if (!matching) continue;
                        toGroup.Add(currentLOD.Key);
                    }
                }
                var group = GroupMeshes("LOD Group " + section + " " + lodSet, toGroup.ToArray());
                var lodGroup = group.AddComponent<LODGroup>();
                lodGroup.fadeMode = lod0.Value[0].fadeMode;
                var newLoDs = lodGroup.GetLODs();
                
                var level = 0;
                foreach (var obj in toGroup)
                {
                    var rend = new Renderer[]
                    {
                        obj.GetComponent<Renderer>()
                    };
                    var newLoD = new LOD(newLoDs[level].screenRelativeTransitionHeight, rend);
                    newLoDs[level] = newLoD;
                    level++;
                }
                lodGroup.SetLODs(newLoDs);
                lodGroup.RecalculateBounds();
                lodSet++;
            }
        }

        private void LODMergeAll(Dictionary<int, List<FilterAndMat>> lodFilters, int section)
        {
            var keys = lodFilters.Keys.ToArray();
            var lods = new Dictionary<int, Dictionary<GameObject, List<LODGroup>>>();
            
//            var allMats = new List<Material[]>();
            for (var i = 0; i < keys.Length; i++)
            {
                var tempSet = MergeAllMeshes(lodFilters[i], i);
                if (tempSet == null) continue;
                var tempKeys = tempSet.Keys.ToArray();
                foreach (var key in tempKeys)
                {
                    if (!lods.ContainsKey(i))
                    {
                        lods.Add(i, new Dictionary<GameObject, List<LODGroup>>());
                    }if (!lods[i].ContainsKey(key))
                    {
                        lods[i].Add(key, new List<LODGroup>());
                    }
                    lods[i][key].AddIfDoesNotContain(tempSet[key]);
                }
                
            }
            var firstSet = 0;
            while (!lods.ContainsKey(firstSet))
            {
                firstSet++;
                if (firstSet > 1000) break;
            }
            var lodSet = 0;
            foreach (var lod0 in lods[firstSet])
            {
                var toGroup = new List<GameObject>()
                {
                    lod0.Key
                };
                var lod0Value = lod0.Value;
                for (var i = 1; i < lods.Count; i++)
                {
                    foreach (var currentLOD in lods[i])
                    {
                        var matching = currentLOD.Value.All(v => lod0Value.Contains(v));
                        if (!matching) continue;
                        toGroup.Add(currentLOD.Key);
                    }
                }
                var group = GroupMeshes("LOD Group " + section + " " + lodSet, toGroup.ToArray());
                var lodGroup = group.AddComponent<LODGroup>();
                lodGroup.fadeMode = lod0.Value[0].fadeMode;
                var newLoDs = lodGroup.GetLODs();
                
                var level = 0;
                foreach (var obj in toGroup)
                {
                    var rend = new Renderer[]
                    {
                        obj.GetComponent<Renderer>()
                    };
                    var newLoD = new LOD(newLoDs[level].screenRelativeTransitionHeight, rend);
                    newLoDs[level] = newLoD;
                    level++;
                }
                lodGroup.SetLODs(newLoDs);
                lodGroup.RecalculateBounds();
                lodSet++;
            }
        }

        private static GameObject GroupMeshes(string groupName, MeshFilter[] sels)
        {
            var group = new GameObject(groupName);
            group.transform.SetParent(null, true);
            var transforms = sels.Select(s => s.transform).ToArray();

            group.transform.position = transforms.AveragePosition(); // wont work without FuzzyTools

            //Register Groups creation to Undo.
            Undo.RegisterCreatedObjectUndo(group, "CreateGroup");
            //Loop through each object in the selection setting the parent of the transform to be the new group while
            //registering the action to Undo at the same time
            foreach (var obj in sels)
            {
                Undo.SetTransformParent(obj.transform, group.transform, "GroupObjects");
            }

            return group;
        }
        
        private static GameObject GroupMeshes(string groupName, GameObject[] sels)
        {
            var group = new GameObject(groupName);
            group.transform.SetParent(null, true);
            var transforms = sels.Select(s => s.transform).ToArray();

            group.transform.position = transforms.AveragePosition(); // wont work without FuzzyTools

            //Register Groups creation to Undo.
            Undo.RegisterCreatedObjectUndo(group, "CreateGroup");
            //Loop through each object in the selection setting the parent of the transform to be the new group while
            //registering the action to Undo at the same time
            foreach (var obj in sels)
            {
                Undo.SetTransformParent(obj.transform, group.transform, "GroupObjects");
            }

            return group;
        }

        private static void CheckIfPathExists(string path)
        {
            if (!path.StartsWith("Assets/")) path = "Assets/" + path;

            if (path.Contains('\\'))
            {
                path = path.Replace('\\', '/');
            }

            var split = path.Split('/');
            var checkedPath = "";
            var folder = "";
            for (var i = 0; i < split.Length; i++)
            {
                if (split[i] == "" || split[i] == " ") continue;

                if (i > 0)
                {
                    if (!AssetDatabase.IsValidFolder(checkedPath + split[i]))
                    {
                        var guid = AssetDatabase.CreateFolder(folder, split[i]);
                        AssetDatabase.GUIDToAssetPath(guid);
                    }
                }

                folder = checkedPath + split[i];
                checkedPath += split[i] + "/";


            }
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

            CheckIfPathExists(path);
            if (path.StartsWith("Assets/"))
                path = path.Substring(6, path.Length - 6);
            if (!path.StartsWith("/")) path = "/" + path;

            path += name;

            MeshTools.CreateModel.ObjFile(verts, newMesh.triangles.Reverse().ToArray(), normals, newMesh.uv, path);

            AssetDatabase.Refresh();
        }

        private void JustGroup(IEnumerable<FilterAndMat> currentList, int section)
        {
            var filters = new List<MeshFilter>();
            foreach (var obj in currentList)
            {
                filters.Add(obj.filter);
            }

            var actualName = saveName == ""
                ? defaultSavePrefix + dash + section
                : (_sequentialName ? saveName + dash + section : saveName);
            GroupMeshes(actualName, filters.ToArray());
        }

        private static void UpdateFolders()
        {
            var folderList = new List<string>();
            folderList.AddRange(AssetDatabase.GetAllAssetPaths());
            for (var i = 0; i< folderList.Count; i++)
            {
                var folder = folderList[i];
                
                if (!folder.StartsWith("Assets"))
                {
                    folderList.Remove(folder);
                    i--;
                    continue;
                }
                var index = folder.LastIndexOf("/");
                if (index > 0)
                    folder = folder.Substring(0, index);
                folderList[i] = folder;

            }

            allFolders = folderList.ToArray();
        }
    }
#endif
}