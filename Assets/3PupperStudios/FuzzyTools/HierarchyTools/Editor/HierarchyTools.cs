using System.Linq;
using UnityEngine;
using UnityEditor;

namespace FuzzyTools
{
    [InitializeOnLoad]
    public class HierarchyTools
    {
        #region Const Strings
        
        private const string TrackerName = "InSceneTrackerForFuzzyToolsHierarchy";
        private const string EditorOnly = "EditorOnly";
        private const string ColorMode = "colorMode";
        private const string GoFontColor = "gameObjectFontColor";
        private const string PrefFontColor = "prefabOrgFontColor";
        private const string InactiveColor = "inActiveColor";
        private const string InactiveFontColor = "inActiveFontColor";
        private const string StandardFont = "standardFont";
        private const string PrefabFont = "prefebFont";
        private const string AutoInvert = "autoInvertColors";
        private const string HierarchyColor1 = "hierarchyColor1";
        private const string HierarchyColor2 = "HierarchyColor2";
        private const string HierarchyColor3 = "hierarchyColor3";
        private const string HierarchyColor4 = "hierarchyColor4";
        private const string HierarchyColor5 = "hierarchyColor5";
        private const string PrimaryColor = "primaryColor";
        private const string SecondaryColor = "secondaryColor";
        private const string UniformChange = "UniformChangeColors";
        #endregion
        private static bool _everyOther = true;
        
        /*******************************READONLY_DEFAULTS*******************************/
        public static readonly Color DefaultSkin = new Color(.76f, .76f, .76f);
        public static readonly Color DefaultProSkin = new Color(.26f,.26f,.26f);
        private static readonly Color DefaultSelected = new Color(0.24f, 0.48f, 0.90f);
        //public static readonly Color DefaultSelectedInactive = new Color(.56f,.56f,.56f);
        private static readonly Color DefaultFontColor = Color.black;
        private static readonly Color DefaultPrefabFontColor = new Color(.07f,.2f,.5f, 1);
        private const float DefaultInactiveAlpha = .6f;
        private static readonly Vector2 Offset = new Vector2(0, 2);
        /*******************************************************************************/
        public static InSceneTracker sceneTracker;
        
        
        static HierarchyTools()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGui;
            GetPreferences();
        }
    
        private static void HandleHierarchyWindowItemOnGui(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj == null) return;
    
            var fontColor = FuzzyTools.gameObjectFontColor;
            var defaultColor = EditorGUIUtility.isProSkin ? DefaultProSkin : DefaultSkin;
            var backgroundColor = defaultColor;
            var styleFont = FuzzyTools.standardFont;
            var gameObj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObj == null) return;
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
            var prefabType = PrefabUtility.GetPrefabType(obj);
#endif
#if UNITY_2018_3_OR_NEWER
            var prefabType = PrefabUtility.GetPrefabAssetType(obj);
#endif
            var offsetRect = new Rect(selectionRect.position + Offset, selectionRect.size);
            
            switch (FuzzyTools.colorMode)
            {
                case CustomColorType.Off:
                    
                    break;
                case CustomColorType.AutoColors:
                    /*******************AUTO_COLOR_MODE****************************/
                    var gameObjectFontColor = FuzzyTools.gameObjectFontColor;
                    var prefabOrgFontColor = FuzzyTools.prefabOrgFontColor;
                    var inActiveColor = FuzzyTools.inActiveColor;
                    var inActiveFontColor = FuzzyTools.inActiveFontColor;
                    var standardFont = FuzzyTools.standardFont;
                    var prefebFont = FuzzyTools.prefebFont;
                    var autoInvertColors = FuzzyTools.autoInvertColors;
                    /**************************************************************/
                    
                    if (Selection.instanceIDs.Contains(instanceID))
                    {
                        backgroundColor = DefaultSelected;
                    }
                    else if (gameObj.activeInHierarchy == false)
                    {
                        backgroundColor = inActiveColor;
                        fontColor = autoInvertColors
                            ? new Color(1 - inActiveColor.r, 1 - inActiveColor.g, 1 - inActiveColor.b, 1)
                            : inActiveFontColor;
                        if (fontColor == inActiveColor) fontColor += new Color(.25f, .25f, .25f);
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.PrefabInstance)
#endif
#if UNITY_2018_3_OR_NEWER
                    else if(prefabType == PrefabAssetType.Regular)
#endif
                    {
                        styleFont = prefebFont;
                        fontColor = gameObj.activeInHierarchy? prefabOrgFontColor: new Color(prefabOrgFontColor.r,
                            prefabOrgFontColor.g, prefabOrgFontColor.b, DefaultInactiveAlpha);
                    }
                    else
                    {
                        fontColor = gameObjectFontColor;
                        styleFont = standardFont;
                    }
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                    EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                        {
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = styleFont
                        }
                    );
                    break;
                case CustomColorType.CustomColors:

                    styleFont = FontStyle.Normal;
                    if(sceneTracker == null)
                    {
                        sceneTracker = Object.FindObjectOfType<InSceneTracker>();
                        if (sceneTracker == null)
                        {
                            var sceneTrackerObj =
                                new GameObject(TrackerName, typeof(InSceneTracker))
                                {
                                    hideFlags = HideFlags.HideInHierarchy,
                                    tag = EditorOnly
                                };

                            sceneTracker = sceneTrackerObj.GetComponent<InSceneTracker>();
                        }
                    }

                    if (Selection.instanceIDs.Contains(instanceID))
                    {
                        backgroundColor = DefaultSelected;
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.None)
#endif
#if UNITY_2018_3_OR_NEWER
                    else if(prefabType == PrefabAssetType.NotAPrefab)
#endif
                    {
                        fontColor = DefaultFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                        if (fontColor == backgroundColor) fontColor += new Color(.25f, .25f, .25f);
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.PrefabInstance)
#endif
#if UNITY_2018_3_OR_NEWER
                    else if(prefabType == PrefabAssetType.Regular)
#endif
                    {
                        fontColor = DefaultPrefabFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                    }

                    var currentObj = EditorUtility.InstanceIDToObject(instanceID);
                    var customObjs = sceneTracker.customizedObjs;
                    var customizedOptions = sceneTracker.options;
                    if (currentObj == null) break;
                    if (customObjs.Contains(currentObj))
                    {
                        var index = customObjs.IndexOf(currentObj);
                        backgroundColor = customizedOptions[index].backgroundColor;
                        fontColor = customizedOptions[index].fontColor;
                        styleFont = customizedOptions[index].style;
                    }
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                    EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                        {
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = styleFont
                        }
                    );
                    break;
                case CustomColorType.Hierarchy:
                    /****************HIERARCHY_COLOR_MODE**************************/
                    var hierarchyColor1 = FuzzyTools.hierarchyColor1;
                    var hierarchyColor2 = FuzzyTools.hierarchyColor2;
                    var hierarchyColor3 = FuzzyTools.hierarchyColor3;
                    var hierarchyColor4 = FuzzyTools.hierarchyColor4;
                    var hierarchyColor5 = FuzzyTools.hierarchyColor5;
                    /**************************************************************/
                    
                    styleFont = FontStyle.Normal;
                    if (Selection.instanceIDs.Contains(instanceID))
                    {
                        backgroundColor = DefaultSelected;
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.None)
#endif
#if UNITY_2018_3_OR_NEWER              
                    else if(prefabType == PrefabAssetType.NotAPrefab)
#endif
                    {
                        fontColor = DefaultFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                        if (fontColor == backgroundColor) fontColor += new Color(.25f, .25f, .25f);
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.PrefabInstance)
#endif
#if UNITY_2018_3_OR_NEWER
                    else if(prefabType == PrefabAssetType.Regular)
#endif
                    {
                        fontColor = DefaultPrefabFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1: DefaultInactiveAlpha;
                    }
                    if (backgroundColor != DefaultSelected)
                    {
                        var parentCount = 0.0f;
                        var parent = gameObj.transform.parent;
                        while (parent != null)
                        {
                            parentCount += .05f;
                            parent = parent.parent;
                        }
                        if (parentCount > 0 && parentCount <= .25)
                        {
                            backgroundColor = hierarchyColor1 - (hierarchyColor1 * parentCount);
                        }
                        else if (parentCount > .25f && parentCount <= .5f)
                        {
                            parentCount -= .25f;
                            backgroundColor = hierarchyColor2 - (hierarchyColor2 * (parentCount *2));
                        }
                        else if (parentCount > .5f && parentCount <= .75f)
                        {
                            parentCount -= .5f;
                            backgroundColor = hierarchyColor3 - (hierarchyColor3 *(parentCount *2));
                        }
                        else if (parentCount > .75f && parentCount <= 1f)
                        {
                            parentCount -= .75f;
                            backgroundColor = hierarchyColor4 - (hierarchyColor4 * (parentCount *2));
                        }else if (parentCount > 1f && parentCount <= 1.5f)
                        {
                            parentCount -= 1f;
                            backgroundColor = hierarchyColor5 - (hierarchyColor5 * (parentCount *2));
                        }
                    }
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                    EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                        {
    
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = styleFont
                        }
                    );
                    break;
                case CustomColorType.VariedColor:
                    /***************VARIED_COLOR_MODE*****************/
                    var primaryColor = FuzzyTools.PrimaryColor;
                    var secondaryColor = FuzzyTools.secondaryColor;
                    /*************************************************/
                    
                    styleFont = FontStyle.Normal;
                    if (Selection.instanceIDs.Contains(instanceID))
                    {
                        backgroundColor = DefaultSelected;
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.None)
#endif
#if UNITY_2018_3_OR_NEWER              
                    else if(prefabType == PrefabAssetType.NotAPrefab)
#endif
                    {
                        fontColor = DefaultFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                        if (fontColor == backgroundColor) fontColor += new Color(.25f, .25f, .25f);
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.PrefabInstance)
#endif
#if UNITY_2018_3_OR_NEWER
                    else if(prefabType == PrefabAssetType.Regular)
#endif
                    {
                        fontColor = DefaultPrefabFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                    }
                    if (backgroundColor != DefaultSelected)
                    {
                        backgroundColor = _everyOther ? primaryColor : secondaryColor;
                    }
                    _everyOther = !_everyOther;
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                    EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                        {
    
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = styleFont
                        }
                    );
                    break;
            }
        }

        private static void GetPreferences()
        {
            FuzzyTools.inActiveColor = EditorGUIUtility.isProSkin ? DefaultSkin : DefaultProSkin;
            FuzzyTools.PrimaryColor = EditorGUIUtility.isProSkin ? DefaultProSkin : DefaultSkin;
            FuzzyTools.colorMode =
                (CustomColorType) FuzzyHelper.GetEditorPrefInt(ColorMode, (int) FuzzyTools.colorMode);
            FuzzyTools.gameObjectFontColor =
                FuzzyHelper.GetEditorPrefColor(GoFontColor, FuzzyTools.gameObjectFontColor);
            FuzzyTools.prefabOrgFontColor =
                FuzzyHelper.GetEditorPrefColor(PrefFontColor, FuzzyTools.prefabOrgFontColor);
            FuzzyTools.inActiveColor = FuzzyHelper.GetEditorPrefColor(InactiveColor, FuzzyTools.inActiveColor);
            FuzzyTools.inActiveFontColor =
                FuzzyHelper.GetEditorPrefColor(InactiveFontColor, FuzzyTools.inActiveFontColor);
            FuzzyTools.standardFont =
                (FontStyle) FuzzyHelper.GetEditorPrefInt(StandardFont, (int) FuzzyTools.standardFont);
            FuzzyTools.prefebFont = (FontStyle) FuzzyHelper.GetEditorPrefInt(PrefabFont, (int)FuzzyTools.prefebFont);
            FuzzyTools.autoInvertColors =
                FuzzyHelper.GetEditorPrefBool(AutoInvert, FuzzyTools.autoInvertColors);
            FuzzyTools.hierarchyColor1 = FuzzyHelper.GetEditorPrefColor(HierarchyColor1, FuzzyTools.hierarchyColor1);
            FuzzyTools.hierarchyColor2 = FuzzyHelper.GetEditorPrefColor(HierarchyColor2, FuzzyTools.hierarchyColor2);
            FuzzyTools.hierarchyColor3 = FuzzyHelper.GetEditorPrefColor(HierarchyColor3, FuzzyTools.hierarchyColor3);
            FuzzyTools.hierarchyColor4 = FuzzyHelper.GetEditorPrefColor(HierarchyColor4, FuzzyTools.hierarchyColor4);
            FuzzyTools.hierarchyColor5 = FuzzyHelper.GetEditorPrefColor(HierarchyColor5, FuzzyTools.hierarchyColor5);
            FuzzyTools.PrimaryColor = FuzzyHelper.GetEditorPrefColor(PrimaryColor, FuzzyTools.PrimaryColor);
            FuzzyTools.secondaryColor = FuzzyHelper.GetEditorPrefColor(SecondaryColor, FuzzyTools.secondaryColor);
            FuzzyTools.uniformChangeColors =
                FuzzyHelper.GetEditorPrefBool(UniformChange, FuzzyTools.uniformChangeColors);

            sceneTracker = Object.FindObjectOfType<InSceneTracker>();
            if (sceneTracker != null && FuzzyTools.colorMode != CustomColorType.CustomColors) return;
            var sceneTrackerObj = new GameObject(TrackerName, typeof(InSceneTracker))
            {
                hideFlags = HideFlags.HideInHierarchy,
                tag = EditorOnly
            };
            
            sceneTracker = sceneTrackerObj.GetComponent<InSceneTracker>();
        }
    }
    
}