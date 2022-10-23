using HurricaneVR.Framework.ControllerInput;
using UnityEngine;

namespace Dev_LSG.Scripts.Event
{
    public class TriggerUI : MonoBehaviour
    {
        public GameObject ui;

        private void OnEnable()
        {
            HVRControllerEvents.Instance.LeftTriggerActivated.AddListener(UI_Act);
            HVRControllerEvents.Instance.RightTriggerActivated.AddListener(UI_Act);
        }

        private void OnDisable()
        {
            HVRControllerEvents.Instance.LeftTriggerActivated.RemoveListener(UI_Act);
            HVRControllerEvents.Instance.RightTriggerActivated.RemoveListener(UI_Act);
        }

        void UI_Act()
        {
            ui.SetActive(false);
        }
    }
}
