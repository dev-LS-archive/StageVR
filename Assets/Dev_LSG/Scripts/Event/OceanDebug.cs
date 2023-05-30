using HurricaneVR.Framework.ControllerInput;
using UnityEngine;
using Crest;

namespace Dev_LSG.Scripts.Event
{
    public class OceanDebug : MonoBehaviour
    {
        public OceanRenderer or;
        private void OnEnable()
        {
            HVRControllerEvents.Instance.LeftTriggerActivated.AddListener(LogAsset);
            HVRControllerEvents.Instance.RightTriggerActivated.AddListener(LogAsset);
        }

        private void OnDisable()
        {
            HVRControllerEvents.Instance.LeftTriggerActivated.RemoveListener(LogAsset);
            HVRControllerEvents.Instance.RightTriggerActivated.RemoveListener(LogAsset);
        }

        public void LogAsset()
        {
            Core.Logger.Instance.LogInfo("print");
            
            if (or._simSettingsDynamicWaves == null)
            {
                Core.Logger.Instance.LogError("DynamicWaves Null");
            }
            else
            {
                Core.Logger.Instance.LogInfo("What:" + or._simSettingsDynamicWaves);
            }
        }
    }
}
