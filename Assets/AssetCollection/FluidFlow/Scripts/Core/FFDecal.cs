using System.Collections.Generic;
using UnityEngine;

namespace FluidFlow
{
    [System.Serializable]
    public struct FFDecal
    {
        [Tooltip("Optional mask for masking the decal." +
            "When no mask texture is set, no mask is applied.")]
        public Mask MaskChannel;

        [Tooltip("Texture channels of a canvas affected by this decal.")]
        public Channel[] Channels;

        public FFDecal(Channel channel)
        {
            MaskChannel = new Mask();
            Channels = new Channel[] { channel };
        }

        public FFDecal(Mask maskChannel, Channel channel)
        {
            MaskChannel = maskChannel;
            Channels = new Channel[] { channel };
        }

        public FFDecal(params Channel[] channels)
        {
            MaskChannel = new Mask();
            Channels = channels;
        }

        public FFDecal(Mask maskChannel, params Channel[] channels)
        {
            MaskChannel = maskChannel;
            Channels = channels;
        }

        // allow implicitly converting a channel to a decal without a mask.
        public static implicit operator FFDecal(Channel channel)
        {
            return new FFDecal(new Channel[] { channel });
        }

        // allow implicitly converting an array of channels to a multi-channel decal without a mask.
        public static implicit operator FFDecal(Channel[] channels)
        {
            return new FFDecal(channels);
        }

        [System.Serializable]
        public struct Channel
        {
            public enum Type
            {
                [Tooltip("Channel contains color information.")]
                COLOR,

                [Tooltip("Channel contains a normal map.")]
                NORMAL,

                [Tooltip("Channel contains fluid information.")]
                FLUID
            };

            [Tooltip("Shader property name of the target texture.")]
            public string Property;

            [Tooltip("Type of this decal channel.")]
            public Type ChannelType;

            [Tooltip("Set where this decal channel samples its color from.")]
            public ColorSource Source;

            [Tooltip("Channel type dependent data.")]
            public float Data;

            public static Channel Color(string property, ColorSource source)
            {
                return new Channel() {
                    Property = property,
                    ChannelType = Type.COLOR,
                    Source = source
                };
            }

            public static Channel Fluid(string property, ColorSource source, float amount = 1)
            {
                return new Channel() {
                    Property = property,
                    ChannelType = Type.FLUID,
                    Source = source,
                    Data = amount
                };
            }

            public static Channel Normal(string property, Texture normal, float amount = 1)
            {
                return new Channel() {
                    Property = property,
                    ChannelType = Type.NORMAL,
                    Source = normal,
                    Data = amount
                };
            }
        }

        [System.Serializable]
        public struct ColorSource
        {
            public enum Type
            {
                [Tooltip("Color sampled from a texture.")]
                TEXTURE,

                [Tooltip("Single color.")]
                COLOR
            }

            [Tooltip("What is used as the color source?")]
            public Type SourceType;

            [Tooltip("Texture used for as the color source.")]
            public Texture Texture;

            [Tooltip("Solid color used as the color source.")]
            public Color Color;

            // allow implicitly converting a color to a color source
            public static implicit operator ColorSource(Color color)
            {
                return new ColorSource() {
                    SourceType = Type.COLOR,
                    Color = color
                };
            }

            // allow implicitly converting a texture to a color source
            public static implicit operator ColorSource(Texture texture)
            {
                return new ColorSource() {
                    SourceType = Type.TEXTURE,
                    Texture = texture
                };
            }
        }

        [System.Serializable]
        public struct Mask
        {
            [Tooltip("Texture defining the shape of the mask.")]
            public Texture Texture;

            [System.Flags]
            public enum Component
            {
                R = 1 << 0,
                G = 1 << 1,
                B = 1 << 2,
                A = 1 << 3
            }

            [Tooltip("Color channels of the mask texture used for masking.")]
            public Component Components;

            public Vector4 ComponentMask()
            {
                return new Vector4() {
                    x = Components.HasFlag(Component.R) ? 1 : 0,
                    y = Components.HasFlag(Component.G) ? 1 : 0,
                    z = Components.HasFlag(Component.B) ? 1 : 0,
                    w = Components.HasFlag(Component.A) ? 1 : 0,
                };
            }

            public static Mask AlphaMask(Texture texture)
            {
                return new Mask() {
                    Texture = texture,
                    Components = Component.A
                };
            }

            public static Mask TextureMask(Texture texture, Component components)
            {
                return new Mask() {
                    Texture = texture,
                    Components = components
                };
            }

            public static Mask None()
            {
                return new Mask() {
                    Components = 0
                };
            }
        }
    }
}