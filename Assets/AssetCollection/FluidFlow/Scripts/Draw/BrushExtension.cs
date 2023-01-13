using UnityEngine;
using UnityEngine.Rendering;

namespace FluidFlow
{
    public static class BrushExtension
    {
        /// <summary>
        /// Draws a 3D sphere brush.
        /// </summary>
        /// <param name="center">Center of the sphere in world space.</param>
        /// <param name="radius">Radius of the sphere.</param>
        public static void DrawSphere(this FFCanvas canvas, TextureChannelIdentifier channel, FFBrush brush, Vector3 center, float radius)
        {
            Shader.SetGlobalVector(InternalShaders.PositionPropertyID, center);
            Shader.SetGlobalFloat(InternalShaders.RadiusInvPropertyID, 1.0f / radius);
            drawBrush(canvas, channel, brush, InternalShaders.DrawSphere);
        }

        /// <summary>
        /// Draws a 3D cylinder brush.
        /// </summary>
        /// <param name="position">Center of the disc in world space.</param>
        /// <param name="normal">Direcition the disc is poining to.</param>
        /// <param name="radius">Radius of the cylinder.</param>
        /// <param name="thickness">Thickness/height of the cylinder in normal direction.</param>
        public static void DrawDisc(this FFCanvas canvas, TextureChannelIdentifier channel, FFBrush brush, Vector3 position, Vector3 normal, float radius, float thickness)
        {
            Shader.SetGlobalVector(InternalShaders.PositionPropertyID, position);
            Shader.SetGlobalVector(InternalShaders.NormalPropertyID, normal.normalized);
            Shader.SetGlobalFloat(InternalShaders.RadiusPropertyID, radius);
            Shader.SetGlobalFloat(InternalShaders.ThicknessInvPropertyID, 1.0f / thickness);
            drawBrush(canvas, channel, brush, InternalShaders.DrawDisc);
        }

        /// <summary>
        /// Draws a 3D capsule brush.
        /// </summary>
        /// <param name="centerA">First center of the capsule in world space.</param>
        /// <param name="centerB">Second center of the capsule in world space.</param>
        /// <param name="radius">Radius of the capsule.</param>
        public static void DrawCapsule(this FFCanvas canvas, TextureChannelIdentifier channel, FFBrush brush, Vector3 centerA, Vector3 centerB, float radius)
        {
            Shader.SetGlobalVector(InternalShaders.PositionPropertyID, centerA);
            var direction = centerB - centerA;
            Shader.SetGlobalVector(InternalShaders.LinePropertyID, new Vector4(direction.x, direction.y, direction.z, 1.0f / direction.sqrMagnitude));
            Shader.SetGlobalFloat(InternalShaders.RadiusInvPropertyID, 1.0f / radius);
            drawBrush(canvas, channel, brush, InternalShaders.DrawCapsule);
        }

        /*
        public static void DrawCustom(this FFCanvas canvas, TextureChannelIdentifier channelId, FFBrush brush, ... )
        {
            // shader variable init ...
            // e.g. Shader.SetGlobalVector("_MyPositionVar", ... );

            // call custom brush shader
            drawBrush(canvas, channelId, brush, InternalShaders.DrawCustomBrush);
        }
        */

        private static void drawBrush(FFCanvas canvas, TextureChannelIdentifier channelId, FFBrush brush, MaterialCache material)
        {
            var materialVariant = brushVariant(brush, material);
            using (var paintScope = canvas.BeginPaintScope(channelId)) {
                if (paintScope.IsValid) {
                    Shader.SetGlobalColor(InternalShaders.ColorPropertyID, brush.Color);
                    Shader.SetGlobalFloat(InternalShaders.DataPropertyID, brush.Data);
                    Shader.SetGlobalFloat(InternalShaders.FadePropertyID, 1.0f - brush.Fade);
                    Shader.SetGlobalFloat(InternalShaders.FadeInvPropertyID, brush.Fade > 0 ? (1.0f / brush.Fade) : 1);
                    Shader.SetGlobalTexture(InternalShaders.OtherTexPropertyID, paintScope.Target);
                    using (var command = new CommandBuffer()) {
                        using (var tmp = command.TemporaryRT(paintScope.Target.descriptor)) {
                            command.Blit(paintScope.Target, tmp);
                            command.SetRenderTarget(tmp);
                            command.DrawRenderTargets(canvas.RenderTargets, materialVariant);
                            command.Blit(tmp, paintScope.Target);
                        }
                        Graphics.ExecuteCommandBuffer(command);
                    }
                }
            }
        }

        private static PerRenderTargetVariant brushVariant(this FFBrush brush, MaterialCache material)
        {
            return new PerRenderTargetVariant(material, Utility.SetBit(1, brush.BrushType == FFBrush.Type.FLUID));
        }
    }
}