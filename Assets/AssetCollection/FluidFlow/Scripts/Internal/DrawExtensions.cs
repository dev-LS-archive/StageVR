using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class DrawExtensions
    {
        public static void DrawRenderTargets(this CommandBuffer command, List<RenderTarget> targets, PerRenderTargetVariant materialVariant, bool onlyActive = true)
        {
            foreach (var target in targets) {
                if (onlyActive && (!target.Renderer.enabled || !target.Renderer.gameObject.activeInHierarchy))
                    continue;
                command.SetGlobalVector(InternalShaders.AtlasTransformPropertyID, target.AtlasTransform);
                var material = materialVariant.Get(target.UVSet);
                foreach (var submeshIndex in target.SubmeshMask.EnumerateSetBits())
                    command.DrawRenderer(target.Renderer, material, submeshIndex, 0);
            }
        }

        public static void DrawMeshes(this List<RenderTarget> targets, PerRenderTargetVariant materialVariant)
        {
            foreach (var target in targets)
                target.DrawMesh(materialVariant);
        }

        public static void DrawMesh(this RenderTarget target, PerRenderTargetVariant materialVariant)
        {
            Shader.SetGlobalVector(InternalShaders.AtlasTransformPropertyID, target.AtlasTransform);
            materialVariant.Get(target.UVSet).SetPass(0);
            foreach (var submeshIndex in target.SubmeshMask.EnumerateSetBits())
                Graphics.DrawMeshNow(target.Mesh, Vector3.zero, Quaternion.identity, submeshIndex);
        }

        public static void DrawUVMap(this List<RenderTarget> renderers, RenderTexture target)
        {
            Graphics.SetRenderTarget(target);
            GL.Clear(false, true, Color.clear);
            renderers.DrawMeshes(InternalShaders.UVUnwrap);
        }

        public static void InitializeTextureChannel(this List<RenderTarget> targets, RenderTexture targetTex, TextureChannelDescriptor channelDescriptor, HashSet<TextureChannelIdentifier> texturePropertyAliases)
        {
            using (RestoreRenderTarget.RestoreActive()) {
                Graphics.SetRenderTarget(targetTex);
                GL.Clear(false, true, Color.clear);
                var sharedMaterialsCache = new List<Material>();
                foreach (var target in targets) {
                    Shader.SetGlobalVector(InternalShaders.AtlasTransformPropertyID, target.AtlasTransform);
                    foreach (var submeshIndex in target.SubmeshMask.EnumerateSetBits()) {
                        if (channelDescriptor.Initialization == TextureChannelDescriptor.InitializationMode.COPY) {
                            target.Renderer.GetSharedMaterials(sharedMaterialsCache);
                            bool found = false;
                            foreach (var alias in texturePropertyAliases) {
                                if (sharedMaterialsCache[submeshIndex].HasProperty(alias)) {
                                    var existingTex = sharedMaterialsCache[submeshIndex].GetTexture(alias);
                                    if (existingTex != null)
                                        Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, existingTex);
                                    else
                                        Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, sharedMaterialsCache[submeshIndex].GetDefaultTexture(channelDescriptor.TexturePropertyNameForAlias(alias)));
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                Debug.LogWarningFormat("Copy initialization failed. '{0}' of '{1}' has no property '{2}'.", sharedMaterialsCache[submeshIndex], target.Renderer, channelDescriptor.MainTexturePropertyName());
                        } else {
                            channelDescriptor.Initialization.SetGlobalShaderColor();
                        }
                        InternalShaders.TextureInitializationVariant(target.UVSet == UVSet.UV1).SetPass(0);
                        Graphics.DrawMeshNow(target.Mesh, Vector3.zero, Quaternion.identity, submeshIndex);
                    }
                }
            }
        }

        private static void SetGlobalShaderColor(this TextureChannelDescriptor.InitializationMode mode)
        {
            switch (mode) {
                case TextureChannelDescriptor.InitializationMode.WHITE:
                    Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, Texture2D.whiteTexture);
                    break;

                case TextureChannelDescriptor.InitializationMode.GRAY:
                    Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, Texture2D.grayTexture);
                    break;

                case TextureChannelDescriptor.InitializationMode.BLACK:
                    Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, Texture2D.blackTexture);
                    break;

                case TextureChannelDescriptor.InitializationMode.BUMP:
                    Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, Texture2D.normalTexture);
                    break;

                case TextureChannelDescriptor.InitializationMode.RED:
                    Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, Texture2D.redTexture);
                    break;
            }
        }
    }
}