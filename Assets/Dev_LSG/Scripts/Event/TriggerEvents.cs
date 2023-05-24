using System.Collections;
using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Shared;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Dev_LSG.Scripts.Event
{
    public class TriggerEvents : MonoBehaviour
    {
        public UnityEvent[] triggerEvents;
        public HVRInputManager inputManager;
        public int eventNum = 0;
        public bool canEvent = false;

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

        public void SetEventNum(int num)
        {
            eventNum = num;
        }
        public void SetCanEvent(bool can)
        {
            canEvent = can;
        }
        void Event_Act()
        {
            if (canEvent)
            {
                triggerEvents[eventNum].Invoke();
                canEvent = false;
                print("Event");
            }
        }
    }
}
