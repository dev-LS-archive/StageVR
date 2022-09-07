using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.TechDemo.Scripts;
using UnityEngine;

namespace Dev_LSG.Scripts.Event
{
    [RequireComponent(typeof(DemoCodeGrabbing))]
    public class MenuButtonActive : MonoBehaviour
    {
        public GameObject menu;
        public Transform uiPos;
        private DemoCodeGrabbing _grabbing;

        private void Awake()
        {
            if (_grabbing == null)
                _grabbing = GetComponent<DemoCodeGrabbing>();
        }

        private void OnEnable()
        {
            HVRControllerEvents.Instance.LeftMenuActivated.AddListener(MenuAct);
        }

        private void OnDisable()
        {
            HVRControllerEvents.Instance.LeftMenuActivated.RemoveListener(MenuAct);
        }

        void MenuAct()
        {
            menu.transform.position = uiPos.position;
            menu.transform.rotation = uiPos.rotation;

            if (menu.activeSelf)
            {
                _grabbing.Grab();
            }
            
            menu.gameObject.SetActive(menu.activeSelf != true);
        }
    }
}
