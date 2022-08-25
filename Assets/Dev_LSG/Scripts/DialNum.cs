using HurricaneVR.Framework.Components;
using TMPro;

namespace Dev_LSG.Scripts
{
    public class DialNum : HVRRotationTracker
    {
        public TextMeshProUGUI numberLabel;
        public int currentNumber;
        
        protected override void OnStepChanged(int step, bool raiseEvents)
        {
            base.OnStepChanged(step, raiseEvents);

            currentNumber = (int) (step*StepSize);

            if (numberLabel)
            {
                numberLabel.text = currentNumber.ToString("n0");
            }
        }
    }
}
