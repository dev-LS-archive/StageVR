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
        #region VARABLE
        [SerializeField]
        private AssetReference playerXRRigAssetReference;

        [SerializeField]
        private AssetReference hvrGlobal;
        
        [SerializeField]
        public AssetReference playerUI;
        
        [SerializeField]
        public AssetReference uiManager;
        
        //[SerializeField]
        //private AssetReference handsMatAssetReference;
        
        [SerializeField]
        private AssetReferenceAudioClip musicAssetReference;
        
        //[SerializeField]
        //private AssetReferenceTexture2D texture2DAssetReference;

        [SerializeField]
        private HVRManager hvrManager;

        [SerializeField]
        private HVRInputModule hvrInputModule;


        //UI Component
        private RawImage _rawImage;
        
        //Instance Objects
        private GameObject _manager, _playerController, _hvr,_playerUIResult;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            Logger("Initializing Addressables...");
            Addressables.InitializeAsync().Completed += AddressablesManager_Completed;
        }

        private void AddressablesManager_Completed(AsyncOperationHandle<IResourceLocator> obj)
        {
            Logger("Initialized Addressables...");
            
            uiManager.InstantiateAsync().Completed += (uiAsset) =>
            {
                _manager = uiAsset.Result;
                hvrInputModule = _manager.GetComponent<HVRInputModule>();
                var target = Array.Find(Labels.Instance.labels, label => label.name == "Manager");
                _manager.transform.SetParent(target);

                Logger("Initialized the " + _manager);
            };
            hvrGlobal.InstantiateAsync().Completed += (manager) =>
            {
                _hvr = manager.Result;
                hvrManager = _hvr.GetComponent<HVRManager>();
                Logger("Initialized the " + _hvr);
                
                playerXRRigAssetReference.InstantiateAsync().Completed += (go) =>
                {
                    _playerController = go.Result;
                    
                    Logger("Initialized the " +_playerController);
                    
                    var target = Array.Find(Labels.Instance.labels, label => label.name == "XR");
                    _playerController.transform.SetParent(target);
                    _hvr.transform.SetParent(target);
                    hvrManager.PlayerController =
                        _playerController.GetComponent<PlayerComponent>().hvrPlayerController;

                    playerUI.InstantiateAsync().Completed += (uiAsset) =>
                    {
                        _playerUIResult = uiAsset.Result;
                        _playerUIResult.transform.SetParent(target);
                        _playerController.GetComponent<PlayerComponent>().menuButtonActive.menu = _playerUIResult;
                        hvrInputModule.UICanvases.Add(_playerUIResult.GetComponent<UICanvas>().canvas);
                        
                        Logger("Initialized the " + _playerUIResult);
                    };
                    musicAssetReference.LoadAssetAsync().Completed += (clip) =>
                    {
                        var audioSource = _hvr.GetComponent<SoundSource>().bgmPlayer;
                        audioSource.clip = clip.Result;
                        audioSource.playOnAwake = false;
                        audioSource.loop = true;
                        audioSource.Play();
                    
                        Logger("Loaded the audio clip...");
                    };
                };
            };
            Logger("Loaded Assets...");
        }
        
        // private void OnDestroy()
        // {
        //     uiManager.ReleaseInstance(_manager);
        //     playerXRRigAssetReference.ReleaseInstance(_playerController);
        //     hvrGlobal.ReleaseInstance(_hvr);
        //     playerUI.ReleaseInstance(_playerUIResult);
        //     musicAssetReference.ReleaseAsset();
        // } 

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
