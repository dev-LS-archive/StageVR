using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Threading;

namespace FluidFlow
{
    public static class Utility
    {
        public static void DebugTexture(this Texture tex)
        {
            var id = "Debug_" + tex.name + "_" + tex.GetInstanceID();
            var debugGO = GameObject.Find(id);
            if (!debugGO) {
                debugGO = GameObject.CreatePrimitive(PrimitiveType.Plane);
                debugGO.name = id;
            }
            debugGO.GetComponent<Renderer>().material.mainTexture = tex;
        }

        public static void DebugFrustum(Matrix4x4 frustum)
        {
            Matrix4x4 inv = frustum.inverse;
            var aaa = inv.MultiplyPoint(new Vector3(1, 1, 1));
            var aab = inv.MultiplyPoint(new Vector3(1, 1, -1));
            var aba = inv.MultiplyPoint(new Vector3(1, -1, 1));
            var abb = inv.MultiplyPoint(new Vector3(1, -1, -1));
            var baa = inv.MultiplyPoint(new Vector3(-1, 1, 1));
            var bab = inv.MultiplyPoint(new Vector3(-1, 1, -1));
            var bba = inv.MultiplyPoint(new Vector3(-1, -1, 1));
            var bbb = inv.MultiplyPoint(new Vector3(-1, -1, -1));

            UnityEngine.Debug.DrawLine(aaa, aba);
            UnityEngine.Debug.DrawLine(aba, bba);
            UnityEngine.Debug.DrawLine(bba, baa);
            UnityEngine.Debug.DrawLine(baa, aaa);
            UnityEngine.Debug.DrawLine(aab, abb);
            UnityEngine.Debug.DrawLine(abb, bbb);
            UnityEngine.Debug.DrawLine(bbb, bab);
            UnityEngine.Debug.DrawLine(bab, aab);
            UnityEngine.Debug.DrawLine(aaa, aab);
            UnityEngine.Debug.DrawLine(aba, abb);
            UnityEngine.Debug.DrawLine(baa, bab);
            UnityEngine.Debug.DrawLine(bba, bbb);
        }

        public static Matrix4x4 OrthogonalProjector(Transform transform, float width, float height, float near, float far)
        {
            return Matrix4x4.Ortho(-width * .5f, width * .5f, -height * .5f, height * .5f, near, far) * transform.worldToLocalMatrix;
        }

        public static Matrix4x4 PerspectiveProjector(Transform transform, float fov, float aspect, float near, float far)
        {
            return Matrix4x4.Perspective(fov, aspect, near, far) * transform.worldToLocalMatrix;
        }

        public static Mesh GetMesh(this Renderer renderer)
        {
            if (renderer is SkinnedMeshRenderer)
                return (renderer as SkinnedMeshRenderer).sharedMesh;
            else if (renderer is MeshRenderer) {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter != null)
                    return filter.sharedMesh;
            }
            return null;
        }

        public static void SetMesh(this Renderer renderer, Mesh mesh)
        {
            if (renderer is SkinnedMeshRenderer)
                (renderer as SkinnedMeshRenderer).sharedMesh = mesh;
            else if (renderer is MeshRenderer) {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter)
                    filter.sharedMesh = mesh;
            }
        }

        public static int[] GetSubmeshIndices(this Mesh mesh, int submeshes)
        {
            var submeshList = new List<Tuple<int, SubMeshDescriptor>>();
            foreach (var submeshIndex in submeshes.EnumerateSetBits()) {
                if (submeshIndex >= mesh.subMeshCount)
                    break;
                submeshList.Add(new Tuple<int, SubMeshDescriptor>(submeshIndex, mesh.GetSubMesh(submeshIndex)));
            }
            var indices = new int[submeshList.Sum(submesh => submesh.Item2.indexCount)];
            int index = 0;
            foreach (var submesh in submeshList) {
                Array.Copy(mesh.GetTriangles(submesh.Item1), 0, indices, index, submesh.Item2.indexCount);
                index += submesh.Item2.indexCount;
            }
            return indices;
        }

        public static float ToPolar(this Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x);
        }

        public static Mesh CopyMesh(Mesh source)
        {
            var copy = Mesh.Instantiate(source);
            copy.name = source.name + "_FF_Copy";
            return copy;
        }

        public static void OverwriteExistingMesh(Mesh data, Mesh target)
        {
            Mesh.ApplyAndDisposeWritableMeshData(Mesh.AcquireReadOnlyMeshData(data), target);
        }

        public static int GetTriangeCount(this Mesh.MeshData meshData)
        {
            int triangleCount = 0;
            for (int i = 0; i < meshData.subMeshCount; i++)
                triangleCount += meshData.GetSubMesh(i).indexCount;
            return triangleCount;
        }

        public static Rect AtlasTileViewport(Vector4 AtlasTransform, int texSize)
        {
            return new Rect(new Vector2(AtlasTransform.x * texSize, AtlasTransform.y * texSize),
                            new Vector2(AtlasTransform.z * texSize, AtlasTransform.z * texSize));
        }

        public static void GetTrianges(this Mesh.MeshData meshData, Unity.Collections.NativeArray<int> triangles)
        {
            for (int i = 0; i < meshData.subMeshCount; i++) {
                var descr = meshData.GetSubMesh(i);
                meshData.GetIndices(triangles.GetSubArray(descr.indexStart, descr.indexCount), i);
            }
        }

        public static Vector2[] GetUVSet(this Mesh mesh, UVSet uvSet)
        {
            return uvSet == UVSet.UV1 ? mesh.uv2 : mesh.uv;
        }

        public static int ValidateSubmeshMask(this Mesh mesh, int submeshMask)
        {
            int validatedMask = 0;
            foreach (var flag in submeshMask.EnumerateSetBits())
                if (flag < mesh.subMeshCount)
                    validatedMask |= (1 << flag);
            return validatedMask;
        }

        public static int ValidateSubmeshMask(this Material[] materials, int submeshMask)
        {
            int validatedMask = 0;
            foreach (var flag in submeshMask.EnumerateSetBits())
                if (flag < materials.Length && materials[flag] != null)
                    validatedMask |= (1 << flag);
            return validatedMask;
        }

        public static IEnumerable<int> EnumerateSetBits(this int flags)
        {
            for (int i = 0; i < 32; i++) {
                if (flags.IsBitSet(i))
                    yield return i;
            }
        }

        public static bool IsBitSet(this int flags, int index)
        {
            return (flags & (1 << index)) != 0;
        }

        public static int SetBit(int index, bool enabled)
        {
            return (enabled ? 1 : 0) << index;
        }

        public static void SetKeyword(this Material material, string keyword, bool enabled)
        {
            if (enabled)
                material.EnableKeyword(keyword);
            else
                material.DisableKeyword(keyword);
        }

        public static Vector4 GetTexelSize(this RenderTexture texture)
        {
            return new Vector4(1.0f / texture.width, 1.0f / texture.height, texture.width, texture.height);
        }

        public static float ManhattanDistance(this Vector4 vec)
        {
            return Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z) + Mathf.Abs(vec.w);
        }

        public static ScopedTextureFiltering SetTemporaryFilterMode(this Texture tex, FilterMode filterMode)
        {
            return new ScopedTextureFiltering(tex, filterMode);
        }

        public static Texture2D GetDefaultTexture(this Material material, string texturePropertyName)
        {
            var shader = material.shader;
            var index = shader.FindPropertyIndex(texturePropertyName);
            if (index >= 0) {
                // according to unity's documentation, this are the only valid default textures, all other strings default to gray
                // https://docs.unity3d.com/Manual/SL-Properties.html
                switch (shader.GetPropertyTextureDefaultName(index)) {
                    case "white":
                        return Texture2D.whiteTexture;

                    case "black":
                        return Texture2D.blackTexture;

                    case "bump":
                        return Texture2D.normalTexture;

                    case "red":
                        return Texture2D.redTexture;

                    case "gray":
                    default:
                        return Texture2D.grayTexture;
                }
            }
            return Texture2D.grayTexture;
        }

        public static ReadbackRenderTextureRequest RequestReadback(this RenderTexture rt, TextureFormat destinationFormat = TextureFormat.RGBA32, bool forceNonAsync = false)
        {
            return new ReadbackRenderTextureRequest(rt, destinationFormat, forceNonAsync);
        }

        public static void SaveAsPNG(this Texture2D texture, string path)
        {
            System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        }
    }

    public class AsyncExecute : CustomYieldInstruction
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        public override bool keepWaiting {
            get { return false; }
        }

        public AsyncExecute(ThreadStart threadStart)
        {
            if (threadStart != null)
                threadStart.Invoke();
        }
#else
        private readonly Thread thread;

        public override bool keepWaiting {
            get { return thread.IsAlive; }
        }

        public AsyncExecute(ThreadStart threadStart)
        {
            thread = new Thread(threadStart);
            thread.Start();
        }

#endif
    }

    public class ReadbackRenderTextureRequest : IEnumerator
    {
        private bool requestRunning = true;
        private bool success = false;
        private RenderTexture source;
        private Texture2D result;

        public Texture2D Result(bool apply = true)
        {
            if (!success) {
                // no valid result yet? -> read texture blocking
                using (RestoreRenderTarget.RestoreActive()) {
                    Graphics.SetRenderTarget(source);
                    result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                    success = true;
                    requestRunning = false;
                }
            }
            if (apply)
                result.Apply();
            return result;
        }

        public ReadbackRenderTextureRequest(RenderTexture source, TextureFormat format, bool forceNonAsync = false)
        {
            this.source = source;
            result = new Texture2D(source.width, source.height, format, false);
            result.filterMode = source.filterMode;
            if (!forceNonAsync && SystemInfo.supportsAsyncGPUReadback) {
                AsyncGPUReadback.Request(source, 0, format, (request) => {
                    success = !request.hasError;
                    if (success)
                        result.LoadRawTextureData(request.GetData<byte>());
                    requestRunning = false;
                });
            } else {
                requestRunning = false;
            }
        }

        public object Current { get { return null; } }

        public bool MoveNext()
        {
            return requestRunning;
        }

        public void Reset()
        {
        }
    }

    public struct ScopedMaterialPropertyBlockEdit : System.IDisposable
    {
        public readonly MaterialPropertyBlock PropertyBlock;
        private readonly Renderer renderer;
        private readonly int index;

        public ScopedMaterialPropertyBlockEdit(Renderer renderer, int index)
        {
            this.renderer = renderer;
            this.index = index;
            PropertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(PropertyBlock, index);
        }

        public void Dispose()
        {
            renderer.SetPropertyBlock(PropertyBlock, index);
        }
    }

    public struct RestoreRenderTarget : System.IDisposable
    {
        private readonly RenderTexture tmp;

        public RestoreRenderTarget(RenderTexture rt)
        {
            tmp = rt;
        }

        public void Dispose()
        {
            RenderTexture.active = tmp;
        }

        public static RestoreRenderTarget RestoreActive()
        {
            return new RestoreRenderTarget(RenderTexture.active);
        }
    }

    public struct ScopedTextureFiltering : System.IDisposable
    {
        private readonly Texture texture;
        private readonly FilterMode tmpFM;

        public ScopedTextureFiltering(Texture tex, FilterMode filterMode)
        {
            texture = tex;
            tmpFM = tex.filterMode;
            tex.filterMode = filterMode;
        }

        public void Dispose()
        {
            texture.filterMode = tmpFM;
        }
    }

    public struct ShaderPropertyIdentifier
    {
        public readonly int ShaderPropertyId;

        public ShaderPropertyIdentifier(string name)
        {
            ShaderPropertyId = Shader.PropertyToID(name);
        }

        public ShaderPropertyIdentifier(int id)
        {
            ShaderPropertyId = id;
        }

        public static implicit operator ShaderPropertyIdentifier(string name)
        {
            return new ShaderPropertyIdentifier(name);
        }

        public static implicit operator int(ShaderPropertyIdentifier identifier)
        {
            return identifier.ShaderPropertyId;
        }
    }
}