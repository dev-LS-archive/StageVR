using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;

namespace FluidFlow
{
    public static class Gravity
    {
        public static void GenerateGravityMap(List<RenderTarget> renderers, RenderTexture target, Vector3 worldGravity)
        {
            using (var command = new CommandBuffer()) {
                command.SetRenderTarget(target);
                command.SetGlobalVector(InternalShaders.GravityPropertyID, worldGravity.normalized);
                command.SetGlobalVector(InternalShaders.TexelSizePropertyID, target.GetTexelSize());
                command.DrawRenderTargets(renderers, InternalShaders.GravityVariant(false), false);
                Graphics.ExecuteCommandBuffer(command);
            }
        }

        public static void GenerateGravityMap(List<RenderTarget> renderers, RenderTexture target, Vector3 worldGravity, int normalMap, Texture2D fallback, float normalInfluence)
        {
            using (RestoreRenderTarget.RestoreActive()) {
                Graphics.SetRenderTarget(target);
                Shader.SetGlobalVector(InternalShaders.GravityPropertyID, worldGravity.normalized);
                Shader.SetGlobalVector(InternalShaders.TexelSizePropertyID, target.GetTexelSize());
                Shader.SetGlobalFloat(InternalShaders.NormalStrengthPropertyID, normalInfluence);
                using (var command = new CommandBuffer()) {
                    foreach (var rt in renderers) {
                        var materials = rt.Renderer.sharedMaterials;
                        foreach (var submeshIndex in rt.SubmeshMask.EnumerateSetBits()) {
                            var hasNormalTex = materials[submeshIndex].HasProperty(normalMap);
                            if (hasNormalTex)
                                command.SetGlobalTexture(InternalShaders.NormalTexPropertyID, materials[submeshIndex].GetTexture(normalMap));
                            else if (fallback)
                                command.SetGlobalTexture(InternalShaders.NormalTexPropertyID, fallback);

                            command.DrawRenderer(rt.Renderer, InternalShaders.GravityVariant(hasNormalTex || fallback).Get(rt.UVSet), submeshIndex, 0);
                        }
                    }
                    Graphics.ExecuteCommandBuffer(command);
                }
            }
        }

        public struct CreateSecondaryUVTransformJobGroup : IDisposable
        {
            private List<Mesh> meshes;
            private CreateSecondaryUVTransformJob[] jobs;

            public CreateSecondaryUVTransformJobGroup(List<Mesh> meshes)
            {
                this.meshes = new List<Mesh>(meshes);
                using (var dataArray = Mesh.AcquireReadOnlyMeshData(meshes)) {
                    jobs = new CreateSecondaryUVTransformJob[dataArray.Length];
                    for (int i = 0; i < dataArray.Length; i++)
                        jobs[i] = new CreateSecondaryUVTransformJob(dataArray[i]);
                }
            }

            public void Run()
            {
                foreach (var job in jobs)
                    job.Run();
            }

            public JobHandle[] ScheduleGroup()
            {
                var handles = new JobHandle[jobs.Length];
                for (int i = 0; i < jobs.Length; i++)
                    handles[i] = jobs[i].Schedule();
                return handles;
            }

            public void ApplyResults()
            {
                for (int i = 0; i < jobs.Length; i++)
                    apply(meshes[i], jobs[i]);
            }

            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
            private struct Half4
            {
                public ushort x, y, z, w;

                public static Half4 FromVector4(Vector4 vec)
                {
                    return new Half4() {
                        x = Mathf.FloatToHalf(vec.x),
                        y = Mathf.FloatToHalf(vec.y),
                        z = Mathf.FloatToHalf(vec.z),
                        w = Mathf.FloatToHalf(vec.w)
                    };
                }
            }

            private static void apply(Mesh mesh, CreateSecondaryUVTransformJob job)
            {
                var attr = new List<VertexAttributeDescriptor>();
                mesh.GetVertexAttributes(attr);
                var usedStreams = new bool[4];
                foreach (var attrDescr in attr)
                    usedStreams[attrDescr.stream] = true;
                int unusedStream = 0;
                foreach (var used in usedStreams) {
                    if (!used)
                        break;
                    unusedStream++;
                }

                if (unusedStream < 4) {
                    var index = attr.FindIndex(descr => descr.attribute == VertexAttribute.TexCoord2);
                    if (index != -1) {
                        Debug.LogWarningFormat("{0} already contains texcoord2!", mesh);
                        attr.RemoveAt(index);
                    }

                    index = attr.FindLastIndex(descr => (int)descr.attribute < (int)VertexAttribute.TexCoord2);
                    attr.Insert(index + 1, new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float16, 4, unusedStream));
                    mesh.SetVertexBufferParams(mesh.vertexCount, attr.ToArray());
                    // compress float to half
                    var transformationsHalf = new Half4[job.Transformations.Length];
                    for (int i = 0; i < job.Transformations.Length; i++)
                        transformationsHalf[i] = Half4.FromVector4(job.Transformations[i]);
                    mesh.SetVertexBufferData(transformationsHalf, 0, 0, transformationsHalf.Length, unusedStream);
                } else {
                    mesh.SetUVs(2, job.Transformations);
                }
            }

            public void Dispose()
            {
                foreach (var job in jobs)
                    job.Dispose();
            }
        }

        public struct CreateSecondaryUVTransformJob : IJob, IDisposable
        {
            public NativeArray<Vector4> Transformations;
            [ReadOnly] private NativeArray<int> triangles;
            [ReadOnly] private NativeArray<Vector3> vertices;
            [ReadOnly] private NativeArray<Vector3> normals;
            [ReadOnly] private NativeArray<Vector4> tangents;
            [ReadOnly] private NativeArray<Vector2> uvs;
            private NativeArray<Vector3> tmpTan;
            private NativeArray<Vector3> tmpBtan;

            public CreateSecondaryUVTransformJob(Mesh.MeshData meshData)
            {
                var vertexCount = meshData.vertexCount;
                var triangleCount = meshData.GetTriangeCount();
                Transformations = new NativeArray<Vector4>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                triangles = new NativeArray<int>(triangleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                vertices = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                normals = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                tangents = new NativeArray<Vector4>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                uvs = new NativeArray<Vector2>(vertexCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                tmpTan = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.ClearMemory);
                tmpBtan = new NativeArray<Vector3>(vertexCount, Allocator.TempJob, NativeArrayOptions.ClearMemory);
                meshData.GetTrianges(triangles);
                meshData.GetVertices(vertices);
                meshData.GetNormals(normals);
                meshData.GetTangents(tangents);
                meshData.GetUVs(1, uvs);
            }

            public void Execute()
            {
                for (int i = 0; i < triangles.Length; i += 3) {
                    int i0 = triangles[i], i1 = triangles[i + 1], i2 = triangles[i + 2];
                    var biTangents = calculateTangents(uvs[i1] - uvs[i0],
                                                       uvs[i2] - uvs[i0],
                                                       vertices[i1] - vertices[i0],
                                                       vertices[i2] - vertices[i0]);
                    tmpTan[i0] += biTangents.Item1;
                    tmpTan[i1] += biTangents.Item1;
                    tmpTan[i2] += biTangents.Item1;
                    tmpBtan[i0] += biTangents.Item2;
                    tmpBtan[i1] += biTangents.Item2;
                    tmpBtan[i2] += biTangents.Item2;
                }
                // TODO: maybe safe tangent for uv2 instead of transformations?
                // TODO: safe two floats (sin cos)
                for (int i = 0; i < vertices.Length; i++)
                    Transformations[i] = createTransformations(normals[i], tangents[i], tmpTan[i], tmpBtan[i]);
            }

            private static Tuple<Vector3, Vector3> calculateTangents(Vector2 e0uv, Vector2 e1uv, Vector3 e0vert, Vector3 e1vert)
            {
                var r0 = 1.0f / (e0uv.x * e1uv.y - e1uv.x * e0uv.y);
                return new Tuple<Vector3, Vector3>((e0vert * e1uv.y - e1vert * e0uv.y) * r0, (e1vert * e0uv.x - e0vert * e1uv.x) * r0);
            }

            private static Vector4 createTransformations(Vector3 normal, Vector4 tangent, Vector3 tangent1, Vector3 bitangent1)
            {
                var tangent0 = (Vector3)tangent;
                var bitangent0 = Vector3.Cross(normal, tangent0) * tangent.w;
                Vector3.OrthoNormalize(ref normal, ref tangent1, ref bitangent1);
                return new Vector4() {
                    x = Vector3.Dot(tangent0, tangent1),
                    y = Vector3.Dot(bitangent0, tangent1),
                    z = Vector3.Dot(tangent0, bitangent1),
                    w = Vector3.Dot(bitangent0, bitangent1)
                };
            }

            public void Dispose()
            {
                Transformations.Dispose();
                triangles.Dispose();
                vertices.Dispose();
                normals.Dispose();
                tangents.Dispose();
                uvs.Dispose();
                tmpTan.Dispose();
                tmpBtan.Dispose();
            }
        }
    }
}