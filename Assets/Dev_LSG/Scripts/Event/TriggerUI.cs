using HurricaneVR.Framework.ControllerInput;
using UnityEngine;
using UnityEngine.Events;

namespace Dev_LSG.Scripts.Event
{
    public class TriggerUI : MonoBehaviour
    {
        public UnityEvent disableEvent;
        public GameObject ui;

        private void OnEnable()
        {
            HVRControllerEvents.Instance.LeftTriggerActivated.AddListener(UI_Act);
            HVRControllerEvents.Instance.RightTriggerActivated.AddListener(UI_Act);
        }

        private void OnDisable()
        {
            disableEvent.Invoke();
            HVRControllerEvents.Instance.LeftTriggerActivated.RemoveListener(UI_Act);
            HVRControllerEvents.Instance.RightTriggerActivated.RemoveListener(UI_Act);
        }

        void UI_Act()
        {
            ui.SetActive(false);
        }
    }
}
