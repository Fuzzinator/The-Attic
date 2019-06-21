using System.IO;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR 
using UnityEditor;
#endif
using UnityEngine;
using System;

namespace FuzzyTools.MeshTools
{
    #if UNITY_EDITOR
    public class CreateModel
    {
        private const int Two = 2;
        private const int Tris = 3;
        private const int Quads = 4;
        private const int VertCap = 20;
        private const int UVCap = 22;
        private const int TrisCap = 43;

        private const string OBJ = ".obj";
        private const string MakingVerts = "Calculating Vertecies";
        private const string MakingUVs = "Generating UVs";
        private const string MakingTris = "Creating Tris";
        private const string MakingQuads = "Creating Quads";

        private static int _counter = 0;
        private static int _currentCount = 0;
        private static int _maxCount = 0;
        private const int UpdateSpeed = 10000;

        public static void ObjFile(IList<Vector3> verts, int[] polys, Vector3[] normals, IList<Vector2> uVs,
            string newName)
        {
            var validName = "/" + newName;
            var nameCount = 0;
            //Debug.Log("Checking: " + Application.dataPath + validName + OBJ);
            while (File.Exists(Application.dataPath + validName + OBJ))
            {
                //Debug.Log(Application.dataPath + validName + OBJ + " Already Exists");
                nameCount++;
                validName = "/" + newName + " " + nameCount.ToString("00");

                // Debug.Log("Trying: " + Application.dataPath + validName + OBJ);
            }

            var saveTo = Application.dataPath + validName + OBJ;
            //var savingModel = false;
            var streamWriter = new StreamWriter(saveTo);
            try
            {
                streamWriter.WriteLine("# Convert Terrain to OBJ File");
                //Debug.Log("Starting To Write");
                //write verts
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                _counter = _currentCount = 0;
                _maxCount = (verts.Count * Two + polys.Length / Tris);
                for (var i = 0; i < verts.Count; i++)
                {
                    ProgressBar(MakingVerts);
                    var stringB = new StringBuilder("v ", VertCap);


                    stringB.Append(verts[i].x.ToString()).Append(" ").Append(verts[i].y.ToString()).Append(" ")
                        .Append(verts[i].z.ToString());
                    streamWriter.WriteLine(stringB);
                }

                //Write normals
                if (normals != null)
                {
                    for (var i = 0; i < normals.Length; i++)
                    {
                        ProgressBar("Writing Normals");
                        var stringB = new StringBuilder("vn ");
                        stringB.Append(normals[i].x.ToString()).Append(" ").Append(normals[i].y.ToString()).Append(" ")
                            .Append(normals[i].z.ToString());
                        streamWriter.WriteLine(stringB);
                    }
                }

                //writeUVs
                for (var i = 0; i < uVs.Count; i++)
                {
                    ProgressBar(MakingUVs);
                    var stringB = new StringBuilder("vt ", UVCap);
                    stringB.Append(uVs[i].x.ToString()).Append(" ").Append(uVs[i].y.ToString()).Append(" ");
                    streamWriter.WriteLine(stringB);
                }

                //Write Tris
                for (var i = 0; i < polys.Length; i += 3)
                {
                    ProgressBar(MakingTris);
                    var stringB = new StringBuilder("f ", TrisCap);
                    stringB.Append(polys[i] + 1).Append("/").Append(polys[i] + 1).Append(" ").Append(polys[i + 1] + 1)
                        .Append("/").Append(polys[i + 1] + 1).Append(" ").Append(polys[i + 2] + 1).Append("/")
                        .Append(polys[i + Two] + 1);
                    streamWriter.WriteLine(stringB);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Encountered error." + e);
            }

            streamWriter.Close();

            EditorUtility.DisplayProgressBar("Saving model",
                "Depending on your settings, this may take some time.", 1f);
            EditorUtility.ClearProgressBar();
        }


        private static void ProgressBar(string currentStep)
        {
            if (_counter++ == UpdateSpeed)
            {
                _counter = 0;
                EditorUtility.DisplayProgressBar("Saving New Model.", currentStep,
                    Mathf.InverseLerp(0, _maxCount, ++_currentCount));
            }
        }
    }
    #endif
}