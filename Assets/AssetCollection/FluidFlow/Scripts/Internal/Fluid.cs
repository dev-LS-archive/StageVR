using System.Collections.Generic;
using UnityEngine;

namespace FluidFlow
{
    public static class Fluid
    {
        public static Material FlowTextureVariant(MaterialCache cache, RenderTexture rt)
        {
            return cache.Get(Utility.SetBit(0, rt.format == RenderTextureFormat.ARGB32));
        }

        /// <summary>
        /// Move fluid in a fluid texture one step depending on a gravity-based flow map.
        /// </summary>
        public static void Simulate(RenderTexture target, RenderTexture flowTex, Vector2 fluidRetained)
        {
            using (RestoreRenderTarget.RestoreActive()) {
                using (target.SetTemporaryFilterMode(FilterMode.Point)) {
                    Shader.SetGlobalTexture(InternalShaders.FlowTexPropertyID, flowTex);
                    Shader.SetGlobalVector(InternalShaders.FluidRetainedPropertyID, fluidRetained);
                    Shader.SetGlobalFloat(InternalShaders.FluidRetainedInvPropertyID, 1.0f / fluidRetained.x);
                    using (var cpy = new TmpRenderTexture(target.descriptor)) {
                        Graphics.Blit(target, cpy, FlowTextureVariant(InternalShaders.FluidStitchSeams, flowTex));
                        Graphics.Blit(cpy, target, FlowTextureVariant(InternalShaders.FluidSimulate, flowTex));
                    }
                }
            }
        }

        /// <summary>
        /// Reduce overall fluid amount of each pixel of a fluid texture by a specified amount.
        /// </summary>
        public static void Fade(RenderTexture target, float amount)
        {
            using (RestoreRenderTarget.RestoreActive()) {
                using (var tmp = new TmpRenderTexture(target.descriptor)) {
                    Graphics.Blit(target, tmp);
                    Shader.SetGlobalFloat(InternalShaders.AmountPropertyID, amount);
                    Graphics.Blit(tmp, target, InternalShaders.FluidFade);
                }
            }
        }
    }
}