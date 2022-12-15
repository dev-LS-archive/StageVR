﻿using HurricaneVR.Framework.ControllerInput;
using HurricaneVR.Framework.Shared;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HurricaneVR.Framework.Core.UI
{
    [RequireComponent(typeof(LineRenderer))]
    [RequireComponent(typeof(Camera))]
    public class HVRUIPointer : MonoBehaviour
    {
        public bool customView = false;
        public bool ViewOnEvent { get; set; } //custom
        
        public HVRHandSide HandSide;

        public HVRInputModule InputModule;
        public Camera Camera { get; private set; }
        public LineRenderer Pointer { get; private set; }

        public PointerEventData PointerEventData { get; internal set; }

        public HVRController Controller => HVRInputManager.Instance.GetController(HandSide);
        public GameObject CurrentUIElement;// { get; internal set; }

        public HVRUIPointer(bool viewOnEvent)
        {
            ViewOnEvent = viewOnEvent;
        }//Custom

        protected virtual void Start()
        {
            Camera = GetComponent<Camera>();
            Pointer = GetComponent<LineRenderer>();
        }
        
        protected virtual void Update()
        {
            // if(!ViewOnEvent) //custom
            //     Pointer.enabled = CurrentUIElement;
            Pointer.enabled = !customView ? CurrentUIElement : ViewOnEvent;

            if (Pointer.enabled)
            {
                Pointer.SetPosition(0, transform.position);
                Pointer.SetPosition(1, PointerEventData.pointerCurrentRaycast.worldPosition);
            }
        }

        public virtual void Process()
        {
            PointerEventData.Reset();
            PointerEventData.position = new Vector2(Camera.pixelWidth / 2, Camera.pixelHeight / 2);
            PointerEventData.scrollDelta = Vector2.zero;
        }
    }
}