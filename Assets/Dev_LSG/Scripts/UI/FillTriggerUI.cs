using System;
using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Core.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.UI
{
    public class FillTriggerUI : MonoBehaviour
    {
        public FillImage fillImage;
        public HVRUIPointer uiLeftPointer;
        public HVRUIPointer uiRightPointer;
        public UnityEvent startEvent;
        public UnityEvent stopEvent;

        private void OnEnable()
        {
            //Invoke(nameof(AddListen), 0.2f);
            //AddListen();
        }

        private void OnDisable()
        {
            //RemoveListen();
        }

        private void AddListen()
        {
            //print(("enable"));
            HVRControllerEvents.Instance.LeftTriggerActivated.AddListener(CallLeft);
            HVRControllerEvents.Instance.RightTriggerActivated.AddListener(CallRight);
            HVRControllerEvents.Instance.LeftTriggerDeactivated.AddListener(StopLeft);
            HVRControllerEvents.Instance.RightTriggerDeactivated.AddListener(StopRight);
        }

        private void RemoveListen()
        {
            //print(("disable"));
            HVRControllerEvents.Instance.LeftTriggerActivated.RemoveListener(CallLeft);
            HVRControllerEvents.Instance.RightTriggerActivated.RemoveListener(CallRight);
            HVRControllerEvents.Instance.LeftTriggerDeactivated.RemoveListener(StopLeft);
            HVRControllerEvents.Instance.RightTriggerDeactivated.RemoveListener(StopRight);
        }

        void CallLeft()
        {
            CallFill(uiLeftPointer);
        }
        void CallRight()
        {
            CallFill(uiRightPointer);
        }
        void StopLeft()
        {
            StopFill(uiLeftPointer);
        }
        void StopRight()
        {
            StopFill(uiRightPointer);
        }

        private void Update()
        {
            if (uiLeftPointer.CurrentUIElement == true)
            {
                if (uiLeftPointer.ViewOnEvent == false)
                    CallLeft();
            }
            if (uiRightPointer.CurrentUIElement == true)
            {
                if (uiRightPointer.ViewOnEvent == false) 
                    CallRight();
            }
        }

        void CallFill(HVRUIPointer pointer)
        {
            if (pointer.CurrentUIElement == true)
            {
                if (pointer.CurrentUIElement.name == fillImage.cooldown.gameObject.name)
                {
                    pointer.ViewOnEvent = true;
                    startEvent.Invoke();
                    fillImage.ActFill();
                }
            }
        }
        public void StopFill(HVRUIPointer pointer)
        {
            pointer.ViewOnEvent = false;
            fillImage.StopFill();
        }
    }
}
