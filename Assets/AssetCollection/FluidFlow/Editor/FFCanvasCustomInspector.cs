using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace FluidFlow
{
    [CustomEditor(typeof(FFCanvas), true)]
    [CanEditMultipleObjects]
    public class FFCanvasCustomInspector : Editor
    {
        private ReorderableList renderersList;
        private ReorderableList texturesList;
        private SerializedProperty initializeOnAwakeProperty;
        private SerializedProperty initializeAsyncProperty;
        private SerializedProperty renderersProperty;
        private SerializedProperty texturesProperty;
        private SerializedProperty resolutionProperty;
        private SerializedProperty overrideProperty;
        private SerializedProperty atlasOverrideProperty;
        private SerializedProperty uvOverrideProperty;
        private SerializedProperty borderPaddingProperty;

        private void OnEnable()
        {
            initializeOnAwakeProperty = serializedObject.FindProperty("InitializeOnAwake");
            initializeAsyncProperty = serializedObject.FindProperty("InitializeAsync");
            renderersProperty = serializedObject.FindProperty("RenderTargetDescriptors");
            texturesProperty = serializedObject.FindProperty("TextureChannelDescriptors");
            resolutionProperty = serializedObject.FindProperty("Resolution");
            overrideProperty = serializedObject.FindProperty("OverrideShaderNames");
            atlasOverrideProperty = serializedObject.FindProperty("AtlasPropertyOverride");
            uvOverrideProperty = serializedObject.FindProperty("UV1KeywordOverride");
            borderPaddingProperty = serializedObject.FindProperty("TextureBorderPadding");

            // Set up the reorderable list
            renderersList = new ReorderableList(serializedObject, renderersProperty, true, true, true, true) {
                elementHeight = EditorUtil.ListElementHeight,
                drawHeaderCallback = (Rect rect) => {
                    rect.xMin += rect.height;
                    var layout = new HorizontalLayout(rect, 2, 1, 1, 1, 2);
                    EditorGUI.LabelField(layout.Get(0), "Renderers", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(1), "UV", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(2), "Submeshes", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(3), "Cache", EditorStyles.centeredGreyMiniLabel);
                    if (!serializedObject.isEditingMultipleObjects) {
                        var tiles = (target as FFCanvas).RenderTargetDescriptors.AtlasTileCount();
                        var atlasSize = Mathf.CeilToInt(Mathf.Sqrt(tiles));
                        EditorGUI.LabelField(layout.Get(4),
                                             $"Atlas (size: {atlasSize}; tiles: {tiles}/{atlasSize * atlasSize})",
                                             EditorStyles.centeredGreyMiniLabel);
                    }
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.y += EditorUtil.ListElementPadding;
                    var layout = new HorizontalLayout(rect, 2, 1, 1, 1, 2);
                    var element = renderersProperty.GetArrayElementAtIndex(index);
                    var rendererProp = element.FindPropertyRelative("Renderer");
                    var cacheProp = element.FindPropertyRelative("ModelCache");
                    var submeshProp = element.FindPropertyRelative("SubmeshMask");
                    var uvProp = element.FindPropertyRelative("UVSet");
                    var atlasProp = element.FindPropertyRelative("AtlasConfiguration");

                    EditorUtil.FullsizePropertyField(layout.Get(0), rendererProp);
                    var renderer = rendererProp.objectReferenceValue as Renderer;
                    if (renderer) {
                        var targetMesh = renderer.GetMesh();
                        if (targetMesh) {
                            EditorUtil.FullsizePropertyField(layout.Get(1), uvProp);
                            using (new GUIHighlightScope(submeshProp.intValue == 0, Color.red))
                                EditorUtil.SubmeshMaskField(layout.Get(2), submeshProp, targetMesh);
                            var cache = cacheProp.objectReferenceValue as FFModelCache;
                            using (new GUIHighlightScope(cache && !cache.Matches(targetMesh, (UVSet)uvProp.enumValueIndex, submeshProp.intValue), Color.red))
                                EditorUtil.FullsizePropertyField(layout.Get(3), cacheProp);
                        } else {
                            EditorGUI.LabelField(layout.Get(.25f, 1f), "No mesh assigned to renderer.", EditorStyles.centeredGreyMiniLabel);
                        }
                    }

                    if (index == 0) {
                        using (var disableGUI = new GUIEnableScope(false))
                            EditorGUI.EnumPopup(layout.Get(4), GUIContent.none, AtlasConfiguration.NewTile);
                    } else {
                        EditorUtil.FullsizePropertyField(layout.Get(4), atlasProp);
                    }
                }
            };
            texturesList = new ReorderableList(serializedObject, texturesProperty, true, true, true, true) {
                elementHeight = EditorUtil.ListElementHeight,
                drawHeaderCallback = (Rect rect) => {
                    rect.xMin += rect.height;
                    var layout = new HorizontalLayout(rect, 3, 2, 1);
                    EditorGUI.LabelField(layout.Get(0), "Texture Property", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(1), "Texture Descriptor", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(2), "Initialization Mode", EditorStyles.centeredGreyMiniLabel);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.y += EditorUtil.ListElementPadding;
                    var layout = new HorizontalLayout(rect, 3, 2, 1);
                    var element = texturesProperty.GetArrayElementAtIndex(index);
                    EditorUtil.OptionsTextField(layout.Get(0), element.FindPropertyRelative("TextureProperty"), getUniqueTextureProperties);
                    EditorUtil.TextureDescriptorField(layout.Get(1), element.FindPropertyRelative("Format"));
                    EditorUtil.FullsizePropertyField(layout.Get(2), element.FindPropertyRelative("Initialization"));
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var group = new HeaderGroupScope("Render Targets")) {
                if (group.expanded) {
                    EditorGUILayout.PropertyField(initializeOnAwakeProperty);
                    EditorGUILayout.PropertyField(initializeAsyncProperty);
                    renderersList.displayRemove = renderersList.count > 1;
                    renderersList.DoLayoutListIndented();
                }
            }

            using (var group = new HeaderGroupScope("Texture Channels")) {
                if (group.expanded) {
                    EditorGUILayout.PropertyField(resolutionProperty);
                    texturesList.DoLayoutListIndented();
                }
            }

            using (var group = new HeaderGroupScope("Advanced")) {
                if (group.expanded) {
                    if (EditorUtil.ButtonFieldLayout("Debug Textures", "Open Preview"))
                        FFCanvasDebugEditor.Show(target as FFCanvas);

                    EditorGUILayout.PropertyField(borderPaddingProperty);
                    using (var check = new EditorGUI.ChangeCheckScope()) {
                        EditorUtil.ToggleableFieldLayout(overrideProperty, () => {
                            EditorGUILayout.PropertyField(atlasOverrideProperty);
                            EditorGUILayout.PropertyField(uvOverrideProperty);
                        });
                        if (check.changed) {
                            if (atlasOverrideProperty.stringValue.Length == 0)
                                atlasOverrideProperty.stringValue = InternalShaders.AtlasTransformPropertyName;
                            if (uvOverrideProperty.stringValue.Length == 0)
                                uvOverrideProperty.stringValue = InternalShaders.SecondaryUVKeyword;
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private List<string> getUniqueTextureProperties()
        {
            var properties = new List<string>();
            var canvas = target as FFCanvas;
            if (canvas) {
                properties.AddRange(EditorUtil.EnumerateUniqueTexturePropertyNames(canvas.RenderTargetDescriptors));
                foreach (var descr in canvas.TextureChannelDescriptors)
                    properties.Remove(descr.MainTexturePropertyName());
            }
            return properties;
        }

        [MenuItem("CONTEXT/FFCanvas/Add FFSimulator")]
        public static void AddFluidSimulator(MenuCommand menuCommand)
        {
            var canvas = (FFCanvas)menuCommand.context;
            var sim = canvas.gameObject.AddComponent<FFSimulator>();
            sim.Canvas = canvas;
        }

        [MenuItem("CONTEXT/FFCanvas/Add FFSeamFixer")]
        public static void AddSeamStitcher(MenuCommand menuCommand)
        {
            var canvas = (FFCanvas)menuCommand.context;
            var fix = canvas.gameObject.AddComponent<FFSeamFixer>();
            fix.Canvas = canvas;
        }

        [MenuItem("CONTEXT/FFCanvas/Debug Texture Viewer")]
        public static void CanvasDebugger(MenuCommand menuCommand)
        {
            FFCanvasDebugEditor.Show(menuCommand.context as FFCanvas);
        }

    }
}