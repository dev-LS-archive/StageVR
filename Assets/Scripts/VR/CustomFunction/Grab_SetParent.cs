using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Grab_SetParent : MonoBehaviour
{
    [SerializeField] private Transform parentTrans;
    private XRBaseInteractable xRBase = null;

    void Awake()
    {
        xRBase = GetComponent<XRBaseInteractable>();
    }
    protected virtual void OnEnable()
    {
        xRBase.selectExited.AddListener(Set_Parent);
    }

    protected virtual void OnDisable()
    {
        xRBase.selectExited.RemoveListener(Set_Parent);
    }

    void Set_Parent(SelectExitEventArgs args)
    {
        transform.SetParent(parentTrans);
    }
}
