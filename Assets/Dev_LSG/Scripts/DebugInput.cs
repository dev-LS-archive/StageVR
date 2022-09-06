using HurricaneVR.Framework.ControllerInput;
using UnityEngine;

namespace Dev_LSG.Scripts
{
    public class DebugInput : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            if (HVRInputSystemController.InputActions.LeftHand.Menu.IsPressed())
            {
                print("act");
            }
            if (HVRInputSystemController.InputActions.LeftHand.PrimaryButton.IsPressed())
            {
                print("act1");
            }
        }
    }
}
