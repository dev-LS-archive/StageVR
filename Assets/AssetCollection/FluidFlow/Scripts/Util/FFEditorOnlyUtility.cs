#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FluidFlow
{
    /// <summary>
    /// Things that are needed in the main project, but are only called in the editor
    /// </summary>
    public static class FFEditorOnlyUtility
    {
        public static readonly string EditorPrefPrefix = "FluidFlow_";

        public static bool AutoUpdateModelCaches {
            get {
                return EditorPrefs.GetBool(EditorPrefPrefix + "AutoUpdateCaches", false);
            }
            set {
                EditorPrefs.SetBool(EditorPrefPrefix + "AutoUpdateCaches", value);
            }
        }

        public static List<T> GetSubObjectsOfType<T>(Object asset) where T : UnityEngine.Object
        {
            var objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
            var ofType = new List<T>();
            foreach (var o in objs) {
                if (o is T)
                    ofType.Add(o as T);
            }
            return ofType;
        }

        public static Hash128 CalculateHashForAsset(GameObject target)
        {
            return AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(target));
        }

        public struct ProgressBarScope : System.IDisposable
        {
            private string title;
            private string info;
            private float progress;

            public ProgressBarScope(string title, string info = "")
            {
                this.title = title;
                this.info = info;
                progress = 0;
                update();
            }

            public void Update(string title, string info, float progress = -1)
            {
                this.title = title;
                this.info = info;
                if (progress >= 0)
                    this.progress = progress;
                update();
            }

            public void Update(string info, float progress = -1)
            {
                this.info = info;
                if (progress >= 0)
                    this.progress = progress;
                update();
            }

            public void Update(float progress)
            {
                this.progress = progress;
                update();
            }

            private void update()
            {
                EditorUtility.DisplayProgressBar(title, info, progress);
            }

            public void Dispose()
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }

    [System.Serializable]
    public struct Hash128Serialized
    {
        public Hash128 Hash {
            get {
                if (!hash.isValid)
                    hash = Hash128.Parse(hashString);
                return hash;
            }
            set {
                hash = value;
                hashString = value.ToString();
            }
        }

        private Hash128 hash;

        [SerializeField]
        private string hashString;

        public static implicit operator Hash128Serialized(Hash128 otherhash)
        {
            return new Hash128Serialized() { Hash = otherhash };
        }

        public static implicit operator Hash128(Hash128Serialized otherhash)
        {
            return otherhash.Hash;
        }
    }
}

#endif