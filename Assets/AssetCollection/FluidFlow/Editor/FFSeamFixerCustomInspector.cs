using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace FluidFlow
{
    [CustomEditor(typeof(FFSeamFixer), true)]
    [CanEditMultipleObjects]
    public class FFSeamFixerCustomInspector : Editor
    {
        private SerializedProperty canvasProperty;
        private SerializedProperty channelsProperty;
        private ReorderableList channelsList;
        private SerializedProperty useCacheProperty;
        private UpdaterCustomInspector seamUpdater;

        private void OnEnable()
        {
            canvasProperty = serializedObject.FindProperty("Canvas");
            channelsProperty = serializedObject.FindProperty("TargetTextureChannels");
            useCacheProperty = serializedObject.FindProperty("UseCache");
            seamUpdater = new UpdaterCustomInspector(serializedObject.FindProperty("SeamUpdater"));

            channelsList = new ReorderableList(serializedObject, channelsProperty, true, true, true, true) {
                elementHeight = EditorUtil.ListElementHeight,
                drawHeaderCallback = (Rect rect) => {
                    EditorGUI.LabelField(rect, "Target Texture Channels", EditorStyles.centeredGreyMiniLabel);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.y += EditorUtil.ListElementPadding;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    var channelProperty = channelsProperty.GetArrayElementAtIndex(index);
                    EditorUtil.OptionsTextField(rect, channelProperty, getTextureChannels, false);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(canvasProperty);
            EditorGUILayout.Space();

            channelsList.DoLayoutListIndented();
            EditorGUILayout.Space();

            seamUpdater.OnInspectorGUI();

            EditorGUILayout.PropertyField(useCacheProperty);

            serializedObject.ApplyModifiedProperties();
        }

        private List<string> getTextureChannels()
        {
            var properties = new List<string>();
            var seamFixer = target as FFSeamFixer;
            if (seamFixer) {
                if (seamFixer.Canvas) {
                    // add texture channel names from canvas
                    foreach (var descr in seamFixer.Canvas.TextureChannelDescriptors)
                        properties.Add(descr.MainTexturePropertyName());
                }
                // remove names already added
                foreach (var name in seamFixer.TargetTextureChannels)
                    properties.Remove(name);
            }
            return properties;
        }
    }
}