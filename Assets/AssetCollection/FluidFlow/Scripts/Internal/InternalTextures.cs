// Uncomment this line if you do not want compression of the internal flow texture. Compression decreases the VRAM consumption, but is not supported on devices without uint bit arithmetic (eg. OpenGLES2/WEBGL)
// #define FF_FLOWTEX_NON_COMPRESSED

using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;

namespace FluidFlow
{
    public static class InternalTextures
    {
#if (UNITY_WEBGL || FF_FLOWTEX_NON_COMPRESSED)
        public static readonly RenderTextureFormatDescriptor FlowFormat = new RenderTextureFormatDescriptor(ChannelSetup.RGBA, ChannelDescriptor.FIXED16);
#else
        public static readonly RenderTextureFormatDescriptor FlowFormat = new RenderTextureFormatDescriptor(ChannelSetup.RGBA, ChannelDescriptor.FIXED8);
#endif
        public static readonly RenderTextureFormatDescriptor FluidFormat = new RenderTextureFormatDescriptor(ChannelSetup.RGBA, ChannelDescriptor.HALF);
        public static readonly RenderTextureFormatDescriptor ColorFormat = new RenderTextureFormatDescriptor(ChannelSetup.RGBA, ChannelDescriptor.FIXED8);
        public static readonly RenderTextureFormatDescriptor MonoFormat = new RenderTextureFormatDescriptor(ChannelSetup.R, ChannelDescriptor.FIXED8);
        public static readonly RenderTextureFormatDescriptor PaddingFormat = new RenderTextureFormatDescriptor(ChannelSetup.R, ChannelDescriptor.FIXED8);

        public static IEnumerable<Tuple<string, RenderTextureFormatDescriptor>> Presets()
        {
            yield return new Tuple<string, RenderTextureFormatDescriptor>("Fluid", FluidFormat);
            yield return new Tuple<string, RenderTextureFormatDescriptor>("Color", ColorFormat);
            yield return new Tuple<string, RenderTextureFormatDescriptor>("Mono", MonoFormat);
        }

        public static RenderTexture CreateRenderTexture(RenderTextureFormatDescriptor type, int size)
        {
            var rt = new RenderTexture(Descriptor(type, size));
            rt.Create();
            return rt;
        }

        public static RenderTextureDescriptor Descriptor(RenderTextureFormatDescriptor type, int size)
        {
            var descr = new RenderTextureDescriptor(size, size, type.TryFindSupportedRenderTextureFormat(), 0, 0);
            descr.autoGenerateMips = false;
            descr.sRGB = false;
            return descr;
        }

        public static TmpCommandBufferRenderTexture TemporaryRT(this CommandBuffer command, RenderTextureDescriptor descr, FilterMode filterMode = FilterMode.Point)
        {
            return new TmpCommandBufferRenderTexture(command, descr, filterMode);
        }
    }

    public enum ChannelSetup
    {
        [Tooltip("Single channel.")]
        R,

        [Tooltip("Two channels.")]
        RG,

        [Tooltip("Four channels.")]
        RGBA
    }

    public enum ChannelDescriptor
    {
        [Tooltip("8bit fixed-point value per channel.")]
        FIXED8,

        [Tooltip("16bit fixed-point value per channel.")]
        FIXED16,

        [Tooltip("16bit floating-point value per channel.")]
        HALF,

        [Tooltip("32bit floating-point value per channel.")]
        FLOAT
    }

    [System.Serializable]
    public struct RenderTextureFormatDescriptor
    {
        [Tooltip("Number of channels.")]
        public ChannelSetup Channels;

        [Tooltip("Precision of each channel.")]
        public ChannelDescriptor Precision;

        public RenderTextureFormatDescriptor(ChannelSetup channels, ChannelDescriptor precision)
        {
            Channels = channels;
            Precision = precision;
        }

        public RenderTextureFormat TryFindSupportedRenderTextureFormat()
        {
            for (int channels = (int)Channels; channels <= (int)ChannelSetup.RGBA; channels++) {
                for (int precision = (int)Precision; precision <= (int)ChannelDescriptor.FLOAT; precision++) {
                    var format = GetRenderTextureFormat((ChannelSetup)channels, (ChannelDescriptor)precision);
                    if (SystemInfo.SupportsRenderTextureFormat(format))
                        return format;
                    Debug.LogWarningFormat("Requested format '{0}' not supported!", format);
                }
            }
            Debug.LogWarning("No supported RenderTextureFormat found. Falling back to default.");
            return RenderTextureFormat.Default;
        }

        public static RenderTextureFormat GetRenderTextureFormat(ChannelSetup channels, ChannelDescriptor precision)
        {
            switch (channels) {
                case ChannelSetup.R:
                    switch (precision) {
                        case ChannelDescriptor.FIXED8:
                            return RenderTextureFormat.R8;

                        case ChannelDescriptor.FIXED16:
                            return RenderTextureFormat.R16;

                        case ChannelDescriptor.HALF:
                            return RenderTextureFormat.RHalf;

                        case ChannelDescriptor.FLOAT:
                        default:
                            return RenderTextureFormat.RFloat;
                    }

                case ChannelSetup.RG:
                    switch (precision) {
                        case ChannelDescriptor.FIXED8:
                            return RenderTextureFormat.RG16;

                        case ChannelDescriptor.FIXED16:
                            return RenderTextureFormat.RG32;

                        case ChannelDescriptor.HALF:
                            return RenderTextureFormat.RGHalf;

                        case ChannelDescriptor.FLOAT:
                        default:
                            return RenderTextureFormat.RGFloat;
                    }

                case ChannelSetup.RGBA:
                default:
                    switch (precision) {
                        case ChannelDescriptor.FIXED8:
                            return RenderTextureFormat.ARGB32;

                        case ChannelDescriptor.FIXED16:
                            return RenderTextureFormat.ARGB64;

                        case ChannelDescriptor.HALF:
                            return RenderTextureFormat.ARGBHalf;

                        case ChannelDescriptor.FLOAT:
                        default:
                            return RenderTextureFormat.ARGBFloat;
                    }
            }
        }
    }

    /// <summary>
    /// RAII for getting/ releasing temporary rendertextures
    /// </summary>
    public struct TmpRenderTexture : System.IDisposable
    {
        private readonly RenderTexture texture;

        public TmpRenderTexture(RenderTextureDescriptor descr)
        {
            texture = create(descr);
        }

        public TmpRenderTexture(RenderTextureFormatDescriptor type, int size)
        {
            texture = create(InternalTextures.Descriptor(type, size));
        }

        private static RenderTexture create(RenderTextureDescriptor descr)
        {
            var texture = RenderTexture.GetTemporary(descr);
            texture.filterMode = FilterMode.Point;
            return texture;
        }

        public static implicit operator RenderTexture(TmpRenderTexture tmpRT)
        {
            return tmpRT.texture;
        }

        public void Dispose()
        {
            RenderTexture.ReleaseTemporary(texture);
        }
    }

    public struct TmpCommandBufferRenderTexture : System.IDisposable
    {
        private static int staticNameId = 0;
        private readonly CommandBuffer commandBuffer;
        private readonly int nameId;

        public TmpCommandBufferRenderTexture(CommandBuffer command, RenderTextureDescriptor descr, FilterMode filterMode = FilterMode.Point)
        {
            commandBuffer = command;
            nameId = staticNameId++;
            commandBuffer.GetTemporaryRT(nameId, descr, filterMode);
        }

        public static implicit operator RenderTargetIdentifier(TmpCommandBufferRenderTexture tmpRT)
        {
            return tmpRT.nameId;
        }

        public void Dispose()
        {
            staticNameId--;
            commandBuffer.ReleaseTemporaryRT(nameId);
        }
    }
}