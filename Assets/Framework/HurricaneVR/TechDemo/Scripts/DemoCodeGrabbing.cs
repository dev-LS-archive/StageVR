 using System.Linq;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.HandPoser;
using HurricaneVR.Framework.Shared;
using UnityEngine;

namespace HurricaneVR.TechDemo.Scripts
{
    public class DemoCodeGrabbing : MonoBehaviour
    {
        public HVRHandGrabber Grabber { get; set; }
        public HVRHandGrabber GrabberSet;
        public HVRGrabbable Grabbable;
        public HVRGrabTrigger GrabTrigger;
        public HVRPosableGrabPoint GrabPoint;

        public void Start()
        {
            if (GrabberSet != null)
            {
                Grabber = GrabberSet;
            }
            else
            {
                Grabber = GameObject.FindObjectsOfType<HVRHandGrabber>().FirstOrDefault(e => e.gameObject.activeInHierarchy);
            }
        }

        void OnEnable()
        {
            Grabbable.Released.AddListener(CallGrab);
        }

        private void CallGrab(HVRGrabberBase arg0, HVRGrabbable arg1)
        {
            Grab();
        }
        
        public void Grab()
        {
            if (Grabbable && Grabber)
            {
                if (GrabTrigger == HVRGrabTrigger.ManualRelease && Grabber.GrabbedTarget == Grabbable)
                {
                    Grabber.ForceRelease();
                    return;
                }

                //grabber needs to have it's release sequence completed if it's holding something
                if(Grabber.IsGrabbing)
                    Grabber.ForceRelease();
                Grabber.Grab(Grabbable, GrabTrigger, GrabPoint);
            }
        }

        public void ResetEvent()
        {
            Grabbable.Released.RemoveAllListeners();
            Grab();
        }
    }
}
