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
            var amp = Data.AmpCurve.Evaluate(Force / Data.MaxForce);

            lHand.Controller.Vibrate(amp, duration, Data.Frequency);
            rHand.Controller.Vibrate(amp, duration, Data.Frequency);
        }
        
        public void CallLeftVibrate(float dur)
        {
            var amp = Data.AmpCurve.Evaluate(Force / Data.MaxForce);

            lHand.Controller.Vibrate(amp, dur, Data.Frequency);
        }
        public void CallRightVibrate(float dur)
        {
            var amp = Data.AmpCurve.Evaluate(Force / Data.MaxForce);
            
            rHand.Controller.Vibrate(amp, dur, Data.Frequency);
        }
        
        public void CallLrVibrate(float dur)
        {
            var amp = Data.AmpCurve.Evaluate(Force / Data.MaxForce);

            lHand.Controller.Vibrate(amp, dur, Data.Frequency);
            rHand.Controller.Vibrate(amp, dur, Data.Frequency);
        }
    }
}
