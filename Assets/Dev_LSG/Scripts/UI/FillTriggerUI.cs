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
            print(("enable"));
            HVRControllerEvents.Instance.LeftTriggerActivated.AddListener(CallLeft);
            HVRControllerEvents.Instance.RightTriggerActivated.AddListener(CallRight);
            HVRControllerEvents.Instance.LeftTriggerDeactivated.AddListener(StopLeft);
            HVRControllerEvents.Instance.RightTriggerDeactivated.AddListener(StopRight);
        }

        private void OnDisable()
        {
            print(("disable"));
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
        
        void CallFill(HVRUIPointer pointer)
        {
            if (pointer.CurrentUIElement == true)
            {
                pointer.ViewOnEvent = true;
                startEvent.Invoke();
                fillImage.ActFill();
            }
        }
        public void StopFill(HVRUIPointer pointer)
        {
            pointer.ViewOnEvent = false;
            fillImage.StopFill();
        }
    }
}
