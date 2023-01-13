using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace FluidFlow
{
    [CustomEditor(typeof(FFSimulator), true)]
    [CanEditMultipleObjects]
    public class FFSimulatorCustomInspector : Editor
    {
        // General

        private SerializedProperty canvasProperty;
        private SerializedProperty targetChannelProperty;
        private SerializedProperty updateInvisibleProperty;
        private SerializedProperty timeoutEnabledProperty;
        private SerializedProperty timeoutCyclesProperty;

        // Gravity

        private UpdaterCustomInspector gravityUpdater;
        private SerializedProperty gravityUpdateInitializedProperty;
        private SerializedProperty useNormalProperty;
        private SerializedProperty normalNameProperty;
        private SerializedProperty normalFallbackProperty;
        private SerializedProperty normalInfluenceProperty;

        // Fluid

        private UpdaterCustomInspector fluidUpdater;
        private SerializedProperty fluidRetainedFlatProperty;
        private SerializedProperty fluidRetainedSteepProperty;

        // Evaporation

        private SerializedProperty useEvaportationProperty;
        private SerializedProperty evaporationAmountProperty;
        private SerializedProperty evaporationTimeoutProperty;
        private UpdaterCustomInspector evaporationUpdater;

        private void OnEnable()
        {
            canvasProperty = serializedObject.FindProperty("Canvas");
            targetChannelProperty = serializedObject.FindProperty("TargetTextureChannel");
            updateInvisibleProperty = serializedObject.FindProperty("UpdateInvisible");
            timeoutEnabledProperty = serializedObject.FindProperty("UseTimeout");
            timeoutCyclesProperty = serializedObject.FindProperty("Timeout");

            gravityUpdateInitializedProperty = serializedObject.FindProperty("UpdateOnInitialized");
            gravityUpdater = new UpdaterCustomInspector(serializedObject.FindProperty("GravityUpdater"));
            useNormalProperty = serializedObject.FindProperty("UseNormalMaps");
            normalNameProperty = serializedObject.FindProperty("NormalPropertyName");
            normalFallbackProperty = serializedObject.FindProperty("NormalTextureFallback");
            normalInfluenceProperty = serializedObject.FindProperty("NormalInfluence");

            fluidUpdater = new UpdaterCustomInspector(serializedObject.FindProperty("FluidUpdater"));
            fluidRetainedFlatProperty = serializedObject.FindProperty("FluidRetainedFlat");
            fluidRetainedSteepProperty = serializedObject.FindProperty("FluidRetainedSteep");

            useEvaportationProperty = serializedObject.FindProperty("UseEvaporation");
            evaporationUpdater = new UpdaterCustomInspector(serializedObject.FindProperty("EvaporationUpdater"));
            evaporationAmountProperty = serializedObject.FindProperty("EvaporationAmount");
            evaporationTimeoutProperty = serializedObject.FindProperty("EvaporationTimeout");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var group = new HeaderGroupScope("General")) {
                if (group.expanded) {
                    EditorGUILayout.PropertyField(canvasProperty);

                    var canvas = canvasProperty.objectReferenceValue as FFCanvas;
                    if (canvas && canvas.Resolution > (1 << 13))
                        EditorGUILayout.HelpBox("Due tue precision, uv seams of fluid textures larger than 8192x8192 can not be stitched properly.", MessageType.Info);

                    using (var indented = new EditorGUI.IndentLevelScope(1))
                        EditorUtil.OptionsTextFieldLayout(targetChannelProperty, getTextureChannelOptions, true);
                    EditorGUILayout.PropertyField(updateInvisibleProperty);
                    EditorUtil.ToggleableFieldLayout(timeoutEnabledProperty, () => EditorGUILayout.PropertyField(timeoutCyclesProperty));
                }
            }

            using (var group = new HeaderGroupScope("Gravity")) {
                if (group.expanded) {
                    EditorGUILayout.PropertyField(gravityUpdateInitializedProperty);
                    gravityUpdater.OnInspectorGUI();
                    EditorUtil.ToggleableFieldLayout(useNormalProperty, () => {
                        EditorUtil.OptionsTextFieldLayout(normalNameProperty, getUniqueTextureProperties, true);
                        EditorGUILayout.PropertyField(normalFallbackProperty);
                        EditorGUILayout.PropertyField(normalInfluenceProperty);
                    });
                }
            }

            using (var group = new HeaderGroupScope("Fluid")) {
                if (group.expanded) {
                    fluidUpdater.OnInspectorGUI();
                    EditorGUILayout.PropertyField(fluidRetainedFlatProperty);
                    EditorGUILayout.PropertyField(fluidRetainedSteepProperty);
                }
            }

            using (var group = new HeaderGroupScope("Evaporation")) {
                if (group.expanded) {
                    EditorUtil.ToggleableFieldLayout(useEvaportationProperty, () => {
                        evaporationUpdater.OnInspectorGUI();
                        EditorGUILayout.PropertyField(evaporationAmountProperty);
                        if (timeoutEnabledProperty.boolValue)
                            EditorGUILayout.PropertyField(evaporationTimeoutProperty, new GUIContent("Additional Timeout"));
                    });
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private List<string> getTextureChannelOptions()
        {
            var properties = new List<string>();
            var canvas = (target as FFSimulator)?.Canvas;
            if (canvas) {
                foreach (var channel in canvas.TextureChannelDescriptors) {
                    if (channel.Format.Channels == ChannelSetup.RGBA)
                        properties.Add(channel.MainTexturePropertyName());
                }
            }
            return properties;
        }

        private List<string> getUniqueTextureProperties()
        {
            var properties = new List<string>();
            var canvas = (target as FFSimulator)?.Canvas;
            if (canvas)
                properties.AddRange(EditorUtil.EnumerateUniqueTexturePropertyNames(canvas.RenderTargetDescriptors));
            return properties;
        }
    }
}