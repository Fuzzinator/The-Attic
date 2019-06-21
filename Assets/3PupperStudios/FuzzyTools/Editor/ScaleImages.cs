using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace FuzzyTools
{
    public class ResizeImages : EditorWindow
    {
        #region Consts
        private const int MinSize = 32;
        private const int MaxSize = 8192;
        private const string ImageRatio = "Image(s) Ratio";
        private const string ImageRes = "New Resolution";
        private const string ImageResY = "New Height Resolution";
        private const string ImageResX = "New Width Resolution";
        private const string ResizeBttn = "Resize Image(s)";
        //private const string MaintainRatio = "Maintain Aspect Ratio";
        private const string AddToList = "Add Selection";
        private const string ClearList = "Clear List";
        private const string SampleTexture = "Sample Texture";
        private const string Box = "box";
        private const string ScaleImages = "Scale Image(s)";
        #endregion
        
        private static readonly List<Texture2D> Images = new List<Texture2D>();

        private static int _newScale = 2048;
        private static int _xScale = 2048;
        private static int _yScale = 2048;
        private static int _chosenRes = 6;
        private static int _radioSelection = 0;
        private static int _ratioOption = 0;
        private static int _whichResize = 0;
        private static int _sampleTexCount = 0;

        private static bool _keepRatio = false;

        private static Vector2 _scrollPos = Vector2.zero;
        private static readonly Vector2 MinWindowSize = new Vector2(400, 240);

        private static readonly string[] RatioOptions = {"1:1", "Other"};
        private static readonly string[] ResizeOptions = {"Rigid Resize", "Flexible Resize"};
        private static readonly string[] ImageSizes =
        {
            "32",
            "64",
            "128",
            "256",
            "512",
            "1024",
            "2048",
            "4096",
            "8192"
        };
        private static readonly string[] SupportedFileTypes =
        {
            "PNG",
            "png",
            "JPG",
            "jpg",
            "EXR",
            "exr"
        };
        private static readonly TextureFormat[] SupportedFormats =
        {
            TextureFormat.ARGB32,
            TextureFormat.RGBA32,
            TextureFormat.RGB24,
            TextureFormat.Alpha8,
            TextureFormat.RGFloat,
            TextureFormat.RGBAFloat,
            TextureFormat.RFloat,
            TextureFormat.RGB9e5Float
        };

        [MenuItem("Assets/FuzzyTools/Scale Image(s)")]
        private static void Init()
        {
            Images.Clear();
            AddSelectionToTextures();
            _radioSelection = 0;
            _ratioOption = 0;
            _whichResize = 0;
            var window = GetWindow(typeof(ResizeImages), true, ScaleImages);
            var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
            if (icon == null) return;
            window.titleContent.image = icon;
            window.minSize = MinWindowSize;
        }


        private void OnGUI()
        {
            var xScale = 0;
            var yScale = 0;
            EditorGUILayout.BeginHorizontal(Box);
            GUILayout.Label(ImageRatio);
            _ratioOption = GUILayout.SelectionGrid(_ratioOption, RatioOptions, RatioOptions.Length);

            EditorGUILayout.EndHorizontal();
            if (_ratioOption == 0)
            {
                _radioSelection = GUILayout.SelectionGrid(_radioSelection, ResizeOptions, ResizeOptions.Length,
                    EditorStyles.radioButton);
                if (_radioSelection == 0)
                {
                    _chosenRes = EditorGUILayout.Popup(ImageRes, _chosenRes, ImageSizes);
                    int.TryParse(ImageSizes[_chosenRes], out _newScale);
                    xScale = _newScale;
                    yScale = _newScale;
                }
                else
                {
                    _newScale = EditorGUILayout.IntSlider(ImageRes, _newScale, MinSize, MaxSize);
                    xScale = _newScale;
                    yScale = _newScale;
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                if (_keepRatio && _whichResize == 0 && Images.Count > 0)
                {
                    _sampleTexCount = EditorGUILayout.Popup(SampleTexture, _sampleTexCount, StringArray(Images));
                }

                EditorGUILayout.EndHorizontal();
                if (_whichResize == 0)
                {
                    _yScale = EditorGUILayout.IntSlider(ImageResY, _yScale, MinSize, MaxSize);
                    _xScale = EditorGUILayout.IntSlider(ImageResX, _xScale, MinSize, MaxSize);
                    
                    xScale = _xScale;
                    yScale = _yScale;
                }
            }
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(AddToList))
            {
                AddSelectionToTextures();
            }

            if (GUILayout.Button(ClearList))
            {
                Images.Clear();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            if (Images.Count > 0)
            {
                EditorGUILayout.BeginVertical(Box);
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                for (var i = 0; i < Images.Count; i++)
                {
                    if (Images[i] == null)
                    {
                        Images.Remove(Images[i]);
                        i--;
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal(Box);

                    var img = Images[i];
                    var myLabel = img.name + "\n \n       " + img.height + "x" + img.width;
                    GUILayout.Label(myLabel);
                    var textDimensions = GUI.skin.label.CalcSize(new GUIContent(""));
                    EditorGUIUtility.labelWidth = textDimensions.x;

                    var texPath = AssetDatabase.GetAssetPath(img);
                    var pos = texPath.LastIndexOf(".") + 1;
                    texPath.Substring(pos, texPath.Length - pos);

                    img = (Texture2D) EditorGUILayout.ObjectField("", img, typeof(Texture2D),
                        true);
                    Images[i] = img;
                    EditorGUILayout.EndHorizontal();

                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }

            EditorGUI.BeginDisabledGroup(Images.Count == 0);
            if (GUILayout.Button(ResizeBttn))
            {
                Resize(xScale, yScale);
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void AddSelectionToTextures()
        {
            var objects = Selection.objects;
            foreach (var obj in objects)
            {
                var image = obj as Texture2D;
                if (image == null) continue;

                var texPath = AssetDatabase.GetAssetPath(obj);
                var pos = texPath.LastIndexOf(".") + 1;
                var fileType = texPath.Substring(pos, texPath.Length - pos);
                if (!SupportedFileTypes.Contains(fileType)) continue;

                Images.AddIfDoesNotContain(image);

            }
        }

        private static string[] StringArray(IEnumerable<Texture2D> input)
        {
            var output = new string[input.Count()];
            var i = 0;
            foreach (var obj in input)
            {
                output[i] = obj.name;
                i++;
            }

            return output;
        }

        /*private static void KeepRatio(bool changingY) ////TODO FINISH THIS
        {
            var newWidth = _yScale;
            var newHeight = _xScale;


            if (_sampleTex == null)
            {
                if (!changingY)
                {
                    _yScale = _xScale;
                }
                else
                {
                    _xScale = _yScale;
                }

                return;
            }

            //if(changingY)
            //{
            var aspect = _sampleTex.width / _sampleTex.height;
            if (aspect == 0) aspect = _sampleTex.height / _sampleTex.width;

            newWidth = (int) (_xScale * aspect);
            newHeight = (int) (newWidth / aspect);


            //if one of the two dimensions exceed the box dimensions
            if (newWidth > _xScale || newHeight > _xScale)
            {
                //depending on which of the two exceeds the box dimensions set it as the box dimension and calculate the other one based on the aspect ratio
                if (newWidth > newHeight)
                {
                    newWidth = _xScale;
                    newHeight = (int) (newWidth / aspect);
                }
                else
                {
                    newHeight = _xScale;
                    newWidth = (int) (newHeight * aspect);
                }
            }
            //}
            else
            {

            }

            _xScale = newWidth;
            _yScale = newHeight;

        }*/

        private static void Resize(int xScale, int yScale)
        {
            for (var i = 0; i < Images.Count; i++)
            {
                var img = Images[i];
                var texPath = AssetDatabase.GetAssetPath(img);
                var pos = texPath.LastIndexOf(".") + 1;
                var fileType = texPath.Substring(pos, texPath.Length - pos);
                if (!SupportedFileTypes.Contains(fileType)) continue;
                var imageType = (ImageType) Enum.Parse(typeof(ImageType), fileType.ToUpper());
                var texSettings = (TextureImporter) AssetImporter.GetAtPath(texPath);
                var originalSettings = texSettings;
                texSettings.isReadable = true;
                texSettings.textureCompression = TextureImporterCompression.Uncompressed;
                var textureType = texSettings.textureType;
                texSettings.textureType = TextureImporterType.Default;
                var texFormat = img.format;
                if (!SupportedFormats.Contains(texFormat)) texFormat = TextureFormat.RGBA32;

                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

                img = img.ScaleTexture(xScale, yScale, texFormat);
                Images[i] = img;
                texPath.Substring(0, texPath.LastIndexOf("/") + 1);
                var bytes = img.GetRawTextureData();

                switch (imageType)
                {
                    case ImageType.EXR:
                        var hdrFormat = new Texture2D(img.width, img.height, TextureFormat.RGBAFloat, false);
                        hdrFormat.SetPixels(img.GetPixels());
                        hdrFormat.Apply();

                        bytes = hdrFormat.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
                        DestroyImmediate(hdrFormat);
                        break;
                    case ImageType.JPG:
                        bytes = img.EncodeToJPG();
                        break;
                    case ImageType.PNG:
                        bytes = img.EncodeToPNG();
                        break;
                }

                File.WriteAllBytes(texPath, bytes);
                texSettings = originalSettings;
                texSettings.textureType = textureType;
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

            }

            Init();
        }
    }
}