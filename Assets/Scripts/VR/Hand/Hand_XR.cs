using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UniRx;

[RequireComponent(typeof(Animator))]
public class Hand_XR : MonoBehaviour
{
    public float speed;

    ActionBasedController controller;
    [SerializeField]
    private Animator m_animator = null;
    private float gripTarget;
    private float triggerTarget;
    private float gripCurrent;
    private float triggerCurrent;
    private string animatorGripParm = "Grip";
    private string animatorTriggerParm = "Trigger";

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AnimateHand();
    }

    internal void SetGrip(float v)
    {
        gripTarget = v;
    }

    internal void SetTrigger(float v)
    {
        triggerTarget = v;
    }
    void AnimateHand()
    {
        if (gripCurrent != gripTarget)
        {
            gripCurrent = Mathf.MoveTowards(gripCurrent, gripTarget, Time.deltaTime * speed);
            m_animator.SetFloat(animatorGripParm, gripCurrent);
        }
        if (triggerCurrent != triggerTarget)
        {
            triggerCurrent = Mathf.MoveTowards(triggerCurrent, triggerTarget, Time.deltaTime * speed);
            m_animator.SetFloat(animatorTriggerParm, triggerCurrent);
        }
    }

}
