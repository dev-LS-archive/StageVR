using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class InternalShaders
    {
        #region Shader Property Cache

        public static readonly ShaderPropertyIdentifier MainTexPropertyID = "_FF_MainTex";
        public static readonly ShaderPropertyIdentifier OtherTexPropertyID = "_FF_OtherTex";
        public static readonly ShaderPropertyIdentifier DecalTexPropertyID = "_FF_DecalTex";
        public static readonly ShaderPropertyIdentifier MaskTexPropertyID = "_FF_MaskTex";
        public static readonly ShaderPropertyIdentifier SeamTexPropertyID = "_FF_SeamTex";
        public static readonly ShaderPropertyIdentifier FlowTexPropertyID = "_FF_FlowTex";
        public static readonly ShaderPropertyIdentifier NormalTexPropertyID = "_FF_NormalTex";

        public static readonly ShaderPropertyIdentifier ProjectionPropertyID = "_FF_Projection";
        public static readonly ShaderPropertyIdentifier PaintBackfacingPropertyID = "_FF_PaintBackfacingSurface";
        public static readonly ShaderPropertyIdentifier DataPropertyID = "_FF_Data";
        public static readonly ShaderPropertyIdentifier ColorPropertyID = "_FF_Color";
        public static readonly ShaderPropertyIdentifier MaskComponentsPropertyID = "_FF_MaskComponents";
        public static readonly ShaderPropertyIdentifier MaskComponentsInvPropertyID = "_FF_MaskComponentsInv";
        public static readonly ShaderPropertyIdentifier AtlasTransformPropertyID = "_FF_AtlasTransform";
        public static readonly ShaderPropertyIdentifier TexelSizePropertyID = "_FF_TexelSize";
        public static readonly ShaderPropertyIdentifier FluidRetainedPropertyID = "_FF_FluidRetained";
        public static readonly ShaderPropertyIdentifier FluidRetainedInvPropertyID = "_FF_FluidRetainedInv";
        public static readonly ShaderPropertyIdentifier PositionPropertyID = "_FF_Position";
        public static readonly ShaderPropertyIdentifier LinePropertyID = "_FF_Line";
        public static readonly ShaderPropertyIdentifier NormalPropertyID = "_FF_Normal";
        public static readonly ShaderPropertyIdentifier RadiusPropertyID = "_FF_Radius";
        public static readonly ShaderPropertyIdentifier RadiusInvPropertyID = "_FF_RadiusInv";
        public static readonly ShaderPropertyIdentifier ThicknessInvPropertyID = "_FF_ThicknessInv";
        public static readonly ShaderPropertyIdentifier AmountPropertyID = "_FF_Amount";
        public static readonly ShaderPropertyIdentifier FadePropertyID = "_FF_Fade";
        public static readonly ShaderPropertyIdentifier FadeInvPropertyID = "_FF_FadeInv";
        public static readonly ShaderPropertyIdentifier NormalStrengthPropertyID = "_FF_NormalStrength";
        public static readonly ShaderPropertyIdentifier GravityPropertyID = "_FF_Gravity";

        #endregion Shader Property Cache

        #region Shader Name Defines

        public static readonly string AtlasTransformPropertyName = "_FF_AtlasTransform";
        public static readonly string SecondaryUVKeyword = "FF_UV1";
        public static readonly string internalShadersRootPath = "Hidden/FluidFlow";

        #endregion Shader Name Defines

        #region Misc Shaders

        public static Material TextureInitializationVariant(bool useSecondaryUV)
        {
            return TextureInitialization.Get(Utility.SetBit(0, useSecondaryUV));
        }

        public static readonly MaterialCache TextureInitialization =
            new MaterialCache(internalShadersRootPath + "/TextureInitialization",
                setSecondaryUV);

        #endregion Misc Shaders

        #region Seam-fix Shaders

        public static readonly MaterialCache UVUnwrap =
            new MaterialCache(internalShadersRootPath + "/UVUnwrap",
                setSecondaryUV);

        public static readonly MaterialCache EncodePadding =
            new MaterialCache(internalShadersRootPath + "/EncodePadding");

        public static Material ApplyPaddingVariant(bool precalculated)
        {
            return ApplyPadding.Get(Utility.SetBit(0, precalculated));
        }

        public static readonly MaterialCache ApplyPadding =
            new MaterialCache(internalShadersRootPath + "/ApplyPadding",
                setKeyword("PRECALCULATED_OFFSET"));

        #endregion Seam-fix Shaders

        #region Draw Shaders

        public static PerRenderTargetVariant ProjectionVariant(FFDecal.Channel channel, bool useMask)
        {
            return new PerRenderTargetVariant(Projection, Utility.SetBit(1, useMask)
                          | Utility.SetBit(2, channel.ChannelType == FFDecal.Channel.Type.FLUID)
                          | Utility.SetBit(3, channel.ChannelType == FFDecal.Channel.Type.NORMAL)
                          | Utility.SetBit(4, channel.Source.SourceType == FFDecal.ColorSource.Type.COLOR));
        }

        public static readonly MaterialCache Projection =
            new MaterialCache(internalShadersRootPath + "/Draw/Projection",
                setSecondaryUV,
                setKeyword("FF_MASK"),
                setKeyword("FF_FLUID"),
                setKeyword("FF_NORMAL"),
                setKeyword("FF_COLOR"));

        public static PerRenderTargetVariant UVDecalVariant(FFDecal.Channel channel, bool useMask, UVSet sourceUVSet)
        {
            return new PerRenderTargetVariant(UVDecal,
                            Utility.SetBit(1, sourceUVSet == UVSet.UV1)
                          | Utility.SetBit(2, useMask)
                          | Utility.SetBit(3, channel.ChannelType == FFDecal.Channel.Type.FLUID)
                          | Utility.SetBit(4, channel.ChannelType == FFDecal.Channel.Type.NORMAL)
                          | Utility.SetBit(5, channel.Source.SourceType == FFDecal.ColorSource.Type.COLOR));
        }

        public static readonly MaterialCache UVDecal =
            new MaterialCache(internalShadersRootPath + "/Draw/UVDecal",
                setSecondaryUV,
                setKeyword("FF_SOURCE_UV1"),
                setKeyword("FF_MASK"),
                setKeyword("FF_FLUID"),
                setKeyword("FF_NORMAL"),
                setKeyword("FF_COLOR"));

        public static readonly MaterialCache DrawSphere =
            new MaterialCache(internalShadersRootPath + "/Draw/Sphere",
                setSecondaryUV,
                setKeyword("FF_FLUID"));

        public static readonly MaterialCache DrawDisc =
            new MaterialCache(internalShadersRootPath + "/Draw/Disc",
                setSecondaryUV,
                setKeyword("FF_FLUID"));

        public static readonly MaterialCache DrawCapsule =
            new MaterialCache(internalShadersRootPath + "/Draw/Capsule",
                setSecondaryUV,
                setKeyword("FF_FLUID"));

        /* your custom brush shaders here
        public static readonly MaterialCache DrawCustomBrush =
            new MaterialCache(internalShadersRootPath + "/Draw/CustomBrush",
                setSecondaryUV,
                setKeyword("FF_FLUID"));
        */

        #endregion Draw Shaders

        #region Gravity Shaders

        public static PerRenderTargetVariant GravityVariant(bool useNormal)
        {
            return new PerRenderTargetVariant(Gravity, Utility.SetBit(1, useNormal));
        }

        public static readonly MaterialCache Gravity =
            new MaterialCache(internalShadersRootPath + "/Gravity",
                setSecondaryUV,
                setKeyword("USE_NORMAL"));

        #endregion Gravity Shaders

        #region Flud Simulation Shaders

        public static readonly MaterialCache FluidFade =
            new MaterialCache(internalShadersRootPath + "/Fluid/Fade");

        public static readonly MaterialCache FluidUVSeamStitch =
            new MaterialCache(internalShadersRootPath + "/Fluid/UVSeamStitch",
                setKeyword("FF_FLOWTEX_COMPRESSED"));

        public static readonly MaterialCache FluidUVSeamPadding =
            new MaterialCache(internalShadersRootPath + "/Fluid/UVSeamPadding");

        public static readonly MaterialCache FluidStitchSeams =
            new MaterialCache(internalShadersRootPath + "/Fluid/StitchSeams",
                setKeyword("FF_FLOWTEX_COMPRESSED"));

        public static readonly MaterialCache FluidSimulate =
            new MaterialCache(internalShadersRootPath + "/Fluid/Simulate",
                setKeyword("FF_FLOWTEX_COMPRESSED"));

        #endregion Flud Simulation Shaders

        #region Editor Only Shaders

#if UNITY_EDITOR

        public static readonly MaterialCache UVOverlap =
            new MaterialCache(internalShadersRootPath + "/UVOverlap",
                setSecondaryUV);

        public static readonly MaterialCache MarkOverlap =
            new MaterialCache(internalShadersRootPath + "/MarkOverlap");

#endif

        #endregion Editor Only Shaders

        #region Helpers

        public static void setSecondaryUV(Material material, bool secondaryUV)
        {
            material.SetKeyword(SecondaryUVKeyword, secondaryUV);
        }

        public static MaterialCache.InitializeMaterialVariant setKeyword(string name)
        {
            return (Material m, bool enabled) => m.SetKeyword(name, enabled);
        }

        #endregion Helpers
    }

    public class MaterialCache
    {
        private Shader shader;
        private Material[] materials;
        private InitializeMaterialVariant[] variantInitializers;

        public MaterialCache(string shaderName, params InitializeMaterialVariant[] initializers)
        {
            shader = Shader.Find(shaderName);
            materials = new Material[1 << initializers.Length];
            variantInitializers = initializers;
        }

        ~MaterialCache()
        {
            foreach (var material in materials) {
                if (material != null)
                    Object.Destroy(material);
            }
        }

        public Material Get(int variant = 0)
        {
            if (!materials[variant]) {
                materials[variant] = new Material(shader);
                for (int i = 0; i < variantInitializers.Length; i++)
                    variantInitializers[i].Invoke(materials[variant], variant.IsBitSet(i));
            }
            return materials[variant];
        }

        public static implicit operator Material(MaterialCache wrapper)
        {
            return wrapper.Get();
        }

        public static implicit operator PerRenderTargetVariant(MaterialCache wrapper)
        {
            return new PerRenderTargetVariant(wrapper, 0);
        }

        public delegate void InitializeMaterialVariant(Material material, bool enabled);
    }

    public struct PerRenderTargetVariant
    {
        private readonly MaterialCache cache;
        private readonly int variant;

        public PerRenderTargetVariant(MaterialCache cache, int variant)
        {
            this.cache = cache;
            this.variant = variant;
        }

        public Material Get(UVSet uvSet)
        {
            return cache.Get(variant + Utility.SetBit(0, uvSet == UVSet.UV1));
        }
    }
}