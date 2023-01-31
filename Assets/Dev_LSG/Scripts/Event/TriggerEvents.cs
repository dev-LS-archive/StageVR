using System.Collections;
using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class TriggerEvents : MonoBehaviour
    {
        public UnityEvent triggerEvent;
        public HVRInputManager inputManager;

        private void OnEnable()
        {
            StartCoroutine(FindInputManager());
        }

        private IEnumerator FindInputManager()
        {
            yield return new WaitForEndOfFrame();
            inputManager = FindObjectOfType<HVRInputManager>();
            inputManager.LeftControllerConnected.AddListener(LeftConnected);
            inputManager.RightControllerConnected.AddListener(RightConnected);
            yield return null;
        }
        
        private void LeftConnected(HVRController arg0)
        {
            HVRControllerEvents.Instance.LeftTriggerActivated.AddListener(Event_Act);
        }

        private void RightConnected(HVRController arg0)
        {
            HVRControllerEvents.Instance.RightTriggerActivated.AddListener(Event_Act);
        }

        private void OnDisable()
        {
            inputManager.LeftControllerConnected.RemoveListener(LeftConnected);
            inputManager.RightControllerConnected.RemoveListener(RightConnected);
            HVRControllerEvents.Instance.LeftTriggerActivated.RemoveListener(Event_Act);
            HVRControllerEvents.Instance.RightTriggerActivated.RemoveListener(Event_Act);
        }

        void Event_Act()
        {
            triggerEvent.Invoke();
        }
    }
}
