using System;
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
        public int eventNum = 0;
        public bool canEvent = false;
        

        private void OnEnable()
        {
            HVRControllerEvents.Instance.LeftTriggerActivated.AddListener(Event_Act);
            HVRControllerEvents.Instance.RightTriggerActivated.AddListener(Event_Act);
        }

        private void OnDisable()
        {
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
