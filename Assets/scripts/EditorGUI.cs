using System;

using UnityEngine;
using Object = UnityEngine.Object;

//public sealed class EditorGUILayout
//{
//    // Fields
//    internal const float kPlatformTabWidth = 30f;
//    internal static Rect s_LastRect;
//    //internal static SavedBool s_SelectedDefault = new SavedBool("Platform.ShownDefaultTab", true);

//    // Methods
//    internal static bool BeginFadeGroup(float value)
//    {
//        GUILayoutFadeGroup group = (GUILayoutFadeGroup) GUILayoutUtility.BeginLayoutGroup(GUIStyle.none, null, typeof(GUILayoutFadeGroup));
//        group.isVertical = true;
//        group.resetCoords = true;
//        group.fadeValue = value;
//        group.wasGUIEnabled = GUI.enabled;
//        group.guiColor = GUI.color;
//        if (((value != 0f) && (value != 1f)) && (Event.current.type == EventType.MouseDown))
//        {
//            Event.current.Use();
//        }
//        EditorGUIUtility.LockContextWidth();
//        GUI.BeginGroup(group.rect);
//        return (value != 0f);
//    }

//    public static Rect BeginHorizontal(params GUILayoutOption[] options)
//    {
//        return BeginHorizontal(GUIContent.none, GUIStyle.none, options);
//    }

//    public static Rect BeginHorizontal(GUIStyle style, params GUILayoutOption[] options)
//    {
//        return BeginHorizontal(GUIContent.none, style, options);
//    }

//    internal static Rect BeginHorizontal(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
//    {
//        GUILayoutUtility.BeginGroup("GUILayout.EndVertical");
//        GUILayoutGroup group = GUILayoutUtility.BeginLayoutGroup(style, options, typeof(GUILayoutGroup));
//        group.isVertical = false;
//        if ((style != GUIStyle.none) || (content != GUIContent.none))
//        {
//            GUI.Box(group.rect, GUIContent.none, style);
//        }
//        return group.rect;
//    }

//    internal static Vector2 BeginHorizontalScrollView(Vector2 scrollPosition, params GUILayoutOption[] options)
//    {
//        return BeginHorizontalScrollView(scrollPosition, false, GUI.skin.horizontalScrollbar, GUI.skin.scrollView, options);
//    }

//    internal static Vector2 BeginHorizontalScrollView(Vector2 scrollPosition, bool alwaysShowHorizontal, GUIStyle horizontalScrollbar, GUIStyle background, params GUILayoutOption[] options)
//    {
//        GUIScrollGroup group = (GUIScrollGroup) GUILayoutUtility.BeginLayoutGroup(background, null, typeof(GUIScrollGroup));
//        if (Event.current.type == EventType.Layout)
//        {
//            group.resetCoords = true;
//            group.isVertical = true;
//            group.stretchWidth = 1;
//            group.stretchHeight = 1;
//            group.verticalScrollbar = GUIStyle.none;
//            group.horizontalScrollbar = horizontalScrollbar;
//            group.allowHorizontalScroll = true;
//            group.allowVerticalScroll = false;
//            group.ApplyOptions(options);
//        }
//        return EditorGUIInternal.DoBeginScrollViewForward(group.rect, scrollPosition, new Rect(0f, 0f, group.clientWidth, group.clientHeight), alwaysShowHorizontal, false, horizontalScrollbar, GUI.skin.verticalScrollbar, background);
//    }

//    internal static int BeginPlatformGrouping(BuildPlayerWindow.BuildPlatform[] platforms, GUIContent defaultTab)
//    {
//        int num = -1;
//        for (int i = 0; i < platforms.Length; i++)
//        {
//            if (platforms[i].targetGroup == EditorUserBuildSettings.selectedBuildTargetGroup)
//            {
//                num = i;
//            }
//        }
//        if (num == -1)
//        {
//            s_SelectedDefault.value = true;
//            num = 0;
//        }
//        int index = (defaultTab != null) ? (!s_SelectedDefault.value ? num : -1) : num;
//        bool enabled = GUI.enabled;
//        GUI.enabled = true;
//        EditorGUI.BeginChangeCheck();
//        Rect rect = BeginVertical(GUI.skin.box, new GUILayoutOption[0]);
//        rect.width--;
//        int length = platforms.Length;
//        int num5 = 0x12;
//        GUIStyle toolbarButton = EditorStyles.toolbarButton;
//        if ((defaultTab != null) && GUI.Toggle(new Rect(rect.x, rect.y, rect.width - (length * 30f), (float) num5), index == -1, defaultTab, toolbarButton))
//        {
//            index = -1;
//        }
//        for (int j = 0; j < length; j++)
//        {
//            Rect rect2;
//            if (defaultTab != null)
//            {
//                rect2 = new Rect(rect.xMax - ((length - j) * 30f), rect.y, 30f, (float) num5);
//            }
//            else
//            {
//                int num7 = Mathf.RoundToInt((j * rect.width) / ((float) length));
//                int num8 = Mathf.RoundToInt(((j + 1) * rect.width) / ((float) length));
//                rect2 = new Rect(rect.x + num7, rect.y, (float) (num8 - num7), (float) num5);
//            }
//            if (GUI.Toggle(rect2, index == j, platforms[j].smallIcon, toolbarButton))
//            {
//                index = j;
//            }
//        }
//        GUILayoutUtility.GetRect(10f, (float) num5);
//        GUI.enabled = enabled;
//        if (EditorGUI.EndChangeCheck())
//        {
//            if (defaultTab == null)
//            {
//                EditorUserBuildSettings.selectedBuildTargetGroup = platforms[index].targetGroup;
//            }
//            else if (index < 0)
//            {
//                s_SelectedDefault.value = true;
//            }
//            else
//            {
//                EditorUserBuildSettings.selectedBuildTargetGroup = platforms[index].targetGroup;
//                s_SelectedDefault.value = false;
//            }
//            Object[] objArray = Resources.FindObjectsOfTypeAll(typeof(BuildPlayerWindow));
//            for (int k = 0; k < objArray.Length; k++)
//            {
//                BuildPlayerWindow window = objArray[k] as BuildPlayerWindow;
//                if (window != null)
//                {
//                    window.Repaint();
//                }
//            }
//        }
//        return index;
//    }

//    public static Vector2 BeginScrollView(Vector2 scrollPosition, GUIStyle style)
//    {
//        string name = style.name;
//        GUIStyle verticalScrollbar = GUI.skin.FindStyle(name + "VerticalScrollbar");
//        if (verticalScrollbar == null)
//        {
//            verticalScrollbar = GUI.skin.verticalScrollbar;
//        }
//        GUIStyle horizontalScrollbar = GUI.skin.FindStyle(name + "HorizontalScrollbar");
//        if (horizontalScrollbar == null)
//        {
//            horizontalScrollbar = GUI.skin.horizontalScrollbar;
//        }
//        return BeginScrollView(scrollPosition, false, false, horizontalScrollbar, verticalScrollbar, style, new GUILayoutOption[0]);
//    }

//    public static Vector2 BeginScrollView(Vector2 scrollPosition, params GUILayoutOption[] options)
//    {
//        return BeginScrollView(scrollPosition, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView, options);
//    }

//    public static Vector2 BeginScrollView(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, params GUILayoutOption[] options)
//    {
//        return BeginScrollView(scrollPosition, alwaysShowHorizontal, alwaysShowVertical, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView, options);
//    }

//    public static Vector2 BeginScrollView(Vector2 scrollPosition, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options)
//    {
//        return BeginScrollView(scrollPosition, false, false, horizontalScrollbar, verticalScrollbar, GUI.skin.scrollView, options);
//    }

//    internal static Vector2 BeginScrollView(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, params GUILayoutOption[] options)
//    {
//        return BeginScrollView(scrollPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, GUI.skin.scrollView, options);
//    }

//    public static Vector2 BeginScrollView(Vector2 scrollPosition, bool alwaysShowHorizontal, bool alwaysShowVertical, GUIStyle horizontalScrollbar, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options)
//    {
//        GUIScrollGroup group = (GUIScrollGroup) GUILayoutUtility.BeginLayoutGroup(background, null, typeof(GUIScrollGroup));
//        if (Event.current.type == EventType.Layout)
//        {
//            group.resetCoords = true;
//            group.isVertical = true;
//            group.stretchWidth = 1;
//            group.stretchHeight = 1;
//            group.verticalScrollbar = verticalScrollbar;
//            group.horizontalScrollbar = horizontalScrollbar;
//            group.ApplyOptions(options);
//        }
//        return EditorGUIInternal.DoBeginScrollViewForward(group.rect, scrollPosition, new Rect(0f, 0f, group.clientWidth, group.clientHeight), alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbar, verticalScrollbar, background);
//    }

//    public static bool BeginToggleGroup(string label, bool toggle)
//    {
//        return BeginToggleGroup(EditorGUIUtility.TempContent(label), toggle);
//    }

//    public static bool BeginToggleGroup(GUIContent label, bool toggle)
//    {
//        GUIStyle toggleGroup = EditorStyles.toggleGroup;
//        if (toggleGroup == null)
//        {
//            toggleGroup = GUI.skin.toggle;
//        }
//        toggle = GUILayout.Toggle(toggle, label, toggleGroup, new GUILayoutOption[0]);
//        EditorGUI.BeginDisabledGroup(!toggle);
//        GUILayout.BeginVertical(new GUILayoutOption[0]);
//        return toggle;
//    }

//    public static Rect BeginVertical(params GUILayoutOption[] options)
//    {
//        return BeginVertical(GUIContent.none, GUIStyle.none, options);
//    }

//    public static Rect BeginVertical(GUIStyle style, params GUILayoutOption[] options)
//    {
//        return BeginVertical(GUIContent.none, style, options);
//    }

//    internal static Rect BeginVertical(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
//    {
//        GUILayoutUtility.BeginGroup("GUILayout.EndVertical");
//        GUILayoutGroup group = GUILayoutUtility.BeginLayoutGroup(style, options, typeof(GUILayoutGroup));
//        group.isVertical = true;
//        if ((style != GUIStyle.none) || (content != GUIContent.none))
//        {
//            GUI.Box(group.rect, GUIContent.none, style);
//        }
//        return group.rect;
//    }

//    internal static Vector2 BeginVerticalScrollView(Vector2 scrollPosition, params GUILayoutOption[] options)
//    {
//        return BeginVerticalScrollView(scrollPosition, false, GUI.skin.verticalScrollbar, GUI.skin.scrollView, options);
//    }

//    internal static Vector2 BeginVerticalScrollView(Vector2 scrollPosition, bool alwaysShowVertical, GUIStyle verticalScrollbar, GUIStyle background, params GUILayoutOption[] options)
//    {
//        GUIScrollGroup group = (GUIScrollGroup) GUILayoutUtility.BeginLayoutGroup(background, null, typeof(GUIScrollGroup));
//        if (Event.current.type == EventType.Layout)
//        {
//            group.resetCoords = true;
//            group.isVertical = true;
//            group.stretchWidth = 1;
//            group.stretchHeight = 1;
//            group.verticalScrollbar = verticalScrollbar;
//            group.horizontalScrollbar = GUIStyle.none;
//            group.allowHorizontalScroll = false;
//            group.ApplyOptions(options);
//        }
//        return EditorGUIInternal.DoBeginScrollViewForward(group.rect, scrollPosition, new Rect(0f, 0f, group.clientWidth, group.clientHeight), false, alwaysShowVertical, GUI.skin.horizontalScrollbar, verticalScrollbar, background);
//    }

//    internal static bool BitToggleField(string label, SerializedProperty bitFieldProperty, int flag)
//    {
//        bool flag2 = (bitFieldProperty.intValue & flag) != 0;
//        bool flag3 = (bitFieldProperty.hasMultipleDifferentValuesBitwise & flag) != 0;
//        EditorGUI.showMixedValue = flag3;
//        EditorGUI.BeginChangeCheck();
//        flag2 = Toggle(label, flag2, new GUILayoutOption[0]);
//        if (EditorGUI.EndChangeCheck())
//        {
//            if (flag3)
//            {
//                flag2 = true;
//            }
//            flag3 = false;
//            int index = -1;
//            for (int i = 0; i < 0x20; i++)
//            {
//                if (((((int) 1) << i) & flag) != 0)
//                {
//                    index = i;
//                    break;
//                }
//            }
//            bitFieldProperty.SetBitAtIndexForAllTargetsImmediate(index, flag2);
//        }
//        EditorGUI.showMixedValue = false;
//        return (flag2 && !flag3);
//    }

//    public static Bounds BoundsField(Bounds value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 32f, EditorStyles.numberField, options);
//        return EditorGUI.BoundsField(position, value);
//    }

//    public static Bounds BoundsField(string label, Bounds value, params GUILayoutOption[] options)
//    {
//        return BoundsField(new GUIContent(label), value, options);
//    }

//    public static Bounds BoundsField(GUIContent label, Bounds value, params GUILayoutOption[] options)
//    {
//        bool hasLabel = EditorGUI.LabelHasContent(label);
//        float height = (hasLabel ? 19f : 0f) + 32f;
//        Rect position = s_LastRect = GetControlRect(hasLabel, height, EditorStyles.numberField, options);
//        return EditorGUI.BoundsField(position, label, value);
//    }

//    public static Color ColorField(Color value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.colorField, options);
//        return EditorGUI.ColorField(position, value);
//    }

//    public static Color ColorField(string label, Color value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.colorField, options);
//        return EditorGUI.ColorField(position, label, value);
//    }

//    public static Color ColorField(GUIContent label, Color value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.colorField, options);
//        return EditorGUI.ColorField(position, label, value);
//    }

//    public static AnimationCurve CurveField(AnimationCurve value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.colorField, options);
//        return EditorGUI.CurveField(position, value);
//    }

//    public static AnimationCurve CurveField(string label, AnimationCurve value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.colorField, options);
//        return EditorGUI.CurveField(position, label, value);
//    }

//    public static AnimationCurve CurveField(GUIContent label, AnimationCurve value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.colorField, options);
//        return EditorGUI.CurveField(position, label, value);
//    }

//    private static void CurveField(SerializedProperty value, Color color, Rect ranges, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.colorField, options);
//        EditorGUI.CurveField(position, value, color, ranges);
//    }

//    public static AnimationCurve CurveField(AnimationCurve value, Color color, Rect ranges, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.colorField, options);
//        return EditorGUI.CurveField(position, value, color, ranges);
//    }

//    public static AnimationCurve CurveField(string label, AnimationCurve value, Color color, Rect ranges, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.colorField, options);
//        return EditorGUI.CurveField(position, label, value, color, ranges);
//    }

//    public static AnimationCurve CurveField(GUIContent label, AnimationCurve value, Color color, Rect ranges, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.colorField, options);
//        return EditorGUI.CurveField(position, label, value, color, ranges);
//    }

//    internal static int CycleButton(int selected, GUIContent[] options, GUIStyle style)
//    {
//        if (GUILayout.Button(options[selected], style, new GUILayoutOption[0]))
//        {
//            selected++;
//            if (selected >= options.Length)
//            {
//                selected = 0;
//            }
//        }
//        return selected;
//    }

//    internal static string DelayedTextField(string value, string allowedLetters, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.textField, options);
//        return EditorGUI.DelayedTextField(position, value, allowedLetters, style);
//    }

//    internal static void EndFadeGroup()
//    {
//        GUI.EndGroup();
//        EditorGUIUtility.UnlockContextWidth();
//        GUILayoutFadeGroup topLevel = EditorGUILayoutUtilityInternal.topLevel as GUILayoutFadeGroup;
//        if (topLevel != null)
//        {
//            GUI.enabled = topLevel.wasGUIEnabled;
//            GUI.color = topLevel.guiColor;
//        }
//        GUILayoutUtility.EndGroup("GUILayout.EndVertical");
//        GUILayoutUtility.EndLayoutGroup();
//    }

//    public static void EndHorizontal()
//    {
//        GUILayout.EndHorizontal();
//    }

//    internal static void EndPlatformGrouping()
//    {
//        EndVertical();
//    }

//    public static void EndScrollView()
//    {
//        GUILayout.EndScrollView(true);
//    }

//    internal static void EndScrollView(bool handleScrollWheel)
//    {
//        GUILayout.EndScrollView(handleScrollWheel);
//    }

//    public static void EndToggleGroup()
//    {
//        GUILayout.EndVertical();
//        EditorGUI.EndDisabledGroup();
//    }

//    public static void EndVertical()
//    {
//        GUILayout.EndVertical();
//    }

//    public static Enum EnumMaskField(Enum enumValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.popup, options);
//        return EditorGUI.EnumMaskField(position, enumValue, EditorStyles.popup);
//    }

//    public static Enum EnumMaskField(Enum enumValue, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.EnumMaskField(position, enumValue, style);
//    }

//    public static Enum EnumMaskField(string label, Enum enumValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.popup, options);
//        return EditorGUI.EnumMaskField(position, label, enumValue, EditorStyles.popup);
//    }

//    public static Enum EnumMaskField(GUIContent label, Enum enumValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.popup, options);
//        return EditorGUI.EnumMaskField(position, label, enumValue, EditorStyles.popup);
//    }

//    public static Enum EnumMaskField(string label, Enum enumValue, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.EnumMaskField(position, label, enumValue, style);
//    }

//    public static Enum EnumMaskField(GUIContent label, Enum enumValue, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.EnumMaskField(position, label, enumValue, style);
//    }

//    public static Enum EnumPopup(Enum selected, params GUILayoutOption[] options)
//    {
//        return EnumPopup(selected, EditorStyles.popup, options);
//    }

//    public static Enum EnumPopup(Enum selected, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.EnumPopup(position, selected, style);
//    }

//    public static Enum EnumPopup(string label, Enum selected, params GUILayoutOption[] options)
//    {
//        return EnumPopup(label, selected, EditorStyles.popup, options);
//    }

//    public static Enum EnumPopup(GUIContent label, Enum selected, params GUILayoutOption[] options)
//    {
//        return EnumPopup(label, selected, EditorStyles.popup, options);
//    }

//    public static Enum EnumPopup(string label, Enum selected, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.EnumPopup(position, label, selected, style);
//    }

//    public static Enum EnumPopup(GUIContent label, Enum selected, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.EnumPopup(position, label, selected, style);
//    }

//    public static float FloatField(float value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.numberField, options);
//        return EditorGUI.FloatField(position, value);
//    }

//    public static float FloatField(float value, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.FloatField(position, value, style);
//    }

//    public static float FloatField(string label, float value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.numberField, options);
//        return EditorGUI.FloatField(position, label, value);
//    }

//    public static float FloatField(GUIContent label, float value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.numberField, options);
//        return EditorGUI.FloatField(position, label, value);
//    }

//    public static float FloatField(string label, float value, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.FloatField(position, label, value, style);
//    }

//    public static float FloatField(GUIContent label, float value, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.FloatField(position, label, value, style);
//    }

//    public static bool Foldout(bool foldout, string content)
//    {
//        GUIStyle style = EditorStyles.foldout;
//        return Foldout(foldout, content, style);
//    }

//    public static bool Foldout(bool foldout, GUIContent content)
//    {
//        GUIStyle style = EditorStyles.foldout;
//        return Foldout(foldout, content, style);
//    }

//    public static bool Foldout(bool foldout, string content, GUIStyle style)
//    {
//        return Foldout(foldout, EditorGUIUtility.TempContent(content), style);
//    }

//    public static bool Foldout(bool foldout, GUIContent content, GUIStyle style)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(EditorGUI.kNumberW, EditorGUI.kNumberW, 16f, 16f, style);
//        return EditorGUI.Foldout(position, foldout, content, style);
//    }

//    internal static void GameViewSizePopup(GameViewSizeGroupType groupType, int selectedIndex, Action<int, object> itemClickedCallback, GUIStyle style, params GUILayoutOption[] options)
//    {
//        EditorGUI.GameViewSizePopup(GetControlRect(false, 16f, style, options), groupType, selectedIndex, itemClickedCallback, style);
//    }

//    internal static Rect GetControlRect(params GUILayoutOption[] options)
//    {
//        return GetControlRect(true, 16f, EditorStyles.layerMaskField, options);
//    }

//    internal static Rect GetControlRect(bool hasLabel, params GUILayoutOption[] options)
//    {
//        return GetControlRect(hasLabel, 16f, EditorStyles.layerMaskField, options);
//    }

//    internal static Rect GetControlRect(bool hasLabel, float height, GUIStyle style, params GUILayoutOption[] options)
//    {
//        return GUILayoutUtility.GetRect(!hasLabel ? EditorGUI.kNumberW : kLabelFloatMinW, kLabelFloatMaxW, height, height, style, options);
//    }

//    internal static Rect GetSliderRect(bool hasLabel, params GUILayoutOption[] options)
//    {
//        return GUILayoutUtility.GetRect(!hasLabel ? EditorGUI.kNumberW : kLabelFloatMinW, (kLabelFloatMaxW + 5f) + 100f, 16f, 16f, EditorStyles.numberField, options);
//    }

//    internal static Rect GetToggleRect(bool hasLabel, params GUILayoutOption[] options)
//    {
//        float num = 10f - EditorGUI.kNumberW;
//        return GUILayoutUtility.GetRect(!hasLabel ? (EditorGUI.kNumberW + num) : (kLabelFloatMinW + num), kLabelFloatMaxW + num, 16f, 16f, EditorStyles.numberField, options);
//    }

//    internal static Gradient GradientField(SerializedProperty value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(kLabelFloatMinW, kLabelFloatMaxW, 16f, 16f, EditorStyles.colorField, options);
//        return EditorGUI.GradientField(position, value);
//    }

//    internal static Gradient GradientField(Gradient value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(EditorGUI.kNumberW, kLabelFloatMaxW, 16f, 16f, EditorStyles.colorField, options);
//        return EditorGUI.GradientField(position, value);
//    }

//    internal static Gradient GradientField(string label, SerializedProperty value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(kLabelFloatMinW, kLabelFloatMaxW, 16f, 16f, EditorStyles.colorField, options);
//        return EditorGUI.GradientField(label, position, value);
//    }

//    internal static Gradient GradientField(string label, Gradient value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(kLabelFloatMinW, kLabelFloatMaxW, 16f, 16f, EditorStyles.colorField, options);
//        return EditorGUI.GradientField(label, position, value);
//    }

//    internal static Gradient GradientField(GUIContent label, SerializedProperty value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(kLabelFloatMinW, kLabelFloatMaxW, 16f, 16f, EditorStyles.colorField, options);
//        return EditorGUI.GradientField(label, position, value);
//    }

//    internal static Gradient GradientField(GUIContent label, Gradient value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(kLabelFloatMinW, kLabelFloatMaxW, 16f, 16f, EditorStyles.colorField, options);
//        return EditorGUI.GradientField(label, position, value);
//    }

//    public static void HelpBox(string message, MessageType type)
//    {
//        LabelField(GUIContent.none, EditorGUIUtility.TempContent(message, EditorGUIUtility.GetHelpIcon(type)), EditorStyles.helpBox, new GUILayoutOption[0]);
//    }

//    public static void HelpBox(string message, MessageType type, bool wide)
//    {
//        LabelField(!wide ? EditorGUIUtility.blankContent : GUIContent.none, EditorGUIUtility.TempContent(message, EditorGUIUtility.GetHelpIcon(type)), EditorStyles.helpBox, new GUILayoutOption[0]);
//    }

//    internal static bool IconButton(int id, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
//    {
//        return EditorGUI.IconButton(id, GUILayoutUtility.GetRect(content, style, options), content, style);
//    }

//    public static bool InspectorTitlebar(bool foldout, Object targetObj)
//    {
//        return EditorGUI.InspectorTitlebar(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.inspectorTitlebar), foldout, targetObj);
//    }

//    public static bool InspectorTitlebar(bool foldout, Object[] targetObjs)
//    {
//        return EditorGUI.InspectorTitlebar(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.inspectorTitlebar), foldout, targetObjs);
//    }

//    public static int IntField(int value, params GUILayoutOption[] options)
//    {
//        return IntField(value, EditorStyles.numberField, options);
//    }

//    public static int IntField(int value, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.IntField(position, value, style);
//    }

//    public static int IntField(string label, int value, params GUILayoutOption[] options)
//    {
//        return IntField(label, value, EditorStyles.numberField, options);
//    }

//    public static int IntField(GUIContent label, int value, params GUILayoutOption[] options)
//    {
//        return IntField(label, value, EditorStyles.numberField, options);
//    }

//    public static int IntField(string label, int value, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.IntField(position, label, value, style);
//    }

//    public static int IntField(GUIContent label, int value, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.IntField(position, label, value, style);
//    }

//    public static int IntPopup(int selectedValue, string[] displayedOptions, int[] optionValues, params GUILayoutOption[] options)
//    {
//        return IntPopup(selectedValue, displayedOptions, optionValues, EditorStyles.popup, options);
//    }

//    public static int IntPopup(int selectedValue, GUIContent[] displayedOptions, int[] optionValues, params GUILayoutOption[] options)
//    {
//        return IntPopup(selectedValue, displayedOptions, optionValues, EditorStyles.popup, options);
//    }

//    public static void IntPopup(SerializedProperty property, GUIContent[] displayedOptions, int[] optionValues, params GUILayoutOption[] options)
//    {
//        IntPopup(property, displayedOptions, optionValues, null, options);
//    }

//    public static int IntPopup(int selectedValue, string[] displayedOptions, int[] optionValues, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.IntPopup(position, selectedValue, displayedOptions, optionValues, style);
//    }

//    public static int IntPopup(int selectedValue, GUIContent[] displayedOptions, int[] optionValues, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.IntPopup(position, selectedValue, displayedOptions, optionValues, style);
//    }

//    public static int IntPopup(string label, int selectedValue, string[] displayedOptions, int[] optionValues, params GUILayoutOption[] options)
//    {
//        return IntPopup(label, selectedValue, displayedOptions, optionValues, EditorStyles.popup, options);
//    }

//    public static void IntPopup(SerializedProperty property, GUIContent[] displayedOptions, int[] optionValues, GUIContent label, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.popup, options);
//        EditorGUI.IntPopup(position, property, displayedOptions, optionValues, label);
//    }

//    public static int IntPopup(GUIContent label, int selectedValue, GUIContent[] displayedOptions, int[] optionValues, params GUILayoutOption[] options)
//    {
//        return IntPopup(label, selectedValue, displayedOptions, optionValues, EditorStyles.popup, options);
//    }

//    public static int IntPopup(string label, int selectedValue, string[] displayedOptions, int[] optionValues, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.IntPopup(position, label, selectedValue, displayedOptions, optionValues, style);
//    }

//    [Obsolete("This function is obsolete and the style is not used.")]
//    public static void IntPopup(SerializedProperty property, GUIContent[] displayedOptions, int[] optionValues, GUIContent label, GUIStyle style, params GUILayoutOption[] options)
//    {
//        IntPopup(property, displayedOptions, optionValues, label, options);
//    }

//    public static int IntPopup(GUIContent label, int selectedValue, GUIContent[] displayedOptions, int[] optionValues, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.IntPopup(position, label, selectedValue, displayedOptions, optionValues, style);
//    }

//    public static int IntSlider(int value, int leftValue, int rightValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(false, options);
//        return EditorGUI.IntSlider(position, value, leftValue, rightValue);
//    }

//    public static void IntSlider(SerializedProperty property, int leftValue, int rightValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(false, options);
//        EditorGUI.IntSlider(position, property, leftValue, rightValue, property.displayName);
//    }

//    public static int IntSlider(string label, int value, int leftValue, int rightValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(true, options);
//        return EditorGUI.IntSlider(position, label, value, leftValue, rightValue);
//    }

//    public static void IntSlider(SerializedProperty property, int leftValue, int rightValue, string label, params GUILayoutOption[] options)
//    {
//        IntSlider(property, leftValue, rightValue, EditorGUIUtility.TempContent(label), options);
//    }

//    public static void IntSlider(SerializedProperty property, int leftValue, int rightValue, GUIContent label, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(true, options);
//        EditorGUI.IntSlider(position, property, leftValue, rightValue, label);
//    }

//    public static int IntSlider(GUIContent label, int value, int leftValue, int rightValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(true, options);
//        return EditorGUI.IntSlider(position, label, value, leftValue, rightValue);
//    }

//    internal static Event KeyEventField(Event e, params GUILayoutOption[] options)
//    {
//        return EditorGUI.KeyEventField(GUILayoutUtility.GetRect(EditorGUIUtility.TempContent("[Please press a key]"), GUI.skin.textField, options), e);
//    }

//    public static void LabelField(string label, params GUILayoutOption[] options)
//    {
//        LabelField(GUIContent.none, EditorGUIUtility.TempContent(label), EditorStyles.label, options);
//    }

//    public static void LabelField(GUIContent label, params GUILayoutOption[] options)
//    {
//        LabelField(GUIContent.none, label, EditorStyles.label, options);
//    }

//    public static void LabelField(string label, string label2, params GUILayoutOption[] options)
//    {
//        LabelField(new GUIContent(label), EditorGUIUtility.TempContent(label2), EditorStyles.label, options);
//    }

//    public static void LabelField(string label, GUIStyle style, params GUILayoutOption[] options)
//    {
//        LabelField(GUIContent.none, EditorGUIUtility.TempContent(label), style, options);
//    }

//    public static void LabelField(GUIContent label, GUIContent label2, params GUILayoutOption[] options)
//    {
//        LabelField(label, label2, EditorStyles.label, options);
//    }

//    public static void LabelField(GUIContent label, GUIStyle style, params GUILayoutOption[] options)
//    {
//        LabelField(GUIContent.none, label, style, options);
//    }

//    public static void LabelField(string label, string label2, GUIStyle style, params GUILayoutOption[] options)
//    {
//        LabelField(new GUIContent(label), EditorGUIUtility.TempContent(label2), style, options);
//    }

//    public static void LabelField(GUIContent label, GUIContent label2, GUIStyle style, params GUILayoutOption[] options)
//    {
//        if (!style.wordWrap)
//        {
//            Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.textField, options);
//            EditorGUI.LabelField(position, label, label2, style);
//        }
//        else
//        {
//            BeginHorizontal(new GUILayoutOption[0]);
//            PrefixLabel(label, style);
//            Rect rect2 = GUILayoutUtility.GetRect(label2, style, options);
//            int indentLevel = EditorGUI.indentLevel;
//            EditorGUI.indentLevel = 0;
//            EditorGUI.LabelField(rect2, label2, style);
//            EditorGUI.indentLevel = indentLevel;
//            EndHorizontal();
//        }
//    }

//    public static int LayerField(int layer, params GUILayoutOption[] options)
//    {
//        return LayerField(layer, EditorStyles.popup, options);
//    }

//    public static int LayerField(int layer, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.LayerField(position, layer, style);
//    }

//    public static int LayerField(string label, int layer, params GUILayoutOption[] options)
//    {
//        return LayerField(EditorGUIUtility.TempContent(label), layer, EditorStyles.popup, options);
//    }

//    public static int LayerField(GUIContent label, int layer, params GUILayoutOption[] options)
//    {
//        return LayerField(label, layer, EditorStyles.popup, options);
//    }

//    public static int LayerField(string label, int layer, GUIStyle style, params GUILayoutOption[] options)
//    {
//        return LayerField(EditorGUIUtility.TempContent(label), layer, style, options);
//    }

//    public static int LayerField(GUIContent label, int layer, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.LayerField(position, label, layer, style);
//    }

//    internal static void LayerMaskField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.layerMaskField, options);
//        EditorGUI.LayerMaskField(position, property, label);
//    }

//    public static int MaskField(int mask, string[] displayedOptions, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.popup, options);
//        return EditorGUI.MaskField(position, mask, displayedOptions, EditorStyles.popup);
//    }

//    public static int MaskField(int mask, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.MaskField(position, mask, displayedOptions, style);
//    }

//    public static int MaskField(string label, int mask, string[] displayedOptions, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.popup, options);
//        return EditorGUI.MaskField(position, label, mask, displayedOptions, EditorStyles.popup);
//    }

//    public static int MaskField(GUIContent label, int mask, string[] displayedOptions, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.popup, options);
//        return EditorGUI.MaskField(position, label, mask, displayedOptions, EditorStyles.popup);
//    }

//    public static int MaskField(string label, int mask, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.MaskField(position, label, mask, displayedOptions, style);
//    }

//    public static int MaskField(GUIContent label, int mask, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.MaskField(position, label, mask, displayedOptions, style);
//    }

//    public static void MinMaxSlider(ref float minValue, ref float maxValue, float minLimit, float maxLimit, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(false, options);
//        EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, minLimit, maxLimit);
//    }

//    public static void MinMaxSlider(GUIContent label, ref float minValue, ref float maxValue, float minLimit, float maxLimit, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(true, options);
//        EditorGUI.MinMaxSlider(label, position, ref minValue, ref maxValue, minLimit, maxLimit);
//    }

//    internal static void MultiSelectionObjectTitleBar(Object[] objects)
//    {
//        string t = objects[0].name + " (" + ObjectNames.NicifyVariableName(ObjectNames.GetTypeName(objects[0])) + ")";
//        if (objects.Length > 1)
//        {
//            string str2 = t;
//            object[] objArray1 = new object[] { str2, " and ", objects.Length - 1, " other", (objects.Length <= 2) ? string.Empty : "s" };
//            t = string.Concat(objArray1);
//        }
//        GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Height(16f) };
//        GUILayout.Label(EditorGUIUtility.TempContent(t, AssetPreview.GetMiniThumbnail(objects[0])), EditorStyles.boldLabel, options);
//    }

//    [Obsolete("Check the docs for the usage of the new parameter 'allowSceneObjects'.")]
//    public static Object ObjectField(Object obj, Type objType, params GUILayoutOption[] options)
//    {
//        return ObjectField(obj, objType, true, options);
//    }

//    [Obsolete("Check the docs for the usage of the new parameter 'allowSceneObjects'.")]
//    public static Object ObjectField(string label, Object obj, Type objType, params GUILayoutOption[] options)
//    {
//        return ObjectField(label, obj, objType, true, options);
//    }

//    [Obsolete("Check the docs for the usage of the new parameter 'allowSceneObjects'.")]
//    public static Object ObjectField(GUIContent label, Object obj, Type objType, params GUILayoutOption[] options)
//    {
//        return ObjectField(label, obj, objType, true, options);
//    }

//    public static Object ObjectField(Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.numberField, options);
//        return EditorGUI.ObjectField(position, obj, objType, allowSceneObjects);
//    }

//    public static Object ObjectField(string label, Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
//    {
//        GUIStyle objectField = EditorStyles.objectField;
//        if (!EditorGUIUtility.lookLikeInspector && EditorGUIUtility.HasObjectThumbnail(objType))
//        {
//            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
//            PrefixLabel(label, objectField);
//            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(64f) };
//            Rect rect = s_LastRect = GUILayoutUtility.GetAspectRect(1f, objectField, optionArray1);
//            Object obj2 = EditorGUI.ObjectField(rect, obj, objType, allowSceneObjects);
//            GUILayout.EndHorizontal();
//            return obj2;
//        }
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.numberField, options);
//        return EditorGUI.ObjectField(position, label, obj, objType, allowSceneObjects);
//    }

//    public static Object ObjectField(GUIContent label, Object obj, Type objType, bool allowSceneObjects, params GUILayoutOption[] options)
//    {
//        GUIStyle objectField = EditorStyles.objectField;
//        if (!EditorGUIUtility.lookLikeInspector && EditorGUIUtility.HasObjectThumbnail(objType))
//        {
//            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
//            PrefixLabel(label, objectField);
//            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(64f) };
//            Rect rect = s_LastRect = GUILayoutUtility.GetAspectRect(1f, objectField, optionArray1);
//            Object obj2 = EditorGUI.ObjectField(rect, obj, objType, allowSceneObjects);
//            GUILayout.EndHorizontal();
//            return obj2;
//        }
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.numberField, options);
//        return EditorGUI.ObjectField(position, label, obj, objType, allowSceneObjects);
//    }

//    internal static void ObjectReferenceField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.objectField, options);
//        EditorGUI.ObjectReferenceField(position, property, label);
//    }

//    public static string PasswordField(string password, params GUILayoutOption[] options)
//    {
//        return PasswordField(password, EditorStyles.textField, options);
//    }

//    public static string PasswordField(string label, string password, params GUILayoutOption[] options)
//    {
//        return PasswordField(EditorGUIUtility.TempContent(label), password, EditorStyles.textField, options);
//    }

//    public static string PasswordField(string password, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.PasswordField(position, password, style);
//    }

//    public static string PasswordField(GUIContent label, string password, params GUILayoutOption[] options)
//    {
//        return PasswordField(label, password, EditorStyles.textField, options);
//    }

//    public static string PasswordField(string label, string password, GUIStyle style, params GUILayoutOption[] options)
//    {
//        return PasswordField(EditorGUIUtility.TempContent(label), password, style, options);
//    }

//    public static string PasswordField(GUIContent label, string password, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.PasswordField(position, label, password, style);
//    }

//    public static int Popup(int selectedIndex, string[] displayedOptions, params GUILayoutOption[] options)
//    {
//        return Popup(selectedIndex, displayedOptions, EditorStyles.popup, options);
//    }

//    public static int Popup(int selectedIndex, GUIContent[] displayedOptions, params GUILayoutOption[] options)
//    {
//        return Popup(selectedIndex, displayedOptions, EditorStyles.popup, options);
//    }

//    internal static void Popup(SerializedProperty property, GUIContent[] displayedOptions, params GUILayoutOption[] options)
//    {
//        Popup(property, displayedOptions, null, options);
//    }

//    public static int Popup(int selectedIndex, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.Popup(position, selectedIndex, displayedOptions, style);
//    }

//    public static int Popup(int selectedIndex, GUIContent[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.Popup(position, selectedIndex, displayedOptions, style);
//    }

//    public static int Popup(string label, int selectedIndex, string[] displayedOptions, params GUILayoutOption[] options)
//    {
//        return Popup(label, selectedIndex, displayedOptions, EditorStyles.popup, options);
//    }

//    internal static void Popup(SerializedProperty property, GUIContent[] displayedOptions, GUIContent label, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.popup, options);
//        EditorGUI.Popup(position, property, displayedOptions, label);
//    }

//    public static int Popup(GUIContent label, int selectedIndex, GUIContent[] displayedOptions, params GUILayoutOption[] options)
//    {
//        return Popup(label, selectedIndex, displayedOptions, EditorStyles.popup, options);
//    }

//    public static int Popup(string label, int selectedIndex, string[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.Popup(position, label, selectedIndex, displayedOptions, style);
//    }

//    public static int Popup(GUIContent label, int selectedIndex, GUIContent[] displayedOptions, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.Popup(position, label, selectedIndex, displayedOptions, style);
//    }

//    internal static float PowerSlider(float sliderValue, float min, float max, float power, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(false, options);
//        return EditorGUI.PowerSlider(position, sliderValue, min, max, power);
//    }

//    public static void PrefixLabel(string label)
//    {
//        GUIStyle followingStyle = "Button";
//        PrefixLabel(label, followingStyle);
//    }

//    public static void PrefixLabel(GUIContent label)
//    {
//        GUIStyle followingStyle = "Button";
//        PrefixLabel(label, followingStyle);
//    }

//    public static void PrefixLabel(string label, GUIStyle followingStyle)
//    {
//        PrefixLabel(EditorGUIUtility.TempContent(label), followingStyle, EditorStyles.label);
//    }

//    public static void PrefixLabel(GUIContent label, GUIStyle followingStyle)
//    {
//        PrefixLabel(label, followingStyle, EditorStyles.label);
//    }

//    public static void PrefixLabel(string label, GUIStyle followingStyle, GUIStyle labelStyle)
//    {
//        PrefixLabel(EditorGUIUtility.TempContent(label), followingStyle, labelStyle);
//    }

//    public static void PrefixLabel(GUIContent label, GUIStyle followingStyle, GUIStyle labelStyle)
//    {
//        float left = followingStyle.margin.left;
//        if (!EditorGUI.LabelHasContent(label))
//        {
//            GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.ExpandWidth(false) };
//            GUILayoutUtility.GetRect(EditorGUI.indent - left, 16f, followingStyle, options);
//        }
//        else
//        {
//            GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.ExpandWidth(false) };
//            Rect position = GUILayoutUtility.GetRect(EditorGUIUtility.labelWidth - left, 16f, followingStyle, optionArray2);
//            if (Event.current.type == EventType.Repaint)
//            {
//                position.x += EditorGUI.indent;
//                position.width += left - EditorGUI.indent;
//                labelStyle.Draw(position, label, 0);
//            }
//        }
//    }

//    public static bool PropertyField(SerializedProperty property, params GUILayoutOption[] options)
//    {
//        return PropertyField(property, null, false, options);
//    }

//    public static bool PropertyField(SerializedProperty property, bool includeChildren, params GUILayoutOption[] options)
//    {
//        return PropertyField(property, null, includeChildren, options);
//    }

//    public static bool PropertyField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options)
//    {
//        return PropertyField(property, label, false, options);
//    }

//    public static bool PropertyField(SerializedProperty property, GUIContent label, bool includeChildren, params GUILayoutOption[] options)
//    {
//        // This item is obfuscated and can not be translated.
//        Rect toggleRect;
//        PropertyDrawer drawer = PropertyDrawer.GetDrawer(property);
//        if (drawer != null)
//        {
//            if (label != null)
//            {
//                goto Label_002C;
//            }
//            toggleRect = GUILayoutUtility.GetRect(0f, drawer.GetPropertyHeight(property.Copy(), EditorGUIUtility.TempContent(property.displayName)));
//        }
//        else if (property.propertyType == SerializedPropertyType.Boolean)
//        {
//            toggleRect = GetToggleRect(true, options);
//        }
//        else
//        {
//            toggleRect = GetControlRect(true, EditorGUI.GetPropertyHeight(property, label, includeChildren), EditorStyles.layerMaskField, options);
//        }
//        s_LastRect = toggleRect;
//        return EditorGUI.PropertyField(toggleRect, property, label, includeChildren);
//    }

//    public static Rect RectField(Rect value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 32f, EditorStyles.numberField, options);
//        return EditorGUI.RectField(position, value);
//    }

//    public static Rect RectField(string label, Rect value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 51f, EditorStyles.numberField, options);
//        return EditorGUI.RectField(position, label, value);
//    }

//    public static Rect RectField(GUIContent label, Rect value, params GUILayoutOption[] options)
//    {
//        bool hasLabel = EditorGUI.LabelHasContent(label);
//        float height = (hasLabel ? 19f : 0f) + 32f;
//        Rect position = s_LastRect = GetControlRect(hasLabel, height, EditorStyles.numberField, options);
//        return EditorGUI.RectField(position, label, value);
//    }

//    public static void SelectableLabel(string text, params GUILayoutOption[] options)
//    {
//        SelectableLabel(text, EditorStyles.label, options);
//    }

//    public static void SelectableLabel(string text, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 32f, style, options);
//        EditorGUI.SelectableLabel(position, text, style);
//    }

//    public static void Separator()
//    {
//        Space();
//    }

//    public static float Slider(float value, float leftValue, float rightValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(false, options);
//        return EditorGUI.Slider(position, value, leftValue, rightValue);
//    }

//    public static void Slider(SerializedProperty property, float leftValue, float rightValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(false, options);
//        EditorGUI.Slider(position, property, leftValue, rightValue);
//    }

//    public static float Slider(string label, float value, float leftValue, float rightValue, params GUILayoutOption[] options)
//    {
//        return Slider(EditorGUIUtility.TempContent(label), value, leftValue, rightValue, options);
//    }

//    public static void Slider(SerializedProperty property, float leftValue, float rightValue, string label, params GUILayoutOption[] options)
//    {
//        Slider(property, leftValue, rightValue, EditorGUIUtility.TempContent(label), options);
//    }

//    public static void Slider(SerializedProperty property, float leftValue, float rightValue, GUIContent label, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(true, options);
//        EditorGUI.Slider(position, property, leftValue, rightValue, label);
//    }

//    public static float Slider(GUIContent label, float value, float leftValue, float rightValue, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetSliderRect(true, options);
//        return EditorGUI.Slider(position, label, value, leftValue, rightValue);
//    }

//    public static void Space()
//    {
//        if (EditorGUIUtility.lookLikeInspector)
//        {
//            GUILayoutUtility.GetRect((float) 10f, (float) 10f);
//        }
//        else
//        {
//            GUILayoutUtility.GetRect((float) 6f, (float) 6f);
//        }
//    }

//    public static string TagField(string tag, params GUILayoutOption[] options)
//    {
//        return TagField(tag, EditorStyles.popup, options);
//    }

//    public static string TagField(string label, string tag, params GUILayoutOption[] options)
//    {
//        return TagField(EditorGUIUtility.TempContent(label), tag, EditorStyles.popup, options);
//    }

//    public static string TagField(string tag, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.TagField(position, tag, style);
//    }

//    public static string TagField(GUIContent label, string tag, params GUILayoutOption[] options)
//    {
//        return TagField(label, tag, EditorStyles.popup, options);
//    }

//    public static string TagField(string label, string tag, GUIStyle style, params GUILayoutOption[] options)
//    {
//        return TagField(EditorGUIUtility.TempContent(label), tag, style, options);
//    }

//    public static string TagField(GUIContent label, string tag, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.TagField(position, label, tag, style);
//    }

//    internal static void TargetChoiceField(SerializedProperty property, GUIContent label, params GUILayoutOption[] options)
//    {
//        TargetChoiceField(property, label, new TargetChoiceHandler.TargetChoiceMenuFunction(TargetChoiceHandler.SetToValueOfTarget), options);
//    }

//    internal static void TargetChoiceField(SerializedProperty property, GUIContent label, TargetChoiceHandler.TargetChoiceMenuFunction func, params GUILayoutOption[] options)
//    {
//        EditorGUI.TargetChoiceField(GUILayoutUtility.GetRect(EditorGUI.kNumberW, kLabelFloatMaxW, 16f, 16f, EditorStyles.popup, options), property, label, func);
//    }

//    public static string TextArea(string text, params GUILayoutOption[] options)
//    {
//        return TextArea(text, EditorStyles.textField, options);
//    }

//    public static string TextArea(string text, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(EditorGUIUtility.TempContent(text), style, options);
//        return EditorGUI.TextArea(position, text, style);
//    }

//    public static string TextField(string text, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.textField, options);
//        return EditorGUI.TextField(position, text);
//    }

//    public static string TextField(string label, string text, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.textField, options);
//        return EditorGUI.TextField(position, label, text);
//    }

//    public static string TextField(string text, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, style, options);
//        return EditorGUI.TextField(position, text, style);
//    }

//    public static string TextField(GUIContent label, string text, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, EditorStyles.textField, options);
//        return EditorGUI.TextField(position, label, text);
//    }

//    public static string TextField(string label, string text, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.TextField(position, label, text, style);
//    }

//    public static string TextField(GUIContent label, string text, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 16f, style, options);
//        return EditorGUI.TextField(position, label, text, style);
//    }

//    public static bool Toggle(bool value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetToggleRect(false, options);
//        return EditorGUI.Toggle(position, value);
//    }

//    public static bool Toggle(bool value, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetToggleRect(false, options);
//        return EditorGUI.Toggle(position, value, style);
//    }

//    public static bool Toggle(string label, bool value, params GUILayoutOption[] options)
//    {
//        return Toggle(EditorGUIUtility.TempContent(label), value, options);
//    }

//    public static bool Toggle(GUIContent label, bool value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetToggleRect(true, options);
//        return EditorGUI.Toggle(position, label, value);
//    }

//    public static bool Toggle(string label, bool value, GUIStyle style, params GUILayoutOption[] options)
//    {
//        return Toggle(EditorGUIUtility.TempContent(label), value, style, options);
//    }

//    public static bool Toggle(GUIContent label, bool value, GUIStyle style, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetToggleRect(true, options);
//        return EditorGUI.Toggle(position, label, value, style);
//    }

//    internal static string ToolbarSearchField(string text, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(0f, kLabelFloatMaxW * 1.5f, 16f, 16f, EditorStyles.toolbarSearchField, options);
//        int searchMode = 0;
//        return EditorGUI.ToolbarSearchField(position, null, ref searchMode, text);
//    }

//    internal static string ToolbarSearchField(string text, string[] searchModes, ref int searchMode, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GUILayoutUtility.GetRect(0f, kLabelFloatMaxW * 1.5f, 16f, 16f, EditorStyles.toolbarSearchField, options);
//        return EditorGUI.ToolbarSearchField(position, searchModes, ref searchMode, text);
//    }

//    public static Vector2 Vector2Field(string label, Vector2 value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 35f, EditorStyles.numberField, options);
//        return EditorGUI.Vector2Field(position, label, value);
//    }

//    public static Vector3 Vector3Field(string label, Vector3 value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 35f, EditorStyles.numberField, options);
//        return EditorGUI.Vector3Field(position, label, value);
//    }

//    public static Vector4 Vector4Field(string label, Vector4 value, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(true, 35f, EditorStyles.numberField, options);
//        return EditorGUI.Vector4Field(position, label, value);
//    }

//    internal static void VUMeterHorizontal(float value, ref EditorGUI.VUMeterData data, params GUILayoutOption[] options)
//    {
//        Rect position = s_LastRect = GetControlRect(false, 16f, EditorStyles.numberField, options);
//        EditorGUI.VUMeterHorizontal(position, value, ref data);
//    }

//    // Properties
//    internal static float kLabelFloatMaxW
//    {
//        get
//        {
//            return ((EditorGUIUtility.labelWidth + EditorGUI.kNumberW) + 5f);
//        }
//    }

//    internal static float kLabelFloatMinW
//    {
//        get
//        {
//            return ((EditorGUIUtility.labelWidth + EditorGUI.kNumberW) + 5f);
//        }
//    }
//}

 
 



