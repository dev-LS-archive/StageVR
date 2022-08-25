using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class XR_Hand_Value : MonoBehaviour
{
    #region XR_inputValue
    [SerializeField]
    private XRNode xRNode = XRNode.LeftHand;

    private List<InputDevice> devices = new List<InputDevice>();

    private InputDevice device;
    #endregion
    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(xRNode, devices);
        device = devices.FirstOrDefault();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
