using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Linq;

namespace FluidFlow
{
    [CustomEditor(typeof(FFModelCache))]
    public class FFModelCacheInspector : Editor
    {
        private FFModelCache cache;
        private ReorderableList list;
        private List<FFModelCache.Setting> settings;

        private string[] meshNames;
        private int[] meshIds;

        private bool showUnwrapParams;
        private SerializedProperty unwrapParamsProp;

        private void OnEnable()
        {
            cache = target as FFModelCache;
            updateSettings();
            unwrapParamsProp = serializedObject.FindProperty("SecondaryUVUnwrapParameters");
        }

        private void OnDisable()
        {
            if (!isCacheValid())
                return;
            if (!cache.Matches(settings)) {
                if (EditorUtility.DisplayDialog("FFModelCache", "Apply changes to " + cache.ToString() + "?", "Apply", "Revert"))
                    applyChanges();
            }
        }

        private void updateSettings()
        {
            if (!isCacheValid())
                return;
            settings = cache.GetSettings();

            meshNames = new string[cache.SourceMeshes.Length];
            meshIds = new int[cache.SourceMeshes.Length];
            for (int i = 0; i < cache.SourceMeshes.Length; i++) {
                meshNames[i] = cache.SourceMeshes[i].name;
                meshIds[i] = i;
            }
            showUnwrapParams = cache.SourceMeshes.Any(mesh => !mesh.HasVertexAttribute(VertexAttribute.TexCoord1));

            list = new ReorderableList(settings, typeof(FFModelCache.Setting), true, true, true, true) {
                elementHeight = EditorUtil.ListElementHeight,
                drawHeaderCallback = (Rect rect) => {
                    rect.xMin += rect.height;
                    var layout = new HorizontalLayout(rect, 2, 1, 1);
                    EditorGUI.LabelField(layout.Get(0), "Mesh", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(1), "UV", EditorStyles.centeredGreyMiniLabel);
                    EditorGUI.LabelField(layout.Get(2), "Stitch Submesh Mask", EditorStyles.centeredGreyMiniLabel);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    rect.y += EditorUtil.ListElementPadding;
                    var layout = new HorizontalLayout(rect, 2, 1, 1);

                    var item = settings[index];
                    item.SourceMeshId = EditorGUI.IntPopup(layout.Get(0), item.SourceMeshId, meshNames, meshIds);
                    item.UVSet = (UVSet)EditorGUI.EnumPopup(layout.Get(1), item.UVSet);
                    item.SubmeshMask = EditorUtil.SubmeshMaskField(layout.Get(2), item.SubmeshMask, cache.SourceMeshes[item.SourceMeshId]);
                    settings[index] = item;
                }
            };
        }

        private GUIContent autoUpdateToggle = new GUIContent("Auto Update Before Play",
            "Ensure all caches are up-to-date before entering the play mode. A cache is only updated, if the hash of the target model has changed. Note that enabling this can add some overhead to entering the playmode.");

        private GUIContent updateCacheButton = new GUIContent("Update All Caches",
           "Update all caches in the current project, whose target models have been modified.");

        public override void OnInspectorGUI()
        {
            if (isCacheValid()) {
                serializedObject.Update();

                using (var disabled = new GUIEnableScope(false))
                    EditorGUILayout.ObjectField("Target", cache.Target, typeof(GameObject), false);
                EditorGUILayout.Space();

                list.DoLayoutList();

                if (showUnwrapParams) {
                    using (var disabled = new GUIEnableScope(settings.Any(setting => setting.UVSet == UVSet.UV1 && !cache.SourceMeshes[setting.SourceMeshId].HasVertexAttribute(VertexAttribute.TexCoord1))))
                        EditorGUILayout.PropertyField(unwrapParamsProp);
                }

                using (var h = new GUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();
                    if (!cache.Matches(settings)) {
                        if (GUILayout.Button("Revert"))
                            updateSettings();
                        if (GUILayout.Button("Apply"))
                            applyChanges();
                    } else {
                        if (GUILayout.Button("Regenerate"))
                            applyChanges(true);
                    }
                }

                serializedObject.ApplyModifiedProperties();
            } else if (cache && !cache.Target) {
                EditorGUILayout.LabelField("Target has been removed.");
            }

            EditorGUILayout.Space();
            using (var header = new HeaderGroupScope(typeof(FFModelCache).Name + " Global Settings")) {
                if (header.expanded) {
                    FFEditorOnlyUtility.AutoUpdateModelCaches = EditorGUILayout.Toggle(autoUpdateToggle, FFEditorOnlyUtility.AutoUpdateModelCaches);
                    if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), updateCacheButton))
                        FFModelCacheUpdater.UpdateAllCaches();
                }
            }
        }

        private void applyChanges(bool force = false)
        {
            cache.ApplySettings(settings, force);
            EditorUtility.SetDirty(cache);
            AssetDatabase.SaveAssets();
            updateSettings();
        }

        private bool isCacheValid()
        {
            return cache && cache.Target;
        }
    }

    public static class FFModelCacheCreator
    {
        [MenuItem("Assets/Create/Fluid Flow/Model Cache")]
        public static void CreateCache()
        {
            CreateFromModel(Selection.activeGameObject);
        }

        [MenuItem("Assets/Create/Fluid Flow/Model Cache", validate = true)]
        public static bool ValidateCreateCache()
        {
            if (!Selection.activeGameObject)
                return false;
            return EditorUtil.IsModel(Selection.activeGameObject);
        }

        [MenuItem("CONTEXT/FFCanvas/Set Auto Update FFModelCaches")]
        [MenuItem("CONTEXT/FFModelCache/Set Auto Update FFModelCaches")]
        public static void SetAutoCacheUpdate()
        {
            FFEditorOnlyUtility.AutoUpdateModelCaches = EditorUtility.DisplayDialog("Auto Update FFModelCaches Before Play?",
                "Ensure all caches are up-to-date before entering the play mode. A cache is only updated, if the hash of the target model has changed. Note that enabling this can add some overhead to entering the playmode.",
                "Enable",
                "Disable");
        }

        public static void CreateFromModel(GameObject model)
        {
            if (!EditorUtil.IsModel(model)) {
                Debug.LogErrorFormat("Unable to create FluidModelCache for {0}. Not a model.", model.name);
                return;
            }

            FFModelCache asset = ScriptableObject.CreateInstance<FFModelCache>();
            try {
                asset.Initialize(model);
            } catch (FileLoadException e) {
                Debug.LogError(e.ToString());
                return;
            }

            var path = AssetDatabase.GetAssetPath(model);
            path = Path.Combine(Path.GetDirectoryName(path), model.name + "_Cache.asset");
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}