using System.Linq;
using HurricaneVR.Framework.Core;
using HurricaneVR.Framework.Core.Grabbers;
using HurricaneVR.Framework.Core.HandPoser;
using HurricaneVR.Framework.Shared;
using UnityEngine;

namespace HurricaneVR.TechDemo.Scripts
{
    public class CustomCodeGrabbing : MonoBehaviour
    {
        public HVRHandGrabber Grabber;
        public HVRGrabbable Grabbable;
        public HVRGrabTrigger GrabTrigger;
        public HVRPosableGrabPoint GrabPoint;

        private void CallGrab(HVRGrabberBase arg0, HVRGrabbable arg1)
        {
            Grab();
        }
        
        public void Grab()
        {
            Grabber.Grab(Grabbable, GrabTrigger, GrabPoint);
        }

        public void ResetEvent()
        {
            Grabber.ForceRelease();
        }
    }
}