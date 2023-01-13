using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    /// <summary>
    /// Fix UV seam artifacts, by expanding the color of the uv islands edges by a few pixels
    /// </summary>
    public class FFSeamFixer : MonoBehaviour
    {
        #region Public Properties

        [Tooltip("Target canvas.")]
        public FFCanvas Canvas;

        [Tooltip("Texture channels of the canvas this component affects.")]
        public List<string> TargetTextureChannels = new List<string>() { "" };

        [Tooltip("Cache padding offsets in a texture, insead of recalculating them for each fix? " +
            "Tradeoff between performance and vram usage.")]
        public bool UseCache = true;

        [Tooltip("Set when or how often modified textures are checked for.")]
        public Updater SeamUpdater = new Updater(Updater.Mode.CONTINUOUS);

        public RenderTexture PaddingCache {
            get {
                if (UseCache) {
                    if (paddingCache == null)
                        paddingCache = SeamFixerUtil.CreatePaddingCache(Canvas);
                } else {
                    ClearCache();
                }
                return paddingCache;
            }
        }

        #endregion Public Properties

        #region Private Variables

        private RenderTexture paddingCache;
        private HashSet<TextureChannelIdentifier> targetChannels = new HashSet<TextureChannelIdentifier>();
        private HashSet<TextureChannelIdentifier> modifiedChannels = new HashSet<TextureChannelIdentifier>();

        #endregion Private Variables

        #region Public Methods

        /// <summary>
        /// Manually update internal cache, if enabled.
        /// </summary>
        public void UpdateCache()
        {
            ClearCache();
            paddingCache = PaddingCache;
        }

        /// <summary>
        /// Update which TextureChannels are targeted by this component from the list of TargetTextureChannel names.
        /// </summary>
        public void UpdateTargetTextureChannels()
        {
            targetChannels.Clear();
            foreach (var channel in TargetTextureChannels)
                targetChannels.Add(channel);
        }

        /// <summary>
        /// Marks a TextureChannel as modified.
        /// </summary>
        public void AddModifiedChannel(TextureChannelIdentifier textureChannel)
        {
            modifiedChannels.Add(textureChannel);
        }

        /// <summary>
        /// Fix seams of all TextureChannels marked as modified and contained in the target TextureChannels.
        /// </summary>
        public void FixModifiedChannels()
        {
            foreach (var channel in modifiedChannels) {
                if (targetChannels.Contains(channel)) {
                    using (var paintScope = Canvas.BeginPaintScope(channel, false)) {
                        if (paintScope.IsValid) {
                            if (UseCache)
                                SeamFixerUtil.FixSeams(PaddingCache, paintScope.Target, true);
                            else
                                Canvas.RenderTargets.FixSeams(paintScope.Target);
                        }
                    }
                }
            }
            modifiedChannels.Clear();
        }

        /// <summary>
        /// Clear cache and release internal resources (if present).
        /// </summary>
        public void ClearCache()
        {
            if (paddingCache != null) {
                paddingCache.Release();
                paddingCache = null;
            }
        }

        #endregion Public Methods

        #region Private

        private void Start()
        {
            SeamUpdater.AddListener(FixModifiedChannels);
            Canvas.OnTextureChannelUpdated.AddListener(AddModifiedChannel);
            // automatically recalculate cache when RenderTargets or TextureChannels are updated
            Canvas.OnTextureChannelsUpdated.AddListener(UpdateCache);
            Canvas.OnRenderTargetsUpdated.AddListener(UpdateCache);
            UpdateTargetTextureChannels();
        }

        private void LateUpdate()
        {
            SeamUpdater.Update();
        }

        #endregion Private
    }

    #region Util

    public static class SeamFixerUtil
    {
        /// <summary>
        /// Fix seams of all specified RenderTextures
        /// </summary>
        public static void FixSeams(this List<RenderTarget> renderers, IEnumerable<RenderTexture> targets, int resolution)
        {
            using (RestoreRenderTarget.RestoreActive()) {
                using (var uvMap = new TmpRenderTexture(InternalTextures.PaddingFormat, resolution)) {
                    renderers.EncodeSeamFixMap(uvMap);
                    foreach (var target in targets)
                        FixSeams(uvMap, target, true);
                }
            }
        }

        /// <summary>
        /// Fix seams of the specified RenderTexture, witout padding cache.
        /// </summary>
        public static void FixSeams(this List<RenderTarget> renderers, RenderTexture target)
        {
            using (RestoreRenderTarget.RestoreActive()) {
                using (var uvMap = new TmpRenderTexture(InternalTextures.PaddingFormat, target.width)) {
                    renderers.DrawUVMap(uvMap);
                    FixSeams(uvMap, target, false);
                }
            }
        }

        /// <summary>
        /// Fix seams of the specified RenderTexture, using an existing uv unwrap, or padding cache.
        /// </summary>
        public static void FixSeams(RenderTexture seamTex, RenderTexture target, bool precalcualtedOffset)
        {
            using (RestoreRenderTarget.RestoreActive()) {
                Shader.SetGlobalTexture(InternalShaders.SeamTexPropertyID, seamTex);
                using (var tmp = new TmpRenderTexture(target.descriptor)) {
                    Graphics.Blit(target, tmp);
                    Graphics.Blit(tmp, target, InternalShaders.ApplyPaddingVariant(precalcualtedOffset));
                }
            }
        }

        /// <summary>
        /// Calculate and store padding offsets to the target texture.
        /// </summary>
        public static void EncodeSeamFixMap(this List<RenderTarget> renderers, RenderTexture target)
        {
            using (RestoreRenderTarget.RestoreActive()) {
                using (var tmp = new TmpRenderTexture(target.descriptor)) {
                    // draw uv map of mesh
                    renderers.DrawUVMap(tmp);
                    // calculate and store padding in the target texture
                    Graphics.Blit(tmp, target, InternalShaders.EncodePadding);
                }
            }
        }

        /// <summary>
        /// Create a rendertexture with cached padding offsets.
        /// </summary>
        public static RenderTexture CreatePaddingCache(FFCanvas canvas)
        {
            var rt = InternalTextures.CreateRenderTexture(InternalTextures.PaddingFormat, canvas.Resolution);
            rt.filterMode = FilterMode.Point;
            canvas.RenderTargets.EncodeSeamFixMap(rt);
            return rt;
        }
    }

    #endregion Util
}