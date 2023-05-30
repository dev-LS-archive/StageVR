using System.Collections;
using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class TriggerEvent : MonoBehaviour
    {
        public UnityEvent triggerEvent;

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

        void Event_Act()
        {
            triggerEvent.Invoke();
        }
    }
}
