using System;
using Dev_LSG.Scripts.Player;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.UI;
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
        public AssetReference playerUI;
        
        [SerializeField]
        public AssetReference uiManager;
        
        [SerializeField]
        private AssetReference handsMatAssetReference;
        
        [SerializeField]
        private AssetReferenceAudioClip musicAssetReference;
        
        [SerializeField]
        private AssetReferenceTexture2D texture2DAssetReference;

        [SerializeField]
        private HVRManager hvrManager;

        [SerializeField]
        private HVRInputModule hvrInputModule;


        //UI Component
        private RawImage _rawImage;
        
        // Start is called before the first frame update
        void Start()
        {
            Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
        }

        private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
        {
            uiManager.InstantiateAsync().Completed += (uiAsset) =>
            {
                var manager = uiAsset.Result;
                hvrInputModule = manager.GetComponent<HVRInputModule>();
                var target = Array.Find(Labels.Instance.labels, label => label.name == "Manager");
                manager.transform.SetParent(target);
                
                Logger(manager.ToString());
            };
            hvrGlobal.InstantiateAsync().Completed += (manager) =>
            {
                var hvr = manager.Result;
                hvrManager = hvr.GetComponent<HVRManager>();
                playerXRRigAssetReference.InstantiateAsync().Completed += (go) =>
                {
                    var playerController = go.Result;
                    
                    Logger(playerController.ToString());
                    
                    var target = Array.Find(Labels.Instance.labels, label => label.name == "XR");
                    playerController.transform.SetParent(target);
                    hvr.transform.SetParent(target);
                    hvrManager.PlayerController =
                        playerController.GetComponent<PlayerComponent>().hvrPlayerController;
                    
                    Logger(hvrManager.PlayerController.ToString());
                    
                    playerUI.InstantiateAsync().Completed += (uiAsset) =>
                    {
                        var playerUIResult = uiAsset.Result;
                        playerUIResult.transform.SetParent(target);
                        playerController.GetComponent<PlayerComponent>().menuButtonActive.menu = playerUIResult;
                        hvrInputModule.UICanvases.Add(playerUIResult.GetComponent<UICanvas>().canvas);
                        Logger(hvrInputModule.UICanvases[1].ToString());
                    };
                };
            };
            
        }

        private void Logger(String str)
        {
#if UNITY_EDITOR
            Core.Logger.Instance.LogInfo(str);
#else
            if(Debug.isDebugBuild)
                Core.Logger.Instance.LogInfo(str);
#endif
        }
    }
}
