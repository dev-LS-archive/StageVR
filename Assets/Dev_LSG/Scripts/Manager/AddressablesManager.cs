using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Dev_LSG.Scripts.Manager
{
    [Serializable]
    public class AssetReferenceAudioClip : AssetReferenceT<AudioClip>
    {
        public AssetReferenceAudioClip(string guid) : base(guid) { }
    }
    public class AddressablesManager : MonoBehaviour
    {
        [SerializeField]
        private AssetReference playerXRRigAssetReference;

        [SerializeField]
        private AssetReference handsMatAssetReference;
        
        [SerializeField]
        private AssetReferenceAudioClip musicAssetReference;
        
        [SerializeField]
        private AssetReferenceTexture2D texture2DAssetReference;


        //UI Component
        private RawImage _rawImage;
        
        // Start is called before the first frame update
        void Start()
        {
            Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
            
        }

        private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
        {
            playerXRRigAssetReference.InstantiateAsync().Completed += (go) =>
            {
                var playerController = go.Result;
                var target = Array.Find(Labels.Instance.labels, label => label.name == "XR");
                playerController.transform.SetParent(target);

            };
        }
        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
