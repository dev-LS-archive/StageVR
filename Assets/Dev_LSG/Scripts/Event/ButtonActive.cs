using HurricaneVR.Framework.ControllerInput;
using UnityEngine;

namespace Dev_LSG.Scripts.Event
{
    [RequireComponent(typeof(HVRControllerEvents))]
    public class ButtonActive : MonoBehaviour
    {
        public GameObject menu;
        public Transform uiPos;
        private HVRControllerEvents events;

        private void Awake()
        {
            if (events == null)
                events = GetComponent<HVRControllerEvents>();
        }

        private void OnEnable()
        {
            events.LeftMenuActivated.AddListener(MenuAct);
        }

        private void OnDisable()
        {
            events.LeftMenuActivated.RemoveListener(MenuAct);
        }

        void MenuAct()
        {
            menu.transform.position = uiPos.position;
            menu.transform.rotation = uiPos.rotation;
            
            menu.gameObject.SetActive(menu.activeSelf != true);
        }
    }
}
