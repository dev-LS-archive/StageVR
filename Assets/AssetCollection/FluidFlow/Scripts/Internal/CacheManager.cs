using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace FluidFlow
{
    /// <summary>
    /// Cache mesh data, so it can be shared between renderers.
    /// </summary>
    public class CacheManager
    {
        #region Singleton

        private static CacheManager instance;

        public static CacheManager Instance {
            get {
                if (instance == null) {
                    instance = new CacheManager();
                    instance.Initialize();
                }
                return instance;
            }
        }

        #endregion Singleton

        private int currentCacheId = 0;
        private Dictionary<Mesh, int> meshToId = new Dictionary<Mesh, int>();

        private int requestIdFor(Mesh mesh, Mesh other = null)
        {
            int id;
            if (!meshToId.TryGetValue(mesh, out id)) {
                if (!other || !meshToId.TryGetValue(other, out id))
                    id = currentCacheId++;
            }
            if (!meshToId.ContainsKey(mesh))
                meshToId.Add(mesh, id);
            if (other && !meshToId.ContainsKey(other))
                meshToId.Add(other, id);
            return id;
        }

        // cached

        private Dictionary<int, Mesh> secondaryUVMeshCache = new Dictionary<int, Mesh>();
        private Dictionary<Tuple<int, UVSet, int>, List<Stitch>> stitchesCache = new Dictionary<Tuple<int, UVSet, int>, List<Stitch>>();

        // requested

        private List<Mesh> requestedMeshes = new List<Mesh>();
        private List<Tuple<SeamStitcher.MeshCache, List<Stitch>>> requestedStitches = new List<Tuple<SeamStitcher.MeshCache, List<Stitch>>>();

        public void Initialize()
        {
            // initialize CacheManager with all currently loaded FFModelCache instances
            foreach (var cache in Resources.FindObjectsOfTypeAll<FFModelCache>()) {
                if (cache.Target) {
                    for (int i = 0; i < cache.SourceMeshes.Length; i++) {
                        if (cache.CachedSecondaryUVMeshes[i])
                            AddSecondaryUVMesh(cache.SourceMeshes[i], cache.CachedSecondaryUVMeshes[i]);
                    }
                    foreach (var data in cache.CachedDataList) {
                        if (data.StitchSubmeshMask != 0)
                            AddStitches(cache.SourceMeshes[data.SourceMeshId], data.UVSet, data.StitchSubmeshMask, data.Stitches);
                    }
                }
            }
        }

        public void Clear()
        {
            currentCacheId = 0;
            meshToId.Clear();
            secondaryUVMeshCache.Clear();
            stitchesCache.Clear();
            requestedMeshes.Clear();
            requestedStitches.Clear();
        }

        public void AddSecondaryUVMesh(Mesh original, Mesh generated)
        {
            if (generated.HasVertexAttribute(VertexAttribute.TexCoord1))
                secondaryUVMeshCache.Add(requestIdFor(original, generated), generated);
            else
                Debug.LogWarningFormat("Can not add {0} to cache, as it does not have a secondary uv set and uv transformation data.", generated);
        }

        public void AddStitches(Mesh original, UVSet uvSet, int submeshMask, List<Stitch> stitches)
        {
            stitchesCache.Add(Tuple.Create(requestIdFor(original), uvSet, original.ValidateSubmeshMask(submeshMask)), stitches);
        }

        public Mesh RequestSecondaryUVMesh(Mesh source)
        {
            Mesh generate()
            {
                if (source.HasVertexAttribute(VertexAttribute.TexCoord1)) {
                    var copy = Utility.CopyMesh(source);
                    requestedMeshes.Add(copy);
                    AddSecondaryUVMesh(source, copy);
                    return copy;
                } else {
                    Debug.LogWarningFormat("Can not generate secondary uv transformations for {0}, as it does not have a secondary uv set.", source);
                    return null;
                }
            }
            int cacheId;
            if (!meshToId.TryGetValue(source, out cacheId))
                // mesh unknown to cache manager
                return generate();
            Mesh result;
            if (!secondaryUVMeshCache.TryGetValue(cacheId, out result))
                // no secondary uv mesh linked to this source mesh
                return generate();
            return result;
        }

        public List<Stitch> RequestStitches(Mesh source, UVSet uvSet, int submeshMask, bool async = false)
        {
            List<Stitch> generate()
            {
                var meshData = new SeamStitcher.MeshCache(source, uvSet, submeshMask);
                var stitches = new List<Stitch>();
                if (async)
                    requestedStitches.Add(Tuple.Create(meshData, stitches));
                else
                    SeamStitcher.GenerateStitches(meshData, stitches);
                AddStitches(source, uvSet, submeshMask, stitches);
                return stitches;
            }
            int cacheId;
            if (!meshToId.TryGetValue(source, out cacheId))
                // mesh unknown to cache manager
                return generate();
            List<Stitch> result;
            if (!stitchesCache.TryGetValue(Tuple.Create(cacheId, uvSet, submeshMask), out result))
                // no pregenerated stitch data linked to this source mesh
                return generate();
            return result;
        }

        public System.Collections.IEnumerator GenerateRequestedMeshes(bool async)
        {
            if (requestedMeshes.Count > 0) {
                using (var jobGroup = new Gravity.CreateSecondaryUVTransformJobGroup(requestedMeshes)) {
                    requestedMeshes.Clear();
                    if (async) {
                        var handles = jobGroup.ScheduleGroup();
                        while (handles.Any(handle => !handle.IsCompleted))
                            yield return null;
                        foreach (var handle in handles)
                            handle.Complete();
                    } else {
                        jobGroup.Run();
                    }
                    jobGroup.ApplyResults();
                }
            }
        }

        public System.Collections.IEnumerator GenerateRequestedStitches()
        {
            // ensure list is not altered during generation
            var tmp = requestedStitches;
            requestedStitches = new List<Tuple<SeamStitcher.MeshCache, List<Stitch>>>();
            yield return new AsyncExecute(() => {
                foreach (var requested in tmp)
                    SeamStitcher.GenerateStitches(requested.Item1, requested.Item2);
            });
        }
    }
}