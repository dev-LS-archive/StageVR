using System;
using Dev_LSG.Scripts.Player;
using HurricaneVR.Framework.Core;
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
        private AssetReference hvrGlobal;
        
        [SerializeField]
        private AssetReference handsMatAssetReference;
        
        [SerializeField]
        private AssetReferenceAudioClip musicAssetReference;
        
        [SerializeField]
        private AssetReferenceTexture2D texture2DAssetReference;

        [SerializeField]
        private HVRManager hvrManager;


        //UI Component
        private RawImage _rawImage;
        
        // Start is called before the first frame update
        void Start()
        {
            Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
        }

        private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
        {
            hvrGlobal.InstantiateAsync().Completed += (manager) =>
            {
                var hvr = manager.Result;
                hvrManager = hvr.GetComponent<HVRManager>();
                playerXRRigAssetReference.InstantiateAsync().Completed += (go) =>
                {
                    var playerController = go.Result;
#if UNITY_EDITOR
                    Core.Logger.Instance.LogInfo(playerController.ToString());
#else
                    if(Debug.isDebugBuild)
                        Core.Logger.Instance.LogInfo(playerController.ToString()); 
#endif
                    var target = Array.Find(Labels.Instance.labels, label => label.name == "XR");
                    playerController.transform.SetParent(target);
                    hvr.transform.SetParent(target);
                    hvrManager.PlayerController =
                        playerController.GetComponent<PlayerComponent>().hvrPlayerController;
#if UNITY_EDITOR
                    Core.Logger.Instance.LogInfo(hvrManager.PlayerController.ToString());
#else
                    if(Debug.isDebugBuild)
                        Core.Logger.Instance.LogInfo(hvrManager.PlayerController.ToString());
#endif
                };
            };
            
        }
    }
}
