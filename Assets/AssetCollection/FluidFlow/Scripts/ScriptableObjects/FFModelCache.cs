using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace FluidFlow
{
    [PreferBinarySerialization]
    public class FFModelCache : ScriptableObject
    {
        public GameObject Target { get { return target; } }

        public Mesh[] SourceMeshes { get { return sourceMeshes; } }

        public Mesh[] CachedSecondaryUVMeshes { get { return cachedSecondaryUVMeshes; } }

        public List<CachedData> CachedDataList { get { return cachedDataList; } }

        [SerializeField, Tooltip("Target model that is cached.")]
        private GameObject target;

        [SerializeField, Tooltip("All child meshes of the target model.")]
        private Mesh[] sourceMeshes;

        [SerializeField, Tooltip("Pregenerated/cached secondary uv meshes.")]
        private Mesh[] cachedSecondaryUVMeshes;

        [System.Serializable]
        public class CachedData
        {
            public int SourceMeshId;
            public UVSet UVSet;
            public int StitchSubmeshMask;
            public List<Stitch> Stitches;
        }

        [SerializeField, Tooltip("Cached stitch data.")]
        private List<CachedData> cachedDataList;

        #region EDITOR ONLY GENERATION

#if UNITY_EDITOR

        [System.Serializable]
        public class SerializableUnwrapParam
        {
            [Range(0f, 180f), Tooltip("This angle (in degrees) or greater between triangles will cause seam to be created.")]
            public float HardAngle = 86f;

            [Range(0f, 1f), Tooltip("Maximum allowed angle distortion (0..1).")]
            public float AngleError = .08f;

            [Range(0f, 1f), Tooltip("Maximum allowed area distortion (0..1).")]
            public float AreaError = .15f;

            [Range(0, 64), Tooltip("Measured in pixels, assuming the mesh will cover an entire 1024x1024 texture.")]
            public int PackMargin = 4;

            public static implicit operator UnwrapParam(SerializableUnwrapParam param)
            {
                return new UnwrapParam() {
                    angleError = param.AngleError,
                    areaError = param.AreaError,
                    hardAngle = param.HardAngle,
                    packMargin = param.PackMargin * (1.0f / 1024.0f)
                };
            }
        }

        [Tooltip("Unwrap parameters used for automatic secondary uv map generation, when the mesh currently has no secondary uv set.")]
        public SerializableUnwrapParam SecondaryUVUnwrapParameters;

        [SerializeField, Tooltip("Hash of the source model asset, to determine if the cache needs to be rebuild")]
        private Hash128Serialized targetAssetHash;

        public Hash128 GetTargetHash()
        {
            return targetAssetHash;
        }

        public void UpdateTargetHash(Hash128 hash)
        {
            targetAssetHash = hash;
            EditorUtility.SetDirty(this);
        }

        public void Initialize(GameObject target)
        {
            this.target = target;
            targetAssetHash = FFEditorOnlyUtility.CalculateHashForAsset(target);
            sourceMeshes = FFEditorOnlyUtility.GetSubObjectsOfType<Mesh>(target).ToArray();
            cachedSecondaryUVMeshes = new Mesh[sourceMeshes.Length];
            cachedDataList = new List<CachedData>();
            cachedDataList.Add(new CachedData());
        }

        public struct Setting
        {
            public int SourceMeshId;
            public UVSet UVSet;
            public int SubmeshMask;
        }

        public List<Setting> GetSettings()
        {
            var settings = new List<Setting>();
            foreach (var cache in cachedDataList)
                settings.Add(new Setting() { SourceMeshId = cache.SourceMeshId, UVSet = cache.UVSet, SubmeshMask = cache.StitchSubmeshMask });
            return settings;
        }

        public bool Matches(Mesh mesh, UVSet uvSet, int submeshMask)
        {
            var validatedMask = mesh.ValidateSubmeshMask(submeshMask);
            return cachedDataList.Any(data => (sourceMeshes[data.SourceMeshId] == mesh || cachedSecondaryUVMeshes[data.SourceMeshId] == mesh)
                                                && data.UVSet == uvSet
                                                && mesh.ValidateSubmeshMask(data.StitchSubmeshMask) == validatedMask);
        }

        public bool Matches(List<Setting> settings)
        {
            if (settings.Count != cachedDataList.Count)
                return false;
            for (int i = 0; i < settings.Count; i++) {
                if (cachedDataList[i].SourceMeshId != settings[i].SourceMeshId ||
                        cachedDataList[i].UVSet != settings[i].UVSet ||
                        cachedDataList[i].StitchSubmeshMask != settings[i].SubmeshMask)
                    return false;
            }
            return true;
        }

        public void ApplySettings(List<Setting> settings, bool force = false)
        {
            using (var progress = new FFEditorOnlyUtility.ProgressBarScope("Updating FFModelCache")) {
                var secondaryUVMeshIds = new HashSet<int>();
                settings.ForEach(setting => {
                    if (setting.UVSet == UVSet.UV1)
                        secondaryUVMeshIds.Add(setting.SourceMeshId);
                });

                // generate meshes with secondary uv set and transformations
                foreach (var i in secondaryUVMeshIds) {
                    progress.Update("Updating secondary uv mesh cache (" + sourceMeshes[i].name + ")",
                                Mathf.Lerp(0, .5f, (i + 1) / (float)cachedSecondaryUVMeshes.Length));
                    if (!cachedSecondaryUVMeshes[i] || force) {
                        var secondaryUVCache = GenerateSecondaryUVCache(sourceMeshes[i], SecondaryUVUnwrapParameters);
                        if (cachedSecondaryUVMeshes[i]) {
                            // copy to existing mesh, so all references keep valid
                            Utility.OverwriteExistingMesh(secondaryUVCache, cachedSecondaryUVMeshes[i]);
                            EditorUtility.SetDirty(cachedSecondaryUVMeshes[i]);
                        } else {
                            cachedSecondaryUVMeshes[i] = secondaryUVCache;
                            AssetDatabase.AddObjectToAsset(cachedSecondaryUVMeshes[i], this);
                        }
                    }
                }
                // remove unnecessary secondary uv caches
                for (int i = 0; i < cachedSecondaryUVMeshes.Length; i++) {
                    if (!secondaryUVMeshIds.Contains(i) && cachedSecondaryUVMeshes[i]) {
                        // remove existing cached mesh, if present
                        AssetDatabase.RemoveObjectFromAsset(cachedSecondaryUVMeshes[i]);
                        cachedSecondaryUVMeshes[i] = null;
                    }
                }

                // build stitch cache
                cachedDataList.Clear();
                for (int i = 0; i < settings.Count; i++) {
                    progress.Update("Updating stitch cache (" + (i + 1) + "/" + settings.Count + ")",
                            Mathf.Lerp(.5f, 1, (i + 1) / (float)settings.Count));

                    var newData = new CachedData() {
                        SourceMeshId = settings[i].SourceMeshId,
                        UVSet = settings[i].UVSet,
                        StitchSubmeshMask = sourceMeshes[settings[i].SourceMeshId].ValidateSubmeshMask(settings[i].SubmeshMask)
                    };

                    if (!cachedDataList.Any(data => data.SourceMeshId == newData.SourceMeshId
                                                    && data.UVSet == newData.UVSet
                                                    && data.StitchSubmeshMask == newData.StitchSubmeshMask)) {
                        newData.Stitches = SeamStitcher.GetStitches(
                            newData.UVSet == UVSet.UV0
                                ? sourceMeshes[newData.SourceMeshId]
                                : cachedSecondaryUVMeshes[newData.SourceMeshId],
                            newData.UVSet,
                            newData.StitchSubmeshMask);
                        cachedDataList.Add(newData);
                    }
                }
            }
        }

        public void ForceRegeneration()
        {
            ApplySettings(GetSettings(), true);
        }

        private static Mesh GenerateSecondaryUVCache(Mesh source, UnwrapParam unwrapParam)
        {
            var copy = Utility.CopyMesh(source);
            // generate lightmap uv, if required
            if (!copy.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord1))
                Unwrapping.GenerateSecondaryUVSet(copy, unwrapParam);
            using (var job = new Gravity.CreateSecondaryUVTransformJobGroup(new List<Mesh>() { copy })) {
                job.Run();
                job.ApplyResults();
            }
            return copy;
        }

#endif

        #endregion EDITOR ONLY GENERATION
    }
}