using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class XR_InputValue : MonoBehaviour
{
    [SerializeField]
    private XRNode xRNode = XRNode.LeftHand;

    private List<InputDevice> devices = new List<InputDevice>();

    private InputDevice device;

    //to avoid repeat readings
    private bool triggerIsPressed;
    private bool primaryButtonIsPressed;
    private bool primary2DAxisIsChosen;
    private Vector2 primary2DAxisValue = Vector2.zero;
    private Vector2 prevPrimary2DAxisValue;
    private bool gripIsPressed;

    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(xRNode, devices);
        device = devices.FirstOrDefault();
    }

    void OnEnable()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
    }

    void Update()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
        // capturing grip value
        float triggerValue;
        InputFeatureUsage<float> triggerUsage = CommonUsages.trigger;
        if (device.TryGetFeatureValue(triggerUsage, out triggerValue))
        {
            Debug.Log($"Trigger value {triggerValue}");
        }

        // capturing trigger button press and release    
        bool triggerButtonValue = false;
        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonValue) && triggerButtonValue && !triggerIsPressed)
        {
            triggerIsPressed = true;
            Debug.Log($"TriggerButton activated {triggerButtonValue} on {xRNode}");
        }
        else if (!triggerButtonValue && triggerIsPressed)
        {
            triggerIsPressed = false;
            Debug.Log($"TriggerButton deactivated {triggerButtonValue} on {xRNode}");
        }

        // capturing primary button press and release
        bool primaryButtonValue = false;
        InputFeatureUsage<bool> primaryButtonUsage = CommonUsages.primaryButton;

        if (device.TryGetFeatureValue(primaryButtonUsage, out primaryButtonValue) && primaryButtonValue && !primaryButtonIsPressed)
        {
            primaryButtonIsPressed = true;
            Debug.Log($"PrimaryButton activated {primaryButtonValue} on {xRNode}");
        }
        else if (!primaryButtonValue && primaryButtonIsPressed)
        {
            primaryButtonIsPressed = false;
            Debug.Log($"PrimaryButton deactivated {primaryButtonValue} on {xRNode}");
        }

        // capturing primary 2D Axis changes and release
        InputFeatureUsage<Vector2> primary2DAxisUsage = CommonUsages.primary2DAxis;
        // make sure the value is not zero and that it has changed
        if (primary2DAxisValue != prevPrimary2DAxisValue)
        {
            primary2DAxisIsChosen = false;
            //Debug.Log($"CHANGED and prev value is {prevPrimary2DAxisValue} and the new value is {primary2DAxisValue}");
        }
        // was for checking to see if the axis values were reading as changed properly
        /* else
        {
            Debug.Log($"Nope, prev value is {prevPrimary2DAxisValue} and the new value is {primary2DAxisValue}");
        } */
        if (device.TryGetFeatureValue(primary2DAxisUsage, out primary2DAxisValue) && primary2DAxisValue != Vector2.zero && !primary2DAxisIsChosen)
        {
            prevPrimary2DAxisValue = primary2DAxisValue;
            primary2DAxisIsChosen = true;
            Debug.Log($"Primary2DAxis value activated {primary2DAxisValue} on {xRNode}");
        }
        else if (primary2DAxisValue == Vector2.zero && primary2DAxisIsChosen)
        {
            prevPrimary2DAxisValue = primary2DAxisValue;
            primary2DAxisIsChosen = false;
            Debug.Log($"Primary2DAxis deactivated {primary2DAxisValue} on {xRNode}");
        }

        // capturing grip value
        float gripValue;
        InputFeatureUsage<float> gripUsage = CommonUsages.grip;
        if(device.TryGetFeatureValue(gripUsage, out gripValue)){
            Debug.Log($"Grip value {gripValue}");
        }
        if (device.TryGetFeatureValue(gripUsage, out gripValue) && gripValue > 0 && !gripIsPressed)
        {
            gripIsPressed = true;
            Debug.Log($"Grip value {gripValue} activated on {xRNode}");
        }
        else if (gripValue == 0 && gripIsPressed)
        {
            gripIsPressed = false;
            Debug.Log($"Grip value {gripValue} deactivated on {xRNode}");
        }
    }
}
