using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluidFlow;

public static class FFCanvasExtensions
{
    /// <summary>
    /// Readback and save texture channel of a FFCanvas as a png
    /// </summary>
    public static void SaveTextureChannel(this FFCanvas canvas, TextureChannelIdentifier identifier, string path, TextureFormat destinationFormat = TextureFormat.ARGB32)
    {
        IEnumerator save()
        {
            var request = canvas.TextureChannels[identifier].RequestReadback(destinationFormat);
            yield return request;
            request.Result(false).SaveAsPNG(path);
        };
        canvas.StartCoroutine(save());
    }

    private static readonly MaterialCache QueryPaint =
            new MaterialCache(InternalShaders.internalShadersRootPath + "/Extensions/QueryPaint",
                InternalShaders.setSecondaryUV);

    private static readonly MaterialCache QueryPaintConvolution =
            new MaterialCache(InternalShaders.internalShadersRootPath + "/Extensions/QueryPaintConvolution");

    public static float QueryPaintApplied(this FFCanvas canvas, TextureChannelIdentifier identifier, Color matchMin, Color matchMax)
    {
        if (canvas.TextureChannels.TryGetValue(identifier, out RenderTexture target)) {
            using (RestoreRenderTarget.RestoreActive()) {
                int texSize = target.width;
                var format = RenderTextureFormatDescriptor.GetRenderTextureFormat(ChannelSetup.R, ChannelDescriptor.FLOAT);
                var tmpA = RenderTexture.GetTemporary(texSize, texSize, 0, format);
                if (!tmpA.IsCreated())
                    tmpA.Create();
                Graphics.SetRenderTarget(tmpA);
                GL.Clear(false, true, new Color(-1, -1, -1, -1));
                Shader.SetGlobalTexture(InternalShaders.MainTexPropertyID, target);
                Shader.SetGlobalColor("_FF_Min", matchMin);
                Shader.SetGlobalColor("_FF_Max", matchMax);
                canvas.RenderTargets.DrawMeshes(QueryPaint);
                // convolute to make readback faster
                for (texSize /= 2; texSize >= 1; texSize /= 2) {
                    var tmpB = RenderTexture.GetTemporary(texSize, texSize, 0, format);
                    if (!tmpB.IsCreated())
                        tmpB.Create();
                    Graphics.Blit(tmpA, tmpB, QueryPaintConvolution);
                    RenderTexture.ReleaseTemporary(tmpA);
                    tmpA = tmpB;
                }
                var readback = tmpA.RequestReadback(TextureFormat.RFloat, true);
                RenderTexture.ReleaseTemporary(tmpA);
                return readback.Result(false).GetPixel(0, 0).r;
            }
        } else {
            return 0;
        }
    }
}