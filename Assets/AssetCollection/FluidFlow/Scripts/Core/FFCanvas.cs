using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace FluidFlow
{
    public class FFCanvas : MonoBehaviour
    {
        #region Public Properties

        // Render Targets

        [Tooltip("Initialize the canvas automatically on awake?")]
        public bool InitializeOnAwake = true;

        [Tooltip("Initialize canvas asynchronously? " +
            "Depending on complexity initialization might take multiple frames. " +
            "Use caches for faster initialization.")]
        public bool InitializeAsync = true;

        [Tooltip("Description of renderers handled by this canvas.")]
        public List<RenderTargetDescriptor> RenderTargetDescriptors =
           new List<RenderTargetDescriptor>() { new RenderTargetDescriptor() { SubmeshMask = ~0 } };

        // Texture Channels

        [Min(1)]
        [Tooltip("Width and height of the generated textures.")]
        public int Resolution = 512;

        [Tooltip("Description of texture channels used by this canvas. Add texture property name aliases by separating them with a '|'.")]
        public List<TextureChannelDescriptor> TextureChannelDescriptors =
           new List<TextureChannelDescriptor>() { new TextureChannelDescriptor(InternalTextures.FluidFormat, "_FluidTex", TextureChannelDescriptor.InitializationMode.BLACK) };

        // Advanced

        [Tooltip("Override names of internal shader properties/keywords? " +
            "E.g. when multiple canvases access the same renderers.")]
        public bool OverrideShaderNames = false;

        [Tooltip("Shader property name used for defining the atlas offset of the renderer on the texture channel.")]
        public string AtlasPropertyOverride = "";

        [Tooltip("Shader keyword used to indicate, that a material should use its secondary uv set for displaying the texture channels.")]
        public string UV1KeywordOverride = "";

        [Tooltip("Add 1 pixel of padding around each texture atlas tile. This can prevent color bleeding between atlas tiles, and allows proper UV seam stitching for the fluid simulation, when the seam lies on the texture border.")]
        public bool TextureBorderPadding = false;

        // Runtime

        /// <summary>
        /// RenderTarget list initialized at runtime from the RenderTargetDescriptors.
        /// </summary>
        public List<RenderTarget> RenderTargets { get; private set; }

        /// <summary>
        /// TextureChannels initialized at runtime from the TextureChannelDescriptors.
        /// </summary>
        public Dictionary<TextureChannelIdentifier, RenderTexture> TextureChannels { get; private set; }

        /// <summary>
        /// Alias texture property names for a given TextureChannel. Add alias names by adding multiple names to the TextureChannelDescriptor, separated by '|'
        /// </summary>
        public Dictionary<TextureChannelIdentifier, HashSet<TextureChannelIdentifier>> TextureChannelAliases { get; private set; }

        /// <summary>
        /// Name of the atlas transform property for this canvas.
        /// </summary>
        public string AtlasTransformProperty {
            get {
                return OverrideShaderNames ? AtlasPropertyOverride : InternalShaders.AtlasTransformPropertyName;
            }
        }

        /// <summary>
        /// Name of the secondary uv keyword for this canvas.
        /// </summary>
        public string SecondaryUVKeyword {
            get {
                return OverrideShaderNames ? UV1KeywordOverride : InternalShaders.SecondaryUVKeyword;
            }
        }

        /// <summary>
        /// Invoked when a TextureChannel has been drawn on.
        /// Parameter: Shader.PropertyToID("Texture Channel Name")
        /// </summary>
        public UnityEvent<TextureChannelIdentifier> OnTextureChannelUpdated = new UnityEvent<TextureChannelIdentifier>();

        /// <summary>
        /// Invoked when the RenderTarget list has been updated from the RenderTargetDescriptors.
        /// </summary>
        public UnityEvent OnRenderTargetsUpdated = new UnityEvent();

        /// <summary>
        /// Invoked when the TextureChannels have been updated from the TextureChannelDescriptors.
        /// </summary>
        public UnityEvent OnTextureChannelsUpdated = new UnityEvent();

        public InitializationState State { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Sets up the renderers, creates the internal rendertextures and assigns the material properties.
        /// If async initialization is active, initialization might take multiple frames until it is finished.
        /// </summary>
        public void Initialize()
        {
            if (State == InitializationState.UNINITIALIZED) {
                State = InitializationState.INITIALIZING;
                System.Collections.IEnumerator initializeDelayed()
                {
                    UpdateTextureChannels();
                    yield return UpdateRenderTargets();
                    InitializeTextureChannels();
                    UpdateMaterials();
                    State = InitializationState.INITIALIZED;
                }
                StartCoroutine(initializeDelayed());
            }
        }

        /// <summary>
        /// Releases internal rendertextures and unsets material property blocks.
        /// </summary>
        public void Uninitialize()
        {
            if (State == InitializationState.INITIALIZED) {
                // unset textures from material property bocks
                foreach (var target in RenderTargets) {
                    foreach (var index in target.SubmeshMask.EnumerateSetBits())
                        target.Renderer.SetPropertyBlock(null, index);
                }
                releaseTextures();
                State = InitializationState.UNINITIALIZED;
            }
        }

        /// <summary>
        /// Update RenderTargets from RenderTargetDescriptors list.
        /// Note that this is a coroutine, when InitializeAsync is active.
        /// </summary>
        public System.Collections.IEnumerator UpdateRenderTargets()
        {
            if (RenderTargets == null)
                RenderTargets = new List<RenderTarget>();
            else
                RenderTargets.Clear();

            // initialize rendertargets (padding between atlas tiles is relative to the resolution)
            foreach (var atlasDescriptor in RenderTargetDescriptors.EnumerateAtlas(TextureBorderPadding ? (1f / Resolution) : 0)) {
                var renderer = atlasDescriptor.RenderTargetDescriptor.Renderer;
                var mesh = renderer.GetMesh();
                var submeshMask = mesh.ValidateSubmeshMask(atlasDescriptor.RenderTargetDescriptor.SubmeshMask);
                submeshMask = renderer.sharedMaterials.ValidateSubmeshMask(submeshMask);
                var uvSet = atlasDescriptor.RenderTargetDescriptor.UVSet;

                if (uvSet == UVSet.UV1) {
                    var secondaryUVMesh = CacheManager.Instance.RequestSecondaryUVMesh(mesh);
                    if (secondaryUVMesh) {
                        mesh = secondaryUVMesh;
                        renderer.SetMesh(mesh);
                    } else {
                        uvSet = UVSet.UV0;
                    }
                }
                RenderTargets.Add(new RenderTarget(renderer, mesh, submeshMask, uvSet, atlasDescriptor.AtlasTransform));
            }

            // ensure necessary mesh data is generated
            yield return CacheManager.Instance.GenerateRequestedMeshes(InitializeAsync);

            OnRenderTargetsUpdated.Invoke();
        }

        /// <summary>
        /// Update TextureChannels from TextureChannelDescriptors.
        /// </summary>
        /// <param name="releaseOld"></param>
        public void UpdateTextureChannels(bool releaseOld = true)
        {
            // release old textures
            if (releaseOld)
                releaseTextures();

            if (TextureChannels == null)
                TextureChannels = new Dictionary<TextureChannelIdentifier, RenderTexture>();
            else
                TextureChannels.Clear();
            if (TextureChannelAliases == null)
                TextureChannelAliases = new Dictionary<TextureChannelIdentifier, HashSet<TextureChannelIdentifier>>();
            else
                TextureChannelAliases.Clear();

            // create textures
            foreach (var channel in TextureChannelDescriptors) {
                var identifier = channel.MainTextureProperty();
                // ensure each texture property is unique per canvas
                if (!TextureChannels.ContainsKey(identifier)) {
                    TextureChannels.Add(identifier, InternalTextures.CreateRenderTexture(channel.Format, Resolution));
                    TextureChannelAliases.Add(identifier, channel.TexturePropertyAliases());
                } else {
                    Debug.LogWarningFormat("Found duplicate texture property '{0}' in texture channels.", channel.MainTexturePropertyName());
                }
            }

            OnTextureChannelsUpdated.Invoke();
        }

        /// <summary>
        /// Apply textures, atlas transformations to renderers as MaterialProperties.
        /// Additionally, this instances all materials which are not instanced already and require updating the secondary uv keyword.
        /// </summary>
        public void UpdateMaterials()
        {
            // apply material properties to rendertargets
            foreach (var target in RenderTargets) {
                var usesSecondaryUV = target.UVSet == UVSet.UV1;
                var materials = target.Renderer.sharedMaterials;
                foreach (var index in target.SubmeshMask.EnumerateSetBits()) {
                    using (var editMaterial = new ScopedMaterialPropertyBlockEdit(target.Renderer, index)) {
                        // set position of the rendertarget in the texture atlas
                        editMaterial.PropertyBlock.SetVector(AtlasTransformProperty, target.AtlasTransform);
                        // apply texture channels to renderers
                        foreach (var channel in TextureChannels) {
                            if (TextureChannelAliases.TryGetValue(channel.Key, out var aliases)) {
                                foreach (var alias in aliases) {
                                    if (materials[index].HasProperty(alias))
                                        editMaterial.PropertyBlock.SetTexture(alias, channel.Value);
                                }
                            } else {
                                Debug.LogError("Failed getting texture channel aliases.");
                            }
                        }
                    }
                    // switch uv set used by the material, if necessary
                    if (usesSecondaryUV != materials[index].IsKeywordEnabled(SecondaryUVKeyword)) {
                        // ensure material is instanced before changing (instanced objects have an instance id <0)
                        if (materials[index].GetInstanceID() >= 0)
                            // TODO: potential memory leak -> materials have to be destroyed
                            materials[index] = new Material(materials[index]);
                        materials[index].SetKeyword(SecondaryUVKeyword, usesSecondaryUV);
                    }
                }
                target.Renderer.sharedMaterials = materials;
            }
        }

        /// <summary>
        /// Initialize the TextureChannels with the specified color/texture information.
        /// </summary>
        public void InitializeTextureChannels()
        {
            foreach (var descriptor in TextureChannelDescriptors) {
                var identifier = descriptor.MainTextureProperty();
                if (TextureChannels.TryGetValue(identifier, out var target) && TextureChannelAliases.TryGetValue(identifier, out var aliases))
                    RenderTargets.InitializeTextureChannel(target, descriptor, aliases);
            }
            RenderTargets.FixSeams(TextureChannels.Values, Resolution);
        }

        /// <summary>
        /// Is any of the RenderTargets visible to any camera?
        /// </summary>
        /// <returns></returns>
        public bool IsVisible()
        {
            foreach (var target in RenderTargets)
                if (target.Renderer.isVisible)
                    return true;
            return false;
        }

        #endregion Public Methods

        #region Private

        private void Awake()
        {
            if (InitializeOnAwake)
                Initialize();
        }

        private void OnDestroy()
        {
            releaseTextures();
        }

        private void releaseTextures()
        {
            if (TextureChannels != null) {
                foreach (var texture in TextureChannels.Values)
                    texture.Release();
                TextureChannels.Clear();
            }
        }

        #endregion Private
    }

    #region FluidCanvas Types

    public enum InitializationState
    {
        [Tooltip("Not yet initialized.")]
        UNINITIALIZED,

        [Tooltip("Currently being initialized.")]
        INITIALIZING,

        [Tooltip("Fully initialized.")]
        INITIALIZED
    }

    public enum UVSet
    {
        [Tooltip("Default uv set.")]
        UV0,

        [Tooltip("Secondary/lightmap uv set.")]
        UV1
    }

    public enum AtlasConfiguration
    {
        [Tooltip("A new tile in the atlas is created for this renderer.")]
        NewTile,

        [Tooltip("This renderer is drawn to the same atlas tile, as the previous renderer.")]
        CombineWithLast
    }

    [System.Serializable]
    public struct RenderTargetDescriptor
    {
        [Tooltip("Target renderer component.")]
        public Renderer Renderer;

        [Tooltip("Optional cache for the model drawn by the renderer. " +
            "When left empty, the model is processed at runtime, which can slow down initialization.")]
        public FFModelCache ModelCache;

        [Tooltip("Submeshes of the renderer used.")]
        public int SubmeshMask;

        [Tooltip("Set the UV map of the model used for painting. The UV map needs to be bijective (non-overlapping UV-islands).")]
        public UVSet UVSet;

        [Tooltip("Placement of this renderer in the texture atlas.")]
        public AtlasConfiguration AtlasConfiguration;
    }

    [System.Serializable]
    public struct TextureChannelDescriptor
    {
        private const char AliasSeparator = '|';

        [Tooltip("Channel count and precision of the texture.")]
        public RenderTextureFormatDescriptor Format;

        [Tooltip("Name of the shader property, controlled by this TextureChannel.")]
        public string TextureProperty; // TODO: separate into TexturePropertyNameWithAliases type?

        [Tooltip("Initial color of the texture.")]
        public InitializationMode Initialization;

        public TextureChannelDescriptor(RenderTextureFormatDescriptor format, string textureProperty, InitializationMode initializationMode)
        {
            Format = format;
            TextureProperty = textureProperty;
            Initialization = initializationMode;
        }

        public string MainTexturePropertyName()
        {
            for (int index = 0; index < TextureProperty.Length; index++)
                if (TextureProperty[index] == AliasSeparator) {
                    return TextureProperty.Substring(0, index);
                }
            return TextureProperty;
        }

        public TextureChannelIdentifier MainTextureProperty()
        {
            return MainTexturePropertyName();
        }

        public HashSet<TextureChannelIdentifier> TexturePropertyAliases()
        {
            int start = 0;
            var aliases = new HashSet<TextureChannelIdentifier>();
            for (int index = 0; index < TextureProperty.Length; index++) {
                if (TextureProperty[index] == AliasSeparator) {
                    aliases.Add(TextureProperty.Substring(start, index - start));
                    start = index + 1;
                }
            }
            if (start != TextureProperty.Length || TextureProperty.Length == 0)
                aliases.Add(TextureProperty.Substring(start));
            return aliases;
        }

        public string TexturePropertyNameForAlias(TextureChannelIdentifier alias)
        {
            int start = 0;
            for (int index = 0; index < TextureProperty.Length; index++) {
                if (TextureProperty[index] == AliasSeparator) {
                    var name = TextureProperty.Substring(start, index - start);
                    if (Shader.PropertyToID(name) == alias)
                        return name;
                }
            }
            return TextureProperty.Substring(start);
        }

        public enum InitializationMode
        {
            [Tooltip("The content of the texture channel is copied from the current texture in the defined texture channel.")]
            COPY,

            [Tooltip("The texture is initialized with a black color (0, 0, 0, 0).")]
            BLACK,

            [Tooltip("The texture is initialized with a gray color (.5, .5, .5, .5).")]
            GRAY,

            [Tooltip("The texture is initialized with a white color (1, 1, 1, 1).")]
            WHITE,

            [Tooltip("The texture is initialized with a default bump map (.5, .5, 1, .5).")]
            BUMP,

            [Tooltip("The texture is initialized with a red color (1, 0, 0, 0).")]
            RED
        }
    }

    public struct TextureChannelIdentifier
    {
        public readonly int ID;

        private TextureChannelIdentifier(int id)
        {
            ID = id;
        }

        public static implicit operator TextureChannelIdentifier(int id)
        {
            return new TextureChannelIdentifier(id);
        }

        public static implicit operator TextureChannelIdentifier(string name)
        {
            return new TextureChannelIdentifier(Shader.PropertyToID(name));
        }

        public static implicit operator int(TextureChannelIdentifier identifier)
        {
            return identifier.ID;
        }
    }

    public class RenderTarget
    {
        public readonly Renderer Renderer;
        public readonly Mesh Mesh;
        public readonly int SubmeshMask;
        public readonly UVSet UVSet;
        public readonly Vector4 AtlasTransform;

        public RenderTarget(Renderer renderer, Mesh mesh, int submeshMask, UVSet uvSet, Vector4 atlasTransform)
        {
            Renderer = renderer;
            Mesh = mesh;
            SubmeshMask = submeshMask;
            UVSet = uvSet;
            AtlasTransform = atlasTransform;
        }
    }

    #endregion FluidCanvas Types

    #region Helpers

    public struct PaintScope : System.IDisposable
    {
        public readonly RenderTexture Target;
        public readonly bool IsValid;
        private readonly FFCanvas canvas;
        private readonly int channelId;
        private readonly bool notify;

        public PaintScope(FFCanvas canvas, TextureChannelIdentifier channelId, bool notify)
        {
            this.canvas = canvas;
            this.channelId = channelId;
            this.notify = notify;
            IsValid = canvas.TextureChannels.TryGetValue(this.channelId, out Target);
        }

        public void Dispose()
        {
            if (IsValid && notify)
                canvas.OnTextureChannelUpdated.Invoke(channelId);
        }
    }

    public static class FFCanvasUtil
    {
        public struct AtlasDescriptor
        {
            public RenderTargetDescriptor RenderTargetDescriptor;
            public Vector4 AtlasTransform;

            public AtlasDescriptor(RenderTargetDescriptor renderTargetDescriptor, Vector4 atlasTransform)
            {
                RenderTargetDescriptor = renderTargetDescriptor;
                AtlasTransform = atlasTransform;
            }
        }

        /// <summary>
        /// Convenience function for creating a disposable PaintScope, for painting on a TextureChannel of a FFCanvas.
        /// </summary>
        public static PaintScope BeginPaintScope(this FFCanvas canvas, TextureChannelIdentifier channelId, bool notify = true)
        {
            return new PaintScope(canvas, channelId, notify);
        }

        /// <summary>
        /// Map RenderTargetDescriptors to their respective texture atlas tile.
        /// </summary>
        public static IEnumerable<AtlasDescriptor> EnumerateAtlas(this List<RenderTargetDescriptor> renderers, float padding)
        {
            if (renderers.Count == 0)
                yield break;
            var currentTile = renderers[0].AtlasConfiguration == AtlasConfiguration.CombineWithLast ? 1 : 0;
            var size = Mathf.CeilToInt(Mathf.Sqrt(renderers.AtlasTileCount()));
            var sizeInv = 1f / size;
            foreach (var descriptor in renderers) {
                currentTile += (descriptor.AtlasConfiguration == AtlasConfiguration.NewTile) ? 1 : 0;
                yield return new AtlasDescriptor(descriptor, new Vector4(
                        (currentTile - 1) % size * sizeInv + padding,
                        (int)((currentTile - 1) * sizeInv) * sizeInv + padding,
                        sizeInv - padding * 2,
                        sizeInv - padding * 2));
            }
        }

        /// <summary>
        /// Number of texture atlas tiles required for a list of RenderTargetDescriptors.
        /// </summary>
        public static int AtlasTileCount(this List<RenderTargetDescriptor> renderers)
        {
            if (renderers.Count == 0)
                return 0;
            var tiles = renderers.Count(renderer => renderer.AtlasConfiguration == AtlasConfiguration.NewTile);
            if (renderers[0].AtlasConfiguration == AtlasConfiguration.CombineWithLast)
                tiles++;
            return tiles;
        }
    }

    #endregion Helpers
}