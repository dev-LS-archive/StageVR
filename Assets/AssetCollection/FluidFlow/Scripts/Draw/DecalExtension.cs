using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class DecalExtension
    {
        /// <summary>
        /// Project decal onto canvas in world space.
        /// </summary>
        /// <param name="projector">Wrapper around a world space view-projection matrix.</param>
        /// <param name="paintBackfacing">Only paint surfaces pointing towards the projector.</param>
        public static void ProjectDecal(this FFCanvas canvas, FFDecal decal, FFProjector projector, bool paintBackfacing = false)
        {
            Shader.SetGlobalMatrix(InternalShaders.ProjectionPropertyID, projector);
            Shader.SetGlobalFloat(InternalShaders.PaintBackfacingPropertyID, paintBackfacing ? 1 : 0);
            SetupDecalMask(decal.MaskChannel);

            foreach (var channel in decal.Channels) {
                using (var paintScope = canvas.BeginPaintScope(channel.Property)) {
                    if (!paintScope.IsValid)
                        continue;
                    SetupDecalChannel(channel);
                    Shader.SetGlobalTexture(InternalShaders.OtherTexPropertyID, paintScope.Target);
                    using (var command = new CommandBuffer()) {
                        using (var tmp = command.TemporaryRT(paintScope.Target.descriptor)) {
                            command.Blit(paintScope.Target, tmp);
                            command.SetRenderTarget(tmp);
                            command.DrawRenderTargets(canvas.RenderTargets, InternalShaders.ProjectionVariant(channel, decal.MaskChannel.Texture != null));
                            command.Blit(tmp, paintScope.Target);
                        }
                        Graphics.ExecuteCommandBuffer(command);
                    }
                }
            }
        }

        /// <summary>
        /// Draw decal onto canvas in uv space.
        /// </summary>
        public static void DrawDecal(this FFCanvas canvas, FFDecal decal, int renderTargetMask = -1, UVSet decalTextureUVSet = UVSet.UV0)
        {
            var validatedMask = renderTargetMask & ((1 << canvas.RenderTargets.Count) - 1);
            SetupDecalMask(decal.MaskChannel);

            foreach (var channel in decal.Channels) {
                using (var paintScope = canvas.BeginPaintScope(channel.Property)) {
                    if (!paintScope.IsValid)
                        continue;
                    SetupDecalChannel(channel);
                    Shader.SetGlobalTexture(InternalShaders.OtherTexPropertyID, paintScope.Target);
                    using (var tmp = new TmpRenderTexture(paintScope.Target.descriptor)) {
                        Graphics.Blit(paintScope.Target, tmp);
                        foreach (var index in validatedMask.EnumerateSetBits())
                            canvas.RenderTargets[index].DrawMesh(InternalShaders.UVDecalVariant(channel, decal.MaskChannel.Texture != null, decalTextureUVSet));
                        Graphics.Blit(tmp, paintScope.Target);
                    }
                }
            }
        }

        public static void SetupDecalChannel(FFDecal.Channel channel)
        {
            switch (channel.ChannelType) {
                case FFDecal.Channel.Type.NORMAL:
                case FFDecal.Channel.Type.FLUID:
                    Shader.SetGlobalFloat(InternalShaders.DataPropertyID, channel.Data);
                    break;
            }
            SetupColorSource(channel.Source);
        }

        public static void SetupColorSource(FFDecal.ColorSource source)
        {
            switch (source.SourceType) {
                case FFDecal.ColorSource.Type.TEXTURE:
                    Shader.SetGlobalTexture(InternalShaders.DecalTexPropertyID, source.Texture);
                    break;

                case FFDecal.ColorSource.Type.COLOR:
                    Shader.SetGlobalColor(InternalShaders.ColorPropertyID, source.Color);
                    break;
            }
        }

        public static void SetupDecalMask(FFDecal.Mask mask)
        {
            if (mask.Texture) {
                Shader.SetGlobalTexture(InternalShaders.MaskTexPropertyID, mask.Texture);
                Shader.SetGlobalVector(InternalShaders.MaskComponentsPropertyID, mask.ComponentMask());
                var manhattan = mask.ComponentMask().ManhattanDistance();
                Shader.SetGlobalFloat(InternalShaders.MaskComponentsInvPropertyID, manhattan == 0 ? 1 : (1f / manhattan));
            }
        }
    }

    public struct FFProjector
    {
        public readonly Matrix4x4 ViewProjection;

        public FFProjector(Matrix4x4 matrix)
        {
            ViewProjection = matrix;
        }

        // allow implicitly converting a projector to a view-projection matrix.
        public static implicit operator Matrix4x4(FFProjector projector)
        {
            return projector.ViewProjection;
        }

        // allow implicitly converting a matrix to a projector.
        public static implicit operator FFProjector(Matrix4x4 matrix)
        {
            return new FFProjector(matrix);
        }

        /// <summary>
        /// Convenience function for creating a orthogonal view-projection from a transform, pointing in local -z direction.
        /// </summary>
        public static FFProjector Orthogonal(Transform transform, float width, float height, float near, float far)
        {
            return Matrix4x4.Ortho(-width * .5f, width * .5f, -height * .5f, height * .5f, near, far) * transform.worldToLocalMatrix;
        }

        /// <summary>
        /// Convenience function for creating a perspective view-projection from a transform, pointing in local -z direction.
        /// </summary>
        public static FFProjector Perspective(Transform transform, float fov, float aspect, float near, float far)
        {
            return Matrix4x4.Perspective(fov, aspect, near, far) * transform.worldToLocalMatrix;
        }
    }
}