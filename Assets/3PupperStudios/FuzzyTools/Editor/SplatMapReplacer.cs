using UnityEngine;
using UnityEditor;

namespace FuzzyTools
{
    
    public class SplatMapReplacer : EditorWindow
    {
        private static Texture2D _splatMap;
        private static Texture2D _newSplat;
        private static readonly Vector2 MinSize = new Vector2(300, 187);

        #region const Strings
        private const string windowName = "SplatMap Replacer";
        private const string menuPlace = "Assets/FuzzyTools/Terrain/Replace SplatMap";
        private const string placeHere = "Please select the target and new Splat Maps";
        private const string currentSplat = "Current SplatMap";
        private const string newSplat = "New SplatMap";
        private const string splatName = "SplatAlpha";
        private const string noSplat = "No SplatMap Found";
        private const string okay = "Okay";
        private const string doSwitch = "Switch";
        private const string undoSwitch = "ReplaceSplatMap";
        private const string currentSplatInvalid =
            "The selected texture for current SplatMap does not appear to be a valid SplatMap and was automatically removed.";
        #endregion
        
        [MenuItem(menuPlace, false)]
        private static void ReplaceSplatMap()
        {
            Init();
        }

        [MenuItem(menuPlace, true)]
        private static bool ReplaceValidation()
        {
            var obj = Selection.activeObject as Texture2D;
            if (obj == null) return false;
            return obj.format == TextureFormat.ARGB32 && obj.name.Contains(splatName);
        }

        public static void Init()
        {
            _splatMap = Selection.activeObject as Texture2D;
            _newSplat = null;
            if (_splatMap != null && _splatMap.format != TextureFormat.ARGB32)
            {
                _splatMap = null;
            }

            var window = GetWindow(typeof(SplatMapReplacer), true, windowName);
            var icon = Resources.Load("FuzzyToolsIcon") as Texture2D;
            if (icon == null) return;
            window.titleContent.image = icon;
            window.minSize = MinSize;
        }

        private void OnGUI()
        {
            var splatAsObj = _newSplat as Object;
            var splatAsObj2 = _splatMap as Object;
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField(placeHere);
            EditorGUILayout.Separator();
            _splatMap = (Texture2D) EditorGUILayout.ObjectField(currentSplat, splatAsObj2, typeof(Texture2D),
                false);
            _newSplat = (Texture2D) EditorGUILayout.ObjectField(newSplat, splatAsObj, typeof(Texture2D), false);
            if (_splatMap != null)
            {
                if (!_splatMap.name.Contains(splatName) && _splatMap.format != TextureFormat.ARGB32)
                {
                    _splatMap = null;
                    EditorUtility.DisplayDialog(noSplat, currentSplatInvalid,okay );
                }
            }

            EditorGUI.BeginDisabledGroup(_splatMap == null || _newSplat == null);
            if (GUILayout.Button(doSwitch) && _splatMap != null && _newSplat != null)
            {
                ReplaceSplat();
                Close();
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void ReplaceSplat()
        {
            var path = AssetDatabase.GetAssetPath(_newSplat);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.isReadable = true;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            _newSplat = _newSplat.ScaleTexture(_splatMap.width, _splatMap.height, TextureFormat.ARGB32);

            var texture = _newSplat.GetPixels(0);

            if (texture.Length / _splatMap.width != _splatMap.height) return;
            Undo.RegisterCompleteObjectUndo(_splatMap, undoSwitch);
            _splatMap.SetPixels(texture);
            _splatMap.Apply();
            AssetDatabase.Refresh();
        }
        
    }
}