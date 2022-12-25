using HurricaneVR.Framework.Components;
using UnityEngine;
using HurricaneVR.Framework.Core.Grabbers;

namespace Dev_LSG.Scripts.Player
{
    public class CallVirate : HVRImpactHapticsBase
    {
        public HVRHandGrabber lHand;
        public HVRHandGrabber rHand;
        public bool HandGrabbingPrevents = true;

        public float duration, amplitude, frequency;

        [ContextMenu("Vibrate")]
        public void CallVibrate()
        {
            if (HandGrabbingPrevents && lHand.IsGrabbing) return;
            if (HandGrabbingPrevents && rHand.IsGrabbing) return;
            var amp = Data.AmpCurve.Evaluate(Force / Data.MaxForce);

            lHand.Controller.Vibrate(amp, duration, Data.Frequency);
            rHand.Controller.Vibrate(amp, duration, Data.Frequency);
        }
    }
}
