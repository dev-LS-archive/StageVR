using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

namespace FluidFlow
{
    /// <summary>
    /// Find and stitch seams between uv islands, to allow for fluid flowing between them
    /// </summary>
    public static class SeamStitcher
    {
        #region Public

        public class Edge
        {
            public readonly Vector3 VertA;
            public readonly Vector3 VertB;
            public readonly Vector2 UvA;
            public readonly Vector2 UvB;
            public readonly Vector2 UvC;

            public Edge(Vector3 vertA, Vector3 vertB, Vector2 uvA, Vector2 uvB, Vector2 uvC)
            {
                bool switchVerts = vertexComparer.Compare(vertA, vertB) <= 0;
                VertA = switchVerts ? vertB : vertA;
                VertB = switchVerts ? vertA : vertB;
                UvA = switchVerts ? uvB : uvA;
                UvB = switchVerts ? uvA : uvB;
                UvC = uvC;
            }

            public bool Matches(Edge other)
            {
                // vertex positions match, but at least one uv does not
                return (VertA - other.VertA).sqrMagnitude <= delta &&
                       (VertB - other.VertB).sqrMagnitude <= delta &&
                       ((UvA - other.UvA).sqrMagnitude > delta ||
                        (UvB - other.UvB).sqrMagnitude > delta);
            }

            public Vector2 Normal()
            {
                var ab = UvB - UvA;
                var f = UvA + (Vector2.Dot(UvC - UvA, ab) / ab.sqrMagnitude) * ab;
                return (UvC - f).normalized;
            }
        }

        public struct MeshCache
        {
            public readonly Vector3[] Vertices;
            public readonly Vector2[] UVs;
            public readonly int[] Triangles;

            public MeshCache(Mesh mesh, UVSet uvSet, int submesh)
            {
                Vertices = mesh.vertices;
                UVs = mesh.GetUVSet(uvSet);
                Triangles = mesh.GetSubmeshIndices(submesh);
            }
        }

        public static List<Stitch> GetStitches(Mesh from, UVSet uvSet, int submeshMask)
        {
            var stitches = new List<Stitch>();
            if (submeshMask != 0)
                GenerateStitches(new MeshCache(from, uvSet, submeshMask), stitches);
            return stitches;
        }

        public static void GenerateStitches(MeshCache meshData, List<Stitch> stitches)
        {
            // simple sweep and prune
            var edges = orderedEdgesFromMesh(meshData);
            for (int a = 0; a < edges.Length; a++) {
                var edgeA = edges[a];
                for (int b = a + 1; b < edges.Length; b++) {
                    var edgeB = edges[b];
                    if ((edgeA.VertA - edgeB.VertA).sqrMagnitude > delta)
                        break;
                    if (edgeA.Matches(edgeB)) {
                        stitches.Add(new Stitch(edgeA, edgeB));
                        break;
                    }
                }
            }
        }

        public struct TmpStitchData
        {
            public readonly Vector2 VertA;
            public readonly Vector2 VertB;
            public readonly Vector4 DataA;
            public readonly Vector4 DataB;

            public TmpStitchData(Vector2 vertA, Vector2 vertB, Vector4 dataA, Vector4 dataB)
            {
                this.VertA = vertA;
                this.VertB = vertB;
                this.DataA = dataA;
                this.DataB = dataB;
            }
        }

        public static IEnumerable<TmpStitchData> Transform(this List<Stitch> stitches, Vector4 atlasTransform)
        {
            var atlasOffset = new Vector2(atlasTransform.x, atlasTransform.y);
            foreach (var stitch in stitches) {
                var a0 = stitch.A0 * atlasTransform.z + atlasOffset;
                var b0 = stitch.B0 * atlasTransform.z + atlasOffset;
                var a1 = stitch.A1 * atlasTransform.z + atlasOffset;
                var b1 = stitch.B1 * atlasTransform.z + atlasOffset;
                yield return new TmpStitchData(a0, b0, new Vector4(a1.x, a1.y, stitch.N0, stitch.N1), new Vector4(b1.x, b1.y, stitch.N0, stitch.N1));
                yield return new TmpStitchData(a1, b1, new Vector4(a0.x, a0.y, stitch.N1, stitch.N0), new Vector4(b0.x, b0.y, stitch.N1, stitch.N0));
            }
        }

        #endregion Public

        #region Private

        private const float delta = float.Epsilon;

        private static Edge[] orderedEdgesFromMesh(MeshCache mesh)
        {
            var edges = new List<Edge>(mesh.Triangles.Length);
            for (int i = 0; i < mesh.Triangles.Length; i++) {
                int t = (i / 3) * 3;
                int a = mesh.Triangles[t + ((i + 0) % 3)];
                int b = mesh.Triangles[t + ((i + 1) % 3)];
                int c = mesh.Triangles[t + ((i + 2) % 3)];
                edges.Add(new Edge(mesh.Vertices[a], mesh.Vertices[b], mesh.UVs[a], mesh.UVs[b], mesh.UVs[c]));
            }
            return edges.OrderBy(edge => edge.VertA, vertexComparer).ToArray();
        }

        private static readonly IComparer<Vector3> vertexComparer = Comparer<Vector3>.Create((Vector3 a, Vector3 b) => {
            if (a.x < b.x)
                return 1;
            if (a.x == b.x) {
                if (a.y < b.y)
                    return 1;
                if (a.y == b.y) {
                    if (a.z < b.z)
                        return 1;
                    if (a.z == b.z)
                        return 0;
                }
            }
            return -1;
        });

        #endregion Private
    }

    public struct StitchMapDrawer
    {
        private RenderTexture target;
        private bool generateAsync;
        private List<Tuple<List<Stitch>, Vector4>> stitchesLists;

        public StitchMapDrawer(RenderTexture target, bool generateAsync)
        {
            this.target = target;
            this.generateAsync = generateAsync;
            stitchesLists = new List<Tuple<List<Stitch>, Vector4>>();
        }

        public void AddStitches(RenderTarget rt)
        {
            var stitches = CacheManager.Instance.RequestStitches(rt.Mesh, rt.UVSet, rt.SubmeshMask, generateAsync);
            stitchesLists.Add(Tuple.Create(stitches, rt.AtlasTransform));
        }

        public IEnumerator Draw()
        {
            if (generateAsync)
                yield return CacheManager.Instance.GenerateRequestedStitches();
            var mesh = toMesh();
            using (RestoreRenderTarget.RestoreActive()) {
                using (var tmp = new TmpRenderTexture(target.descriptor)) {
                    // draw stitch map
                    Graphics.SetRenderTarget(tmp);
                    GL.Clear(false, true, Color.clear);
                    Shader.SetGlobalVector(InternalShaders.TexelSizePropertyID, target.GetTexelSize());
                    Fluid.FlowTextureVariant(InternalShaders.FluidUVSeamStitch, target).SetPass(0);
                    Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                    // add padding
                    Graphics.Blit(tmp, target, InternalShaders.FluidUVSeamPadding);
                }
            }
            UnityEngine.Object.Destroy(mesh);
        }

        private Mesh toMesh()
        {
            int stitchesCount = 0;
            foreach (var stitches in stitchesLists)
                stitchesCount += stitches.Item1.Count;
            var verts = new Vector3[stitchesCount * 4];
            var data = new Vector4[stitchesCount * 4];
            var tris = new int[stitchesCount * 4];
            int index = 0;
            foreach (var stitches in stitchesLists) {
                foreach (var stitchData in stitches.Item1.Transform(stitches.Item2)) {
                    verts[index] = stitchData.VertA;
                    data[index] = stitchData.DataA;
                    tris[index] = index;
                    index++;
                    verts[index] = stitchData.VertB;
                    data[index] = stitchData.DataB;
                    tris[index] = index;
                    index++;
                }
            }
            var mesh = new Mesh();
            mesh.indexFormat = verts.Length >= (1 << 16) ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.SetVertices(verts);
            mesh.SetUVs(0, data);
            mesh.SetIndices(tris, MeshTopology.Lines, 0);
            mesh.UploadMeshData(true);
            return mesh;
        }
    }

    [System.Serializable]
    public struct Stitch
    {
        public Vector2 A0;
        public Vector2 B0;
        public float N0;
        public Vector2 A1;
        public Vector2 B1;
        public float N1;

        public Stitch(SeamStitcher.Edge e0, SeamStitcher.Edge e1)
        {
            A0 = e0.UvA;
            B0 = e0.UvB;
            N0 = e0.Normal().ToPolar();
            A1 = e1.UvA;
            B1 = e1.UvB;
            N1 = e1.Normal().ToPolar();
        }
    }
}