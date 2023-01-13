using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace FluidFlow
{
    public static class EditorUtil
    {
        public static readonly float ListElementPadding = 2;
        public static readonly float ListElementHeight = EditorGUIUtility.singleLineHeight + ListElementPadding * 2;

        public static bool IsModel(UnityEngine.Object obj)
        {
            return PrefabUtility.GetPrefabAssetType(obj) == PrefabAssetType.Model;
        }

        public static IEnumerable<string> EnumerateUniqueTexturePropertyNames(List<RenderTargetDescriptor> descriptors)
        {
            var unique = new HashSet<string>();
            foreach (var target in descriptors) {
                if (!target.Renderer)
                    continue;
                foreach (var material in target.Renderer.sharedMaterials) {
                    foreach (var property in material.GetTexturePropertyNames()) {
                        if (!unique.Contains(property)) {
                            unique.Add(property);
                            yield return property;
                        }
                    }
                }
            }
        }

        public static void MetaSubmeshMaskField(Rect rect, SerializedProperty maskProperty, List<int> submeshMasks, int submeshCount)
        {
            var value = MetaSubmeshMaskField(rect, maskProperty.intValue, submeshMasks, submeshCount);
            if (value != maskProperty.intValue)
                maskProperty.intValue = value;
        }

        public static int MetaSubmeshMaskField(Rect rect, int maskValue, List<int> submeshMasks, int submeshCount)
        {
            var labels = new string[submeshMasks.Count];
            for (int i = 0; i < labels.Length; i++)
                labels[i] = SubmeshMaskToString(submeshMasks[i], submeshCount);
            var index = submeshMasks.IndexOf(maskValue);
            index = index == -1 ? 0 : index;
            index = EditorGUI.Popup(rect, index, labels);
            return submeshMasks[index];
        }

        private static readonly string[] submeshIndexNames = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30" };

        public static int SubmeshMaskField(Rect rect, int maskValue, Mesh target)
        {
            if (target) {
                var subMeshCount = target.subMeshCount;
                var subMeshLabels = new string[subMeshCount];
                System.Array.Copy(submeshIndexNames, subMeshLabels, Mathf.Min(submeshIndexNames.Length, subMeshCount));
                maskValue = EditorGUI.MaskField(rect, maskValue, subMeshLabels);
                var label = SubmeshMaskToString(maskValue, subMeshCount);
                if (EditorGUI.showMixedValue)
                    label = "─";
                EditorGUI.LabelField(rect, label, EditorStyles.popup);
                return maskValue;
            } else {
                using (var disabled = new GUIEnableScope(false))
                    EditorGUI.MaskField(rect, 0, new string[1]);
                return maskValue;
            }
        }

        public static void SubmeshMaskField(Rect rect, SerializedProperty maskProperty, Mesh target)
        {
            using (var propertyScope = new EditorGUI.PropertyScope(rect, GUIContent.none, maskProperty)) {
                var mask = SubmeshMaskField(rect, maskProperty.intValue, target);
                if (mask != maskProperty.intValue)
                    maskProperty.intValue = mask;
                EditorGUI.LabelField(rect, new GUIContent("", propertyScope.content.tooltip));
            }
        }

        public static string SubmeshMaskToString(int mask, int submeshCount)
        {
            var relevantMask = (mask & ((1 << submeshCount) - 1));
            if (relevantMask == 0)
                return "Nothing";
            if (relevantMask == (1 << submeshCount) - 1)
                return "Everything";
            var aggregate = "";
            for (int i = 0; i < submeshCount; i++)
                if ((relevantMask & (1 << i)) != 0)
                    aggregate += (aggregate.Length > 0 ? ", " : "") + submeshIndexNames[i];
            return aggregate;
        }

        public static void OptionsTextFieldLayout(SerializedProperty property, System.Func<List<string>> options, bool showLabel = false)
        {
            OptionsTextField(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField), property, options, showLabel);
        }

        public static void OptionsTextField(Rect rect, SerializedProperty property, System.Func<List<string>> options, bool showLabel = false)
        {
            rect.xMax -= rect.height + 1;
            if (showLabel)
                EditorGUI.PropertyField(rect, property);
            else
                FullsizePropertyField(rect, property);

            var style = EditorStyles.miniButton;
            style.padding = new RectOffset(0, 0, 0, 0);
            if (GUI.Button(new Rect(rect.xMax + 1, rect.yMin, rect.height, rect.height), EditorGUIUtility.IconContent("_Menu"), style)) {
                GenericMenu.MenuFunction2 callback = (object data) => {
                    property.stringValue = data as string;
                    property.serializedObject.ApplyModifiedProperties();
                };
                GUI.FocusControl(null);
                var popup = new GenericMenu();
                foreach (var name in options())
                    popup.AddItem(new GUIContent(name), false, callback, name);
                popup.ShowAsContext();
            }
        }

        public static void TextureDescriptorField(Rect rect, SerializedProperty property)
        {
            rect.xMax -= rect.height;

            var channelsProp = property.FindPropertyRelative("Channels");
            var precisionProp = property.FindPropertyRelative("Precision");

            var style = EditorStyles.miniButton;
            style.padding = new RectOffset(0, 0, 0, 0);
            if (GUI.Button(new Rect(rect.xMax, rect.yMin, rect.height, rect.height), EditorGUIUtility.IconContent("_Menu"), style)) {
                GenericMenu.MenuFunction2 callback = (object data) => {
                    var descr = (RenderTextureFormatDescriptor)data;
                    channelsProp.enumValueIndex = (int)descr.Channels;
                    precisionProp.enumValueIndex = (int)descr.Precision;
                    property.serializedObject.ApplyModifiedProperties();
                };
                GUI.FocusControl(null);
                var popup = new GenericMenu();
                foreach (var preset in InternalTextures.Presets())
                    popup.AddItem(new GUIContent(preset.Item1), false, callback, preset.Item2);
                popup.ShowAsContext();
            }

            var layout = new HorizontalLayout(rect, 1, 1);
            FullsizePropertyField(layout.Get(0, 0, 1), channelsProp);
            FullsizePropertyField(layout.Get(1, 0, 1), precisionProp);
        }

        public static void DoLayoutListIndented(this UnityEditorInternal.ReorderableList list)
        {
            EditorGUILayout.Space(list.GetHeight());
            var rect = EditorGUI.IndentedRect(GUILayoutUtility.GetLastRect());
            using (var noIndent = new GUIIndentScope(0))
                list.DoList(rect);
        }

        public static void PropertyComponentsFieldLayout(SerializedProperty texProperty, SerializedProperty vecProperty)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.objectField);
            PropertyComponentsField(rect, texProperty, vecProperty);
        }

        public static void PropertyComponentsField(Rect rect, SerializedProperty texProperty, SerializedProperty vecProperty, GUIContent content = null)
        {
            rect.xMax -= ComponentsFieldWidth + 2;
            if (content != null)
                EditorGUI.PropertyField(rect, texProperty, content);
            else
                EditorGUI.PropertyField(rect, texProperty);
            ComponentsField(new Rect(rect.xMax + 2, rect.y - 1, ComponentsFieldWidth, rect.height), vecProperty);
        }

        public static float ComponentsFieldWidth = 75;

        public static void ComponentsField(Rect rect, SerializedProperty vecProperty)
        {
            var layout = new HorizontalLayout(rect, 1, 1, 1, 1);
            var components = (FFDecal.Mask.Component)vecProperty.intValue;
            var r = components.HasFlag(FFDecal.Mask.Component.R);
            var g = components.HasFlag(FFDecal.Mask.Component.G);
            var b = components.HasFlag(FFDecal.Mask.Component.B);
            var a = components.HasFlag(FFDecal.Mask.Component.A);

            var newVal = (FFDecal.Mask.Component)0;
            using (var highlight = new GUIHighlightScope(r, Color.red))
                newVal |= GUI.Toggle(layout.Get(0, 0, 0), r, "R", EditorStyles.miniButtonLeft) ? FFDecal.Mask.Component.R : 0;
            using (var highlight = new GUIHighlightScope(g, Color.green))
                newVal |= GUI.Toggle(layout.Get(1, 0, 0), g, "G", EditorStyles.miniButtonMid) ? FFDecal.Mask.Component.G : 0;
            using (var highlight = new GUIHighlightScope(b, Color.blue))
                newVal |= GUI.Toggle(layout.Get(2, 0, 0), b, "B", EditorStyles.miniButtonMid) ? FFDecal.Mask.Component.B : 0;
            using (var highlight = new GUIHighlightScope(a, Color.clear))
                newVal |= GUI.Toggle(layout.Get(3, 0, 0), a, "A", EditorStyles.miniButtonRight) ? FFDecal.Mask.Component.A : 0;
            if (components != newVal)
                vecProperty.intValue = (int)newVal;
        }

        public static void ToggleableFieldLayout(SerializedProperty toggleProperty, System.Action drawInternal)
        {
            EditorGUILayout.PropertyField(toggleProperty);
            if (toggleProperty.boolValue) {
                using (var indented = new EditorGUI.IndentLevelScope(1))
                    drawInternal.Invoke();
            }
        }

        public static void FullsizePropertyField(Rect rect, SerializedProperty property)
        {
            EditorGUI.PropertyField(rect, property, new GUIContent("", property.tooltip));
        }

        public static void MinMaxSliderLayout(SerializedProperty property, float lower, float upper)
        {
            var range = property.vector2Value;
            float min = range.x;
            float max = range.y;
            EditorGUILayout.MinMaxSlider(property.displayName, ref min, ref max, lower, upper);
            if (min != range.x || max != range.y)
                property.vector2Value = new Vector2(min, max);
        }

        public static bool ButtonFieldLayout(string label, string text)
        {
            var rect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(rect, label);
            rect.xMin += EditorGUIUtility.labelWidth;
            return GUI.Button(rect, text);
        }
    }

    public class HeaderGroupScope : System.IDisposable
    {
        public bool expanded { get; private set; }

        public HeaderGroupScope(string name)
        {
            expanded = EditorPrefs.GetBool(FFEditorOnlyUtility.EditorPrefPrefix + name, true);
            var after = EditorGUILayout.BeginFoldoutHeaderGroup(expanded, name);
            if (after != expanded) {
                expanded = after;
                EditorPrefs.SetBool(FFEditorOnlyUtility.EditorPrefPrefix + name, expanded);
            }
            EditorGUI.indentLevel++;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    public struct GUIEnableScope : System.IDisposable
    {
        private bool tmp;

        public GUIEnableScope(bool enable)
        {
            tmp = GUI.enabled;
            GUI.enabled = enable;
        }

        public void Dispose()
        {
            GUI.enabled = tmp;
        }
    }

    public struct GUIHighlightScope : System.IDisposable
    {
        private Color tmp;

        public GUIHighlightScope(bool set, Color color)
        {
            tmp = GUI.backgroundColor;
            if (set)
                GUI.backgroundColor = color;
        }

        public void Dispose()
        {
            GUI.backgroundColor = tmp;
        }
    }

    public struct GUIIndentScope : System.IDisposable
    {
        private int tmp;

        public GUIIndentScope(int indent)
        {
            tmp = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indent;
        }

        public void Dispose()
        {
            EditorGUI.indentLevel = tmp;
        }
    }

    public struct LabelWidthScope : System.IDisposable
    {
        private float tmp;

        public LabelWidthScope(float width)
        {
            tmp = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = width;
        }

        public void Dispose()
        {
            EditorGUIUtility.labelWidth = tmp;
        }
    }

    public struct HorizontalLayout
    {
        private Rect rect;
        private float[] elements;

        public HorizontalLayout(Rect rect, params int[] elements)
        {
            this.rect = rect;
            this.elements = new float[elements.Length];
            var sumInv = 1.0f / elements.Sum();
            var tmpSum = 0;
            for (int i = 0; i < elements.Length; i++) {
                tmpSum += elements[i];
                this.elements[i] = tmpSum * sumInv;
            }
        }

        public Rect Get(int index, int line = 0, int padding = 2)
        {
            return Get(index > 0 ? elements[index - 1] : 0f, elements[index], line, padding);
        }

        public Rect Get(float from, float to, int line = 0, int padding = 2)
        {
            var width = rect.width * (to - from) - padding * 2;
            var left = rect.x + rect.width * from + padding;
            var right = left + width;
            var top = rect.y + EditorGUIUtility.singleLineHeight * line;
            return new Rect(left, top, right - left, EditorGUIUtility.singleLineHeight);
        }
    }
}