using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FluidFlow;
public class FFCanvasDebugEditor : EditorWindow
{
    public FFCanvas target;
    private Vector2 scrollPos;
    private float scale = 1;
    private Dictionary<string, bool> expanded = new Dictionary<string, bool>();

    [MenuItem("Window/Analysis/FFCanvas Texture Debugger", false, 100000)]
    public static void ShowWindow()
    {
        Get().Show();
    }

    public static void Show(FFCanvas canvas)
    {
        var window = Get();
        window.target = canvas;
        window.Show();
    }

    private static FFCanvasDebugEditor Get()
    {
        return GetWindow<FFCanvasDebugEditor>(typeof(FFCanvas).Name + " Debugger");
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField(typeof(FFCanvas).Name + " Texture Preview", EditorStyles.largeLabel);

        EditorGUILayout.Space();

        target = EditorGUILayout.ObjectField(target, typeof(FFCanvas), true) as FFCanvas;
        scale = EditorGUILayout.Slider("Preview Scale", scale, .1f, 10f);

        if (target != null) {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos)) {
                scrollPos = scrollView.scrollPosition;
                EditorGUILayout.Space();
                using (var layoutGroup = new HeaderGroupScope("Atlas UV Layout Preview")) {
                    if (layoutGroup.expanded) {
                        EditorGUILayout.HelpBox("Red areas mark overlapping UVs. Painting of the FFCanvas will not work properly with overlapping UVs! Resolve this via a custom UV unmap, switching to lightmapping UVs, or splitting submeshes into separate render targets.", MessageType.Info);
                        drawUVLayoutPreview();
                    }
                }

                EditorGUILayout.Space();

                using (var group = new HeaderGroupScope("Texture Channel Preview")) {
                    if (group.expanded) {
                        if (target.State != InitializationState.INITIALIZED)
                            EditorGUILayout.LabelField("Runtime only", EditorStyles.miniLabel);
                        var initialized = target.State == InitializationState.INITIALIZED;
                        using (new GUIEnableScope(initialized)) {
                            foreach (var descr in target.TextureChannelDescriptors) {
                                var identifier = descr.MainTexturePropertyName();
                                setExpanded(identifier, EditorGUILayout.Foldout(isExpanded(identifier), identifier, true));
                                if (initialized && target.TextureChannels.TryGetValue(identifier, out var tex) && isExpanded(identifier)) {
                                    EditorGUILayout.Space();
                                    PreviewTexture(tex);
                                }
                                EditorGUILayout.Space();
                            }
                        }
                    }
                }
            }
        }
    }

    private void drawUVLayoutPreview()
    {
        if (target) {
            using (var overlap = new TmpRenderTexture(new RenderTextureDescriptor(target.Resolution, target.Resolution, RenderTextureFormat.ARGB32))) {
                using (var uv = new TmpRenderTexture(new RenderTextureDescriptor(target.Resolution, target.Resolution, RenderTextureFormat.ARGB32))) {
                    using (RestoreRenderTarget.RestoreActive()) {
                        Graphics.SetRenderTarget(uv);
                        GL.Clear(false, true, Color.clear);
                        foreach (var renderer in target.RenderTargetDescriptors.EnumerateAtlas(target.TextureBorderPadding ? (1f / target.Resolution) : 0)) {
                            var mesh = renderer.RenderTargetDescriptor.Renderer.GetMesh();
                            if (mesh) {
                                if (renderer.RenderTargetDescriptor.UVSet == UVSet.UV1 && !mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord1)) {
                                    // try to get secondary uv mesh from cache
                                    var cache = renderer.RenderTargetDescriptor.ModelCache;
                                    if (cache) {
                                        var index = System.Array.IndexOf(cache.SourceMeshes, mesh);
                                        if (index != -1 && cache.CachedSecondaryUVMeshes[index] != null)
                                            mesh = cache.CachedSecondaryUVMeshes[index];
                                    }
                                    if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord1)) {
                                        Debug.LogWarningFormat("Failed drawing preview for {0}, as it has no UV1", mesh);
                                        continue;
                                    }
                                }
                                var submeshMask = mesh.ValidateSubmeshMask(renderer.RenderTargetDescriptor.SubmeshMask);
                                Shader.SetGlobalVector(InternalShaders.AtlasTransformPropertyID, renderer.AtlasTransform);
                                InternalShaders.UVOverlap.Get(renderer.RenderTargetDescriptor.UVSet == UVSet.UV1 ? 1 : 0).SetPass(0);
                                foreach (var submeshIndex in submeshMask.EnumerateSetBits())
                                    Graphics.DrawMeshNow(mesh, Matrix4x4.identity, submeshIndex);
                            }
                        }
                        Graphics.Blit(uv, overlap, InternalShaders.MarkOverlap);
                    }
                    PreviewTexture(overlap);
                }
            }
        }
    }

    private void setExpanded(string key, bool value)
    {
        if (expanded.ContainsKey(key))
            expanded[key] = value;
        else
            expanded.Add(key, value);
    }

    private bool isExpanded(string key)
    {
        return expanded.ContainsKey(key) && expanded[key];
    }

    private void PreviewTexture(Texture texture)
    {
        var rect = GUILayoutUtility.GetRect(texture.width * scale, texture.height * scale);
        using (texture.SetTemporaryFilterMode(FilterMode.Point))
            EditorGUI.DrawTextureTransparent(rect, texture, ScaleMode.ScaleToFit);
    }
}
