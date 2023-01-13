using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace FluidFlow
{
    [CustomEditor(typeof(FFDecalSO), true)]
    [CanEditMultipleObjects]
    public class FFDecalCustomInspector : Editor
    {
        private ReorderableList channelsList;
        private SerializedProperty channelsProperty;
        private SerializedProperty maskChannelProperty;

        private void OnEnable()
        {
            var decal = serializedObject.FindProperty("Decal");
            maskChannelProperty = decal.FindPropertyRelative("MaskChannel");
            channelsProperty = decal.FindPropertyRelative("Channels");

            channelsList = new ReorderableList(serializedObject, channelsProperty, true, true, true, true) {
                elementHeight = EditorUtil.ListElementHeight,
                drawHeaderCallback = (Rect rect) => {
                    rect.xMin += rect.height;
                    var layout = new HorizontalLayout(rect, 1, 1, 1, 3);
                    EditorGUI.LabelField(layout.Get(0), "Property", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(1), "Type", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(2), "Source", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(3), "Settings", EditorStyles.centeredGreyMiniLabel);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.y += EditorUtil.ListElementPadding;
                    var layout = new HorizontalLayout(rect, 1, 1, 1, 3);
                    var element = channelsProperty.GetArrayElementAtIndex(index);
                    var name = element.FindPropertyRelative("Property");
                    var type = element.FindPropertyRelative("ChannelType");
                    var source = element.FindPropertyRelative("Source");
                    var data = element.FindPropertyRelative("Data");

                    EditorUtil.FullsizePropertyField(layout.Get(0), name);
                    EditorUtil.FullsizePropertyField(layout.Get(1), type);

                    var sourceType = source.FindPropertyRelative("SourceType");
                    EditorUtil.FullsizePropertyField(layout.Get(2), sourceType);

                    var layout2 = (FFDecal.Channel.Type)type.enumValueIndex != FFDecal.Channel.Type.COLOR
                                    ? new HorizontalLayout(layout.Get(3), 1, 1)
                                    : new HorizontalLayout(layout.Get(3), 1);
                    if ((FFDecal.ColorSource.Type)sourceType.enumValueIndex == FFDecal.ColorSource.Type.TEXTURE)
                        EditorUtil.FullsizePropertyField(layout2.Get(0), source.FindPropertyRelative("Texture"));
                    else
                        EditorUtil.FullsizePropertyField(layout2.Get(0), source.FindPropertyRelative("Color"));

                    switch ((FFDecal.Channel.Type)type.enumValueIndex) {
                        case FFDecal.Channel.Type.NORMAL:
                            using (var labelWidth = new LabelWidthScope(52))
                                EditorGUI.PropertyField(layout2.Get(1), data, new GUIContent("Strength"));
                            break;

                        case FFDecal.Channel.Type.FLUID:
                            using (var labelWidth = new LabelWidthScope(52))
                                EditorGUI.PropertyField(layout2.Get(1), data, new GUIContent("Amount"));
                            break;
                    }
                }
            };
        }

        private void DrawMaskChannelField(SerializedProperty mask)
        {
            EditorUtil.PropertyComponentsFieldLayout(mask.FindPropertyRelative("Texture"), mask.FindPropertyRelative("Components"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mask", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            using (var indent = new EditorGUI.IndentLevelScope(1))
                DrawMaskChannelField(maskChannelProperty);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Channels", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            using (var indent = new EditorGUI.IndentLevelScope(1))
                channelsList.DoLayoutListIndented();

            serializedObject.ApplyModifiedProperties();
        }
    }
}