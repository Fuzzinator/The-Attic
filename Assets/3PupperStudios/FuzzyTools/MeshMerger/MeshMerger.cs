using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FuzzyTools
{
    public struct Float3
    {
        public float x;
        public float y;
        public float z;

        public Float3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Float3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public static Float3 Lerp(Float3 one, Float3 two, float lerpValue)
        {
            return new Float3(Mathf.Lerp(one.x, two.x, lerpValue), Mathf.Lerp(one.y, two.y, lerpValue),
                Mathf.Lerp(one.z, two.z, lerpValue));
        }
        
        

        public static Float3 operator -(Float3 valueOne, Float3 valueTwo)
        {
            return new Float3(valueOne.x - valueTwo.x, valueOne.y - valueTwo.y, valueOne.z - valueTwo.z);
        }
        public static Float3 operator -(Float3 valueOne, float valueTwo)
        {
            return new Float3(valueOne.x - valueTwo, valueOne.y - valueTwo, valueOne.z - valueTwo);
        }
        public static Float3 operator -(Float3 valueOne, Vector3 valueTwo)
        {
            return new Float3(valueOne.x - valueTwo.x, valueOne.y - valueTwo.y, valueOne.z - valueTwo.z);
        }
        public static Float3 operator +(Float3 valueOne, Float3 valueTwo)
        {
            return new Float3(valueOne.x + valueTwo.x, valueOne.y + valueTwo.y, valueOne.z + valueTwo.z);
        }
        public static Float3 operator /(Float3 valueOne, Float3 valueTwo)
        {
            return new Float3(valueOne.x / valueTwo.x, valueOne.y / valueTwo.y, valueOne.z / valueTwo.z);
        }
        public static Float3 operator /(Float3 valueOne, float valueTwo)
        {
            return new Float3(valueOne.x / valueTwo, valueOne.y / valueTwo, valueOne.z / valueTwo);
        }
        public static Float3 operator /(float valueOne, Float3 valueTwo)
        {
            return new Float3(valueOne / valueTwo.x, valueOne / valueTwo.y, valueOne / valueTwo.z);
        }
        public static Float3 operator *(Float3 valueOne, Float3 valueTwo)
        {
            return new Float3(valueOne.x * valueTwo.x, valueOne.y * valueTwo.y, valueOne.z * valueTwo.z);
        }
        public static Float3 operator *(Vector3 valueOne, Float3 valueTwo)
        {
            return new Float3(valueOne.x * valueTwo.x, valueOne.y * valueTwo.y, valueOne.z * valueTwo.z);
        }
        public static Float3 operator *(Float3 valueOne, float valueTwo)
        {
            return new Float3(valueOne.x * valueTwo, valueOne.y * valueTwo, valueOne.z * valueTwo);
        }
        
        
    }

    public static class TypeExtensions
    {
        public static Vector3 ToVector3(this Float3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }
        
        public static Float3 ToFloat3(this Vector3 value)
        {
            return new Float3(value.x, value.y, value.z);
        }

        public static bool IsEqualTo<T1>(this T1[] firstArray, T1[] secondArray) where T1 : class
        {
            var equalTo = true;
            var keys1 = firstArray;
            var keys2 = secondArray;
            if (keys1.Length != keys2.Length) return false;
            for (var i = 0; i < keys1.Length; i++)
            {
                if(keys1[i] == keys2[i]) continue;
                equalTo = false;
                break;
            }
            
            return equalTo;
        }

        public static void MatchInWorld(this Transform thisT, Transform newT)
        {
            thisT.position = newT.position;
            thisT.eulerAngles = newT.eulerAngles;
            thisT.rotation = newT.rotation;
            var parent = thisT.parent;
            thisT.parent = newT.parent;
            thisT.localScale = newT.localScale;
            thisT.parent = parent;
        }

        public static void Reset(this Transform transform)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }

    public struct FilterAndMat
    {
        public MeshFilter filter;
        public Material[] mats;
        //public Collider[] colliders;

        public FilterAndMat(MeshFilter filter, Material[] mats)//, Collider[] colliders)
        {
            this.filter = filter;
            this.mats = mats;
            //this.colliders = colliders;
        }
    }

    public struct FilterLOD
    {
        public MeshFilter filter;
        public LODGroup lodGroup;

        public FilterLOD(MeshFilter filter, LODGroup lodGroup)
        {
            this.filter = filter;
            this.lodGroup = lodGroup;
        }
    }

    public class MeshMerger : MonoBehaviour
    {
        public Vector3 scanningArea = Vector3.one*10;
        public Vector3 sections = Vector3.one*2;
        public MeshMergerPolicyList policyList;
        public bool previewSections = true;
        public bool onlyStaticObjects = true;
        
        [Space]
        public bool skipLODObjects = true;

        [HideInInspector] public int selectedLODOption = 0;

        [HideInInspector] public string[] LODOptions =
        {
            "Maintain LOD",
            "Keep Only Highest"
        };
        private Float3 _pieceSize;
        private List<MeshCollider> _generatedColliders = new List<MeshCollider>();
        private MeshFilter[] _existingRenderers;
        private List<Vector3> _scannedSpaces = new List<Vector3>();
        private Dictionary<Vector3, List<FilterAndMat>> _foundMeshes = new Dictionary<Vector3, List<FilterAndMat>>();

        //private Dictionary<Vector3, Dictionary<LODGroup, Dictionary<int, List<FilterAndMat>>>> _lodMeshes =
        //    new Dictionary<Vector3, Dictionary<LODGroup, Dictionary<int, List<FilterAndMat>>>>();
        private Dictionary<Vector3, Dictionary<int, List<FilterAndMat>>> _lodMeshes =
            new Dictionary<Vector3, Dictionary<int, List<FilterAndMat>>>();
        //private Dictionary<Vector3, Collider[]> _foundColliders = new Dictionary<Vector3, Collider[]>();

        private List<GameObject> _scannedObjects = new List<GameObject>();
        
        private Color offWhite = new Color(.6f,.6f,.6f,1);
        private Float3 _halfPiece;

#if UNITY_EDITOR
        
        private void OnValidate()
        {
            if (scanningArea.x < 0) scanningArea.x *= -1;
            if (scanningArea.y < 0) scanningArea.y *= -1;
            if (scanningArea.z < 0) scanningArea.z *= -1;
            if (sections.x < 0) sections.x *= -1;
            if (sections.y < 0) sections.y *= -1;
            if (sections.z < 0) sections.z *= -1;
            if (scanningArea.x == 0) scanningArea.x = 1;
            if (scanningArea.y == 0) scanningArea.y = 1;
            if (scanningArea.z == 0) scanningArea.z = 1;
            if (sections.x == 0) sections.x = 1;
            if (sections.y == 0) sections.y = 1;
            if (sections.z == 0) sections.z = 1;
            _scannedSpaces.Clear();
            if (!previewSections) return;
            GenerateCubes();
        }
        
        public void FindMeshes()
        {
            GenerateColliders();
            GenerateCubes();
            ScanForMeshes();
            
            DeleteColliders();
            if (_scannedObjects.Count == 0)
            {
                Debug.Log("No Mergeable Meshes Found");
                return;
            }
            
            
            MeshMergerTool.Init(_foundMeshes, _lodMeshes, skipLODObjects);
            //_foundMeshes.Clear();
            //_lodMeshes.Clear();
            //_scannedObjects.Clear();
        }
        
#endif
        private void GenerateColliders()
        {
            _existingRenderers = FindObjectsOfType<MeshFilter>();
            foreach (var filter in _existingRenderers)
            {
            if (!filter.GetComponent<MeshRenderer>()) continue;
                var col = filter.gameObject.AddComponent<MeshCollider>();
                col.sharedMesh = filter.sharedMesh;
                _generatedColliders.Add(col);
            }
        }
        
        private void GenerateCubes()
        {
            _scannedSpaces.Clear();
            _pieceSize = scanningArea.ToFloat3() / sections.ToFloat3();
            var totalSpaces = new Float3(scanningArea.x / _pieceSize.x, scanningArea.y / _pieceSize.y,
                scanningArea.z / _pieceSize.z);
            
            var halfSpaces = new Float3(scanningArea.x *.5f, scanningArea.y *.5f, scanningArea.z *.5f);
            
            _halfPiece = _pieceSize*.5f;
            var min = new Float3(transform.position.x - halfSpaces.x + _halfPiece.x,
                transform.position.y - halfSpaces.y + _halfPiece.y,
                transform.position.z - halfSpaces.z + _halfPiece.z);
            
            
            var currentOffset = new Float3(0f, 0f, 0f);
            
            var pos = new Float3(0f, 0f, 0f);
            for(var y = 0f; y < totalSpaces.y; y++)
            {
                var currentY = min.y + currentOffset.y;
                currentOffset.x = 0;
                currentOffset.z = 0;
                for (var x = 0f; x < totalSpaces.x; x++)
                {
                    var currentX = min.x + currentOffset.x;
                    currentOffset.z = 0;
                    for (var i = 0f; i < totalSpaces.z; i++)
                    {
                        var currentZ = min.z + currentOffset.z;
                        pos = new Float3(currentX, currentY, currentZ);
                        if (_scannedSpaces.Contains(pos.ToVector3())) continue;
                        currentOffset.z += _pieceSize.z;
                        _scannedSpaces.Add(pos.ToVector3());
                    }
                    currentOffset.x += _pieceSize.x;
                }
                currentOffset.y += _pieceSize.y;
            }
        }

        private void ScanForMeshes()
        {
            _scannedObjects.Clear();
            _foundMeshes.Clear();
            _lodMeshes.Clear();
            foreach (var space in _scannedSpaces)
            {
                if (!_foundMeshes.ContainsKey(space))
                {
                    var list = new List<FilterAndMat>();
                    _foundMeshes.Add(space, list);
                }

                var colliders = Physics.OverlapBox(space, _halfPiece.ToVector3());
                foreach (var col in colliders)
                {
                    if (onlyStaticObjects)
                    {
                        if (!col.gameObject.isStatic) continue;
                    }

                    if (policyList != null)
                    {
                        if (!policyList.CheckPolicy(col.gameObject)) continue;
                    }
                    if (skipLODObjects)
                    {
                        
                        if(col.GetComponentInChildren<LODGroup>() || col.GetComponentInParent<LODGroup>()) continue;
                        if (_scannedObjects.Contains((col.gameObject))) continue;
                        var filter = col.GetComponent<MeshFilter>();
                        var rend = col.GetComponent<MeshRenderer>();
                        if (col.GetType() != typeof(MeshCollider) || filter == null || rend == null) continue;
                        _scannedObjects.Add(col.gameObject);
                        
                        var obj = new FilterAndMat(filter, rend.sharedMaterials); //, filter.GetComponents<Collider>());

                        _foundMeshes[space].Add(obj);
                    }
                    else
                    {
                        var lOD = col.GetComponentInParent<LODGroup>();
                        if (lOD == null)
                        {
                            if (_scannedObjects.Contains((col.gameObject))) continue;
                            var filter = col.GetComponent<MeshFilter>();
                            var rend = col.GetComponent<MeshRenderer>();
                            if (col.GetType() != typeof(MeshCollider) || filter == null || rend == null) continue;
                            _scannedObjects.Add(col.gameObject);
                            var obj = new FilterAndMat(filter, rend.sharedMaterials); //, filter.GetComponents<Collider>());

                            _foundMeshes[space].Add(obj);
                        }
                        else
                        {
                            /*if (!_lodMeshes[space].ContainsKey(lOD))
                            {
                                _lodMeshes[space].Add(lOD, new Dictionary<int, List<FilterAndMat>>());
                            };*/
                            var lods = lOD.GetLODs();
                            switch (selectedLODOption)
                            {
                                case 0:
                                    if (!_lodMeshes.ContainsKey(space))
                                    {
                                        var d = new Dictionary<int, List<FilterAndMat>>();
                                        _lodMeshes.Add(space, d);
                                    }
                                    for (var i = 0; i<lods.Length; i++)
                                    {
                                        if (!_lodMeshes[space].ContainsKey(i))
                                        {
                                            _lodMeshes[space].Add(i, new List<FilterAndMat>());
                                        }
                                        var iRends = lods[i].renderers;
                                        foreach (var rend in iRends)
                                        {
                                            if (rend == null) continue;
                                            var obj = rend.gameObject;
                                            if (obj != col.gameObject) continue;
                                            if (_scannedObjects.Contains(obj)) continue;
                                            _scannedObjects.Add(obj);
                                            var filter = rend.GetComponent<MeshFilter>();
                                            var lodObj = new FilterAndMat(filter, rend.sharedMaterials);
                                            _lodMeshes[space][i].Add(lodObj);
                                        }
                                    }
                                    break;
                                case 1:
                                    var rends = lods[0].renderers;
                                    foreach (var rend in rends)
                                    {
                                        var obj = rend.gameObject;
                                        if (obj != col.gameObject) continue;
                                        if (_scannedObjects.Contains(obj)) continue;
                                        _scannedObjects.Add(obj);
                                        var filter = rend.GetComponent<MeshFilter>();
                                        var lodObj = new FilterAndMat(filter, rend.sharedMaterials);
                                       _foundMeshes[space].Add(lodObj);
                                    }
                                    break;
                            }
                        }
                    }
                }

                if (_foundMeshes[space].Count > 1) continue;
                _foundMeshes.Remove(space);
            }
        }

        private void DeleteColliders()
        {
            foreach (var col in _generatedColliders)
            {
                DestroyImmediate(col);
            }
            _generatedColliders.Clear();
        }
        
        private void OnDrawGizmos()
        {
            var origColor = Gizmos.color;
            
            Gizmos.color = offWhite;
            foreach (var space in _scannedSpaces)
            {
                Gizmos.DrawWireCube(space, _pieceSize.ToVector3());
            }
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, scanningArea);
            Gizmos.color = origColor;
        }
    }
}